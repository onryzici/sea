# Real Editor host + development macOS client. Uses the existing opt-in probe; no mocked network.
require 'json'
require 'tmpdir'
require 'open3'
require 'fileutils'
root = Dir.pwd
temp = Dir.mktmpdir('salvage-art-network.')
host = File.join(temp, 'host'); client = File.join(temp, 'client')
[host, client].each { |p| FileUtils.mkdir_p(p) }
checks = {}; evidence = {}; sequences = Hash.new(0)
cli = ['unity', 'command', '--caller', 'plugin', '--skill', 'unity-cli', '--result-only', '--json']
def state(path)
  JSON.parse(File.read(File.join(path, 'state.json')))
rescue Errno::ENOENT, JSON::ParserError
  nil
end
def wait_for(seconds = 12)
  finish = Process.clock_gettime(Process::CLOCK_MONOTONIC) + seconds
  loop do
    value = yield
    return value if value
    raise 'Timed out waiting for real instance state' if Process.clock_gettime(Process::CLOCK_MONOTONIC) > finish
    sleep 0.15
  end
end
send_command = lambda do |path, action, extra = {}|
  sequences[path] += 1
  data = { seq: sequences[path], action: action }.merge(extra)
  File.write(File.join(path, 'pending.json'), JSON.generate(data))
  File.rename(File.join(path, 'pending.json'), File.join(path, 'command.json'))
  wait_for { s = state(path); s if s && s['sequence'] == sequences[path] }
end
walk = lambda do |path, x, z, boat = false|
  send_command.call(path, boat ? 'walkBoat' : 'walk', { x: x, z: z })
  s = wait_for(23) { t = state(path); t if t && t['result'] == 'Reached' }
  s
