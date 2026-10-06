require 'net/http'
require 'json'
require 'fileutils'
require 'digest'
repo='KayKit-Game-Assets/KayKit-Medieval-Hexagon-Pack-1.0'
prefix='addons/kaykit_medieval_hexagon_pack/Assets/fbx(unity)/'
paths=['LICENSE.txt']+%w[buildings/red/building_home_A_red.fbx buildings/red/building_home_B_red.fbx buildings/red/building_tavern_red.fbx buildings/red/building_windmill_red.fbx buildings/neutral/building_bridge_A.fbx decoration/nature/mountain_A_grass.fbx decoration/nature/mountain_B_grass.fbx decoration/nature/rock_single_A.fbx decoration/nature/rock_single_C.fbx decoration/nature/tree_single_A.fbx decoration/nature/tree_single_B.fbx decoration/nature/hill_single_A.fbx buildings/red/hexagons_medieval.png].map{|p|prefix+p}
target='Assets/_Game/Art/ThirdParty/KayKit'
FileUtils.mkdir_p(target)
records=paths.map do |p|
  url="https://raw.githubusercontent.com/#{repo}/main/#{URI::DEFAULT_PARSER.escape(p)}"
  response=Net::HTTP.get_response(URI(url));raise "#{response.code}: #{p}" unless response.is_a?(Net::HTTPSuccess)
  raise "Unexpected LFS pointer: #{p}" if response.body.start_with?('version https://git-lfs')
  file=File.join(target,File.basename(p));File.binwrite(file,response.body)
  {source:url,file:file,bytes:response.body.bytesize,sha256:Digest::SHA256.hexdigest(response.body)}
end
File.write('Docs/Verification/StylizedAssetDownloads.json',JSON.pretty_generate(records))
puts JSON.generate({downloaded:records.length,bytes:records.sum{|r|r[:bytes]}})
