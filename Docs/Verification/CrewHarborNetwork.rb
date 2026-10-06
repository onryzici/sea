# Real Editor host + newly built macOS client. Probe supplies input; not native keyboard automation.
require 'json'
require 'tmpdir'
require 'open3'
require 'fileutils'
root=Dir.pwd;temp=Dir.mktmpdir('salvage-crew.')
host=File.join(temp,'host');client=File.join(temp,'client');[host,client].each{|d|FileUtils.mkdir_p(d)}
checks={};evidence={};seq=Hash.new(0);pid=nil
def state(p)
 JSON.parse(File.read(File.join(p,'state.json')))
rescue Errno::ENOENT,JSON::ParserError
 nil
end
def wait(seconds=25)
 deadline=Process.clock_gettime(Process::CLOCK_MONOTONIC)+seconds
 loop do
  v=yield;return v if v
  raise 'State timeout' if Process.clock_gettime(Process::CLOCK_MONOTONIC)>deadline
  sleep 0.15
 end
end
def editor(code)
 out,status=Open3.capture2('unity','command','eval','--caller','plugin','--skill','unity-cli','--json','--','--code',code,'--timeout','30000')
 j=JSON.parse(out);raise out unless status.success?&&j.dig('data','result','success');j.dig('data','result','result')
end
command=lambda do |dir,action,extra={}|
 seq[dir]+=1
 File.write(File.join(dir,'pending.json'),JSON.generate({seq:seq[dir],action:action}.merge(extra)))
 File.rename(File.join(dir,'pending.json'),File.join(dir,'command.json'))
 wait{v=state(dir);v if v&&v['sequence']==seq[dir]}
end
none=18446744073709551615
begin
 editor("foreach(var old in UnityEngine.Object.FindObjectsByType<SalvageCrew.NetworkTestProbe>())if(old.name==\"CrewHarborProbe\")UnityEngine.Object.Destroy(old.gameObject);new UnityEngine.GameObject(\"CrewHarborProbe\").AddComponent<SalvageCrew.NetworkTestProbe>().Configure(\"#{host}\");return true;")
 command.call(host,'host');wait{state(host)['role']=='HOST'}
 pid=Process.spawn(File.join(root,'Builds/CrewHarbor/macOS/SalvageCrew.app/Contents/MacOS/SalvageCrew'),'--salvage-probe',client,'-logFile',File.join(temp,'client.log'),'-screen-width','1280','-screen-height','720','-windowed',out:File::NULL,err:File::NULL)
 wait(40){state(client)};command.call(client,'client');wait{state(client)['players'].length==2}
 command.call(host,'resume');command.call(client,'resume')
 wait{state(host)['crewRoster'].include?('TAYFA')&&state(client)['crewRoster'].include?('KAPTAN')}
 checks['realTwoInstanceRoster']=true
 command.call(client,'walk',{x:0,z:8.5})
 wait{state(host)['players'].any?{|p|!p['owner']&&p['visualSpeed']>1&&p['animatorReady']}}
 checks['remoteWalkAnimationActive']=true;evidence['walking']=state(host)
 wait{state(client)['result']=='Reached'};sleep 1
 checks['remoteIdleAfterStop']=state(host)['players'].find{|p|!p['owner']}['visualSpeed']<0.15
 # Keep one player on the pier while the host takes the existing ramp to the deck.
 [[0,7.5],[6.6,7.5]].each{|x,z|command.call(host,'walk',{x:x,z:z});wait{state(host)['result']=='Reached'}}
 checks['hostRampStillTraversable']=state(host)['players'].find{|p|p['owner']}['supported']
 [[0,-1],[0,1.35]].each{|x,z|command.call(host,'walkBoat',{x:x,z:z});wait{state(host)['result']=='Reached'}}
 command.call(host,'lookHelm');sleep 0.4;command.call(host,'toggle')
 wait{state(host)['crewRoster'].include?('DÜMENDE')&&state(client)['crewRoster'].include?('DÜMENDE')}
 checks['helmStateVisibleBothInstances']=true
 command.call(host,'toggle');command.call(host,'walkBoat',{x:-0.5,z:-2.5});wait{state(host)['result']=='Reached'}
 crate=state(host)['items'].find{|i|i['mass']==12&&!i['name'].start_with?('Wreck')}
 # Fixture moves the crate into reach. Pickup and simulation still use server validation/forces.
 editor("var item=UnityEngine.Object.FindObjectsByType<SalvageCrew.NetworkScrap>().First(s=>s.NetworkObjectId==#{crate['id']}ul);item.Item.Body.position=new UnityEngine.Vector3(.75f,1.9f,8.8f);item.Item.Body.linearVelocity=UnityEngine.Vector3.zero;return true;")
 sleep 2;command.call(client,'look',{mass:12,target:crate['id'].to_s});sleep 0.4;command.call(client,'toggle')
 wait{state(host)['items'].any?{|i|i['id']==crate['id']&&i['holder']!=none}}
 wait{state(host)['crewRoster'].include?('TAŞIYOR')&&state(client)['crewRoster'].include?('TAŞIYOR')}
 checks['carryStateVisibleBothInstances']=true
 editor('var p=SalvageCrew.HarborSession.Instance.LocalPlayer;var other=UnityEngine.Object.FindObjectsByType<SalvageCrew.NetworkCrewPlayer>().First(x=>!x.IsOwner);p.Motor.enabled=false;var d=other.transform.position-p.transform.position;p.transform.rotation=UnityEngine.Quaternion.LookRotation(UnityEngine.Vector3.ProjectOnPlane(d,UnityEngine.Vector3.up));p.Carry.View.localRotation=UnityEngine.Quaternion.identity;return true;')
 sleep 0.5
 system('unity','command','capture_game_view','--caller','plugin','--skill','unity-cli','--json','--','--source','screen','--width','1920','--height','1080','--save_path','Docs/Screenshots/CrewTogether.png',out:File::NULL)
 evidence['carrying']={host:state(host),client:state(client)}
 checks['remoteMarkerVisible']=editor('return UnityEngine.GameObject.Find("CrewMarker").activeInHierarchy;')
 command.call(client,'disconnect');wait{state(host)['players'].length==1&&!state(host)['crewRoster'].include?('TAYFA')}
 checks['disconnectRemovesRosterAndDropsCargo']=state(host)['items'].find{|i|i['id']==crate['id']}['holder']==none
 command.call(client,'client');wait{state(client)['players'].length==2&&state(host)['crewRoster'].include?('TAYFA')}
 checks['reconnectNoDuplicatePlayers']=state(host)['players'].length==2
 checks['noRuntimeExceptions']=[host,client].all?{|p|state(p)['errors']==0}
 evidence['final']={host:state(host),client:state(client)}
rescue=>e
 evidence['failure']={message:e.message,host:state(host),client:state(client)}
ensure
 begin;command.call(host,'disconnect');rescue;end
 Process.kill('TERM',pid) rescue nil if pid
 File.write(File.join(root,'Docs/Verification/CrewHarborNetworkResults.json'),JSON.pretty_generate({checks:checks,evidence:evidence,temp:temp}))
 puts JSON.pretty_generate({checks:checks,failure:evidence['failure']&&evidence['failure'][:message],temp:temp})
end
