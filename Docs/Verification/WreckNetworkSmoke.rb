# Real Editor host + macOS development client. Fixture positioning is not a manual voyage.
require 'json'
require 'tmpdir'
require 'open3'
require 'fileutils'
root=Dir.pwd
temp=Dir.mktmpdir('salvage-wreck-network.')
host=File.join(temp,'host');client=File.join(temp,'client')
[host,client].each{|p|FileUtils.mkdir_p(p)}
checks={};evidence={};seq=Hash.new(0);pid=nil
def read_state(p)
 JSON.parse(File.read(File.join(p,'state.json')))
rescue Errno::ENOENT,JSON::ParserError
 nil
end
def wait_state(seconds=15)
 until_at=Process.clock_gettime(Process::CLOCK_MONOTONIC)+seconds
 loop do
  v=yield;return v if v
  raise 'Timed out waiting for instance state' if Process.clock_gettime(Process::CLOCK_MONOTONIC)>until_at
  sleep 0.15
 end
end
def editor(code)
 output,status=Open3.capture2('unity','command','eval','--caller','plugin','--skill','unity-cli','--json','--','--code',code,'--timeout','30000')
 result=JSON.parse(output)
 raise output unless status.success? && result.dig('data','result','success')
 result.dig('data','result','result')
end
command=lambda do |path,action,extra={}|
 seq[path]+=1
 File.write(File.join(path,'pending.json'),JSON.generate({seq:seq[path],action:action}.merge(extra)))
 File.rename(File.join(path,'pending.json'),File.join(path,'command.json'))
 wait_state{v=read_state(path);v if v&&v['sequence']==seq[path]}