end
owned = ->(s) { s['players'].find { |p| p['owner'] } }
none = 18_446_744_073_709_551_615
pid = nil
begin
  # Editor Play is already running when this script is invoked.
  output, status = Open3.capture2(*cli, 'eval', '--code', "new UnityEngine.GameObject(\"ArtNetworkProbe\").AddComponent<SalvageCrew.NetworkTestProbe>().Configure(\"#{host}\"); return true;")
  raise output unless status.success?
  send_command.call(host, 'host')
  wait_for { s = state(host); s if s && s['role'] == 'HOST' }
  binary = File.join(root, ARGV[0] || 'Builds/ArtPass/macOS/SalvageCrew.app/Contents/MacOS/SalvageCrew')
  pid = Process.spawn(binary, '--salvage-probe', client, '-logFile', File.join(temp, 'client.log'), '-screen-width', '1024', '-screen-height', '576', '-windowed', out: File::NULL, err: File::NULL)
  wait_for(25) { state(client) }
  send_command.call(client, 'client')
  wait_for(25) { s = state(client); s if s && s['role'] == 'CLIENT' && s['players'].length == 2 && s['items'].length == 3 }
  checks['TwoRealInstances'] = true
  [host, client].each { |p| send_command.call(p, 'resume') }
  checks['LocalOnlyCameraInput'] = [host, client].all? { |p| state(p)['players'].count { |x| x['camera'] && x['input'] } == 1 }
  walk.call(host, 0.25, 4)
  send_command.call(host, 'look', { mass: 12 }); send_command.call(host, 'toggle')
  wait_for { owned.call(state(host))['held'] != none }
  walk.call(host, 0.25, 7.5); walk.call(host, 6.25, 7.5)
  sleep 0.6
  h = state(host); c = state(client)
  a = h['items'].find { |i| i['mass'] == 12 }; b = c['items'].find { |i| i['mass'] == 12 }
  distance = Math.sqrt(%w[x y z].sum { |key| (a['position'][key] - b['position'][key])**2 })
  checks['HostCarryVisibleInClientState'] = a['holder'] == 0 && b['holder'] == 0 && distance < 0.35
  evidence['HostCarry'] = { distance: distance, host: h, client: c }
  send_command.call(host, 'toggle'); walk.call(host, 6.1, 9)
  walk.call(client, 0.25, 2)
  send_command.call(client, 'look', { mass: 3 }); send_command.call(client, 'toggle')
  wait_for { owned.call(state(client))['held'] != none }
  walk.call(client, 0.25, 7.5); walk.call(client, 6.25, 7.5)
  sleep 0.6
  checks['ClientCarryVisibleInHostState'] = state(host)['items'].any? { |i| i['mass'] == 3 && i['holder'] == 1 && i['position']['x'] > 5 }
  evidence['ClientCarry'] = { host: state(host), client: state(client) }
  send_command.call(client, 'toggle'); walk.call(client, 6, 8.2)
  walk.call(host, 7, 11.8)
  send_command.call(host, 'lookHelm'); send_command.call(host, 'toggle')
  wait_for { owned.call(state(host))['driving'] }
  before = state(client); send_command.call(host, 'drive', { x: 0.35, z: 1, duration: 4 })
  sleep 2
  walk.call(client, -1, -3.2, true)
  after = state(client)
  # Use deck-local height: hull draft/heave deliberately changes the world-space waterline.
  checks['MovingDeckClientWalk'] = after['boat']['departed'] && owned.call(after)['supported'] && owned.call(after)['boatLocal']['y'].between?(1.4, 1.85)
  checks['ServerBoatClientKinematic'] = state(host)['boat']['kinematic'] == false && after['boat']['kinematic'] == true
  checks['CargoRemainsFinite'] = state(host)['items'].all? { |i| i['position'].values.all?(&:finite?) && Math.sqrt(i['velocity'].values.sum { |v| v*v }) < 8 }
  evidence['MovingDeck'] = { before: before, after: after, host: state(host) }
  send_command.call(host, 'toggle')
  walk.call(host, 0.8, -3.4, true)
  walk.call(client, 0, -1, true)
  walk.call(client, 0, 1.8, true)
  send_command.call(client, 'lookHelm'); send_command.call(client, 'toggle')
  wait_for { owned.call(state(client))['driving'] }
  before_client_drive = state(host)['boat']['position']
  send_command.call(client, 'drive', { x: -0.3, z: 1, duration: 6 })
  sleep 2
  walk.call(host, 0.8, -3.4, true)
  after_client_drive = state(host)
  moved = Math.sqrt(%w[x z].sum { |axis| (after_client_drive['boat']['position'][axis] - before_client_drive[axis])**2 })
  checks['ClientHelmHostPassenger'] = moved > 0.5 && owned.call(after_client_drive)['supported'] && state(host)['boat']['driver'] == 1
  evidence['ClientHelm'] = { metres: moved, host: state(host), client: state(client) }
  send_command.call(client, 'disconnect')
  wait_for { state(host)['players'].length == 1 }
  wait_for { state(host)['boat']['driver'] == none }
  checks['DisconnectReleasesHelm'] = state(host)['boat']['drive'].values.all? { |v| v.abs < 0.01 }
  send_command.call(client, 'client')
  wait_for(25) { s = state(client); s if s && s['role'] == 'CLIENT' && s['players'].length == 2 && s['items'].length == 3 }
  sleep 1
  h = state(host); c = state(client)
  checks['LateRejoinState'] = c['boat']['departed'] && c['items'].all? do |item|
    authoritative = h['items'].find { |v| v['mass'] == item['mass'] }
    authoritative && authoritative['holder'] == item['holder'] && Math.sqrt(%w[x y z].sum { |axis| (authoritative['position'][axis] - item['position'][axis])**2 }) < 0.6
  end
  evidence['LateRejoin'] = { host: h, client: c }
  send_command.call(client, 'disconnect')
  wait_for { state(host)['players'].length == 1 }
  checks['DisconnectClean'] = state(host)['errors'] == 0 && state(client)['errors'] == 0
  evidence['Final'] = { host: state(host), client: state(client) }
rescue => error
  checks['Completed'] = false
  evidence['Failure'] = { message: error.message, host: state(host), client: state(client) }
ensure
  File.write(File.join(root, ARGV[1] || 'Docs/Verification/ArtNetworkResults.json'), JSON.pretty_generate({ checks: checks, evidence: evidence, temp: temp }))
  puts JSON.pretty_generate(checks)
  begin
    send_command.call(host, 'disconnect') if state(host) && state(host)['role'] == 'HOST'
  rescue => error
    warn "Host cleanup: #{error.message}"
  end
  if pid
    Process.kill('TERM', pid) rescue nil
    Process.wait(pid) rescue nil
  end
end
exit(checks.values.all? ? 0 : 1)