end
none=18446744073709551615
begin
 editor("foreach(var probe in UnityEngine.Object.FindObjectsByType<SalvageCrew.NetworkTestProbe>())if(probe.name==\"WreckNetworkProbe\")UnityEngine.Object.Destroy(probe.gameObject);new UnityEngine.GameObject(\"WreckNetworkProbe\").AddComponent<SalvageCrew.NetworkTestProbe>().Configure(\"#{host}\");return true;")
 command.call(host,'host')
 wait_state{v=read_state(host);v if v&&v['role']=='HOST'&&v['items'].length==9}
 # Keep one mission crate per mass for deterministic existing probe targeting; pier training cargo and site 2 remain in the shipped scene.
 editor('var m=SalvageCrew.WreckExpedition.Instance;var keep=m.Cargo.Take(3).ToArray();foreach(var s in UnityEngine.Object.FindObjectsByType<SalvageCrew.NetworkScrap>())if(!keep.Contains(s.Item))s.NetworkObject.Despawn();var b=SalvageCrew.NetworkBoat.Instance;b.Departed.Value=true;b.Body.constraints=UnityEngine.RigidbodyConstraints.None;b.Body.position=new UnityEngine.Vector3(24.5f,b.Body.position.y,42);b.transform.position=b.Body.position;b.Body.linearVelocity=UnityEngine.Vector3.zero;var p=SalvageCrew.HarborSession.Instance.LocalPlayer;p.Motor.ReturnToSpawn();return true;')
 command.call(host,'scan')
 wait_state{read_state(host)['wreckMask']==3}
 binary=File.join(root,'Builds/WreckSearch/macOS/SalvageCrew.app/Contents/MacOS/SalvageCrew')
 pid=Process.spawn(binary,'--salvage-probe',client,'-logFile',File.join(temp,'client.log'),'-screen-width','1280','-screen-height','720','-windowed',out:File::NULL,err:File::NULL)
 wait_state(35){read_state(client)}
 command.call(client,'client')
 wait_state(30){v=read_state(client);v if v&&v['role']=='CLIENT'&&v['players'].length==2&&v['items'].length==3}
 checks['twoRealInstances']=true
 client_id=read_state(client)['players'].find{|p|p['owner']}['client']
 checks['lateJoinGetsDiscovery']=read_state(client)['wreckMask']==3
 checks['clientCargoKinematic']=read_state(client)['items'].all?{|x|x['kinematic']&&!x['recovery']}
 checks['hostCargoDynamic']=read_state(host)['items'].all?{|x|!x['kinematic']&&x['recovery']}
 command.call(client,'resume');command.call(host,'resume')
 command.call(client,'scan')
 wait_state{read_state(client)['sonarFeedback'].to_s.length>0}
 checks['clientScanServerReply']=read_state(client)['sonarFeedback'].include?('sinyal')||read_state(client)['sonarFeedback'].include?('hazırlanıyor')
 [[0,-1.0],[0,1.35]].each do |x,z|
  command.call(client,'walkBoat',{x:x,z:z});wait_state(24){read_state(client)['result']=='Reached'}
 end
 command.call(client,'lookHelm');sleep 0.4;command.call(client,'toggle')
 wait_state{read_state(host)['boat']['driver']==client_id}
 before=read_state(host)['boat']['position']
 command.call(client,'drive',{x:0.3,z:1,duration:3});sleep 3.5
 after=read_state(host)['boat']['position']
 checks['clientUsesNewHelmAndDrives']=Math.sqrt(%w[x z].sum{|k|(after[k]-before[k])**2})>0.4
 command.call(client,'toggle');wait_state{read_state(host)['boat']['driver']==none}
 command.call(client,'walkBoat',{x:0,z:-2.0});wait_state(24){read_state(client)['result']=='Reached'}
 # Put a real mission crate in reach on deck; the client still uses the validated carry RPC and host spring.
 editor('var p=UnityEngine.Object.FindObjectsByType<SalvageCrew.NetworkCrewPlayer>().First(x=>x.OwnerClientId!=0);var b=SalvageCrew.NetworkBoat.Instance;var item=SalvageCrew.WreckExpedition.Instance.Cargo[1];item.Body.position=p.transform.position+p.transform.forward*.9f+UnityEngine.Vector3.up*.55f;item.Body.linearVelocity=UnityEngine.Vector3.zero;return true;')
 sleep 0.5
 command.call(client,'look',{mass:12});sleep 0.4;command.call(client,'toggle')
 wait_state{read_state(host)['items'].any?{|x|x['mass']==12&&x['holder']!=none}}
 checks['clientCarriesWreckCargo']=read_state(host)['items'].any?{|x|x['mass']==12&&x['holder']==client_id}
 evidence['carrying']={host:read_state(host),client:read_state(client)}
 command.call(client,'disconnect')
 wait_state{read_state(host)['players'].length==1&&read_state(host)['items'].all?{|x|x['holder']==none}}
 checks['disconnectReleasesMissionCargo']=true
 wait_state{read_state(client)['role']=='SOLO'}
 command.call(client,'client')
 wait_state(25){read_state(client)['role']=='CLIENT'&&read_state(client)['items'].length==3}
 checks['reconnectNoDuplicateNetworkCargo']=read_state(host)['items'].length==3&&read_state(host)['players'].length==2
 checks['reconnectDiscoveryPersists']=read_state(client)['wreckMask']==3
 evidence['reconnected']={host:read_state(host),client:read_state(client)}
 editor('var b=SalvageCrew.NetworkBoat.Instance;b.Body.position=new UnityEngine.Vector3(7,b.Body.position.y,10);b.Body.rotation=UnityEngine.Quaternion.identity;b.Body.linearVelocity=UnityEngine.Vector3.zero;b.Body.angularVelocity=UnityEngine.Vector3.zero;b.transform.SetPositionAndRotation(b.Body.position,b.Body.rotation);var m=SalvageCrew.WreckExpedition.Instance;for(int i=0;i<3;i++){var item=m.Cargo[i];item.Body.position=b.transform.TransformPoint(new UnityEngine.Vector3(-1.05f+i*1.05f,2.2f,-2.4f));item.Body.linearVelocity=UnityEngine.Vector3.zero;item.Body.angularVelocity=UnityEngine.Vector3.zero;}return true;')
 wait_state{read_state(host)['expeditionComplete']&&read_state(client)['expeditionComplete']}
 checks['sharedCargoCountAndCompletion']=read_state(client)['securedCargo']==3
 command.call(host,'disconnect')
 wait_state{read_state(client)['role']=='SOLO'}
 checks['hostShutdownRestoresOffline']=true
 checks['noProbeRuntimeErrors']=[host,client].all?{|p|read_state(p)['errors']==0}
rescue=>e
 evidence['failure']={message:e.message,backtrace:e.backtrace.first(5),host:read_state(host),client:read_state(client)}
ensure
 begin;command.call(host,'disconnect');rescue;end
 if pid;Process.kill('TERM',pid) rescue nil;end
 File.write(File.join(root,'Docs/Verification/WreckNetworkResults.json'),JSON.pretty_generate({checks:checks,evidence:evidence,temporaryDirectory:temp}))
 puts JSON.pretty_generate({checks:checks,failure:evidence['failure']&&evidence['failure'][:message],temporaryDirectory:temp})
end
