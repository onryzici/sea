"""Run Blender with --disable-autoexec on the retained BoardingGate source.
Edits the user's mesh, preserving hull/deck, textures and the original source.
Boolean tools are removed before saving/export; none are gameplay decoration.
"""
import bpy, math, json
from mathutils import Vector
root='/Users/trexoinnovation/salvage'
obj=next(o for o in bpy.data.objects if o.type=='MESH')
bpy.context.view_layer.objects.active=obj
def source(x,y,z):return Vector((-(z+.00872)/5.263639,(x+.00225)/5.124989,(y-1.97082)/5.279435))
def game(p):return Vector((p.y*5.124989-.00225,p.z*5.279435+1.97082,-p.x*5.263639-.00872))

# Raise the existing cabin roof/details, stretching its walls smoothly, not the hull.
for vertex in obj.data.vertices:
 p=game(vertex.co)
 if p.z>1.02 and abs(p.x)<1.45 and p.y>1.62:
  p.y+=.58*min(1,max(0,(p.y-1.62)/1.35))
  vertex.co=source(*p)
obj.data.update()

wall=bpy.data.materials.new('CabinInteriorIvory');wall.diffuse_color=(.75,.70,.58,1)
obj.data.materials.append(wall);wall_index=len(obj.data.materials)-1
timber=bpy.data.materials.new('CabinInteriorTimber');timber.diffuse_color=(.42,.25,.12,1)
obj.data.materials.append(timber);timber_index=len(obj.data.materials)-1
def cut(name,minimum,maximum):
 a=source(*minimum);b=source(*maximum);center=(a+b)*.5
 bpy.ops.mesh.primitive_cube_add(size=1,location=center);tool=bpy.context.object;tool.name='TEMP_'+name
 tool.dimensions=Vector((abs(b.x-a.x),abs(b.y-a.y),abs(b.z-a.z)))
 bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
 tool.data.materials.clear()
 for m in obj.data.materials:tool.data.materials.append(m)
 for face in tool.data.polygons:face.material_index=wall_index
 bpy.context.view_layer.objects.active=obj
 modifier=obj.modifiers.new(name,'BOOLEAN');modifier.operation='DIFFERENCE';modifier.solver='EXACT';modifier.object=tool
 bpy.ops.object.modifier_apply(modifier=modifier.name);bpy.data.objects.remove(tool,do_unlink=True)

# At least 2 m headroom, 1 m door width and a continuous floor at deck level.
cut('Walkable wheelhouse cavity',(-.72,1.505,1.43),(.68,3.53,3.16))
cut('Open aft entrance',(-.53,1.48,1.03),(.53,3.48,1.62))
cut('Forward observation window',(-.64,2.38,2.98),(.62,3.34,4.15))
cut('Port observation window',(-1.65,2.40,1.78),(-.62,3.30,2.91))
cut('Starboard observation window',(.59,2.40,1.78),(1.65,3.30,2.91))

# Interior cut faces receive coherent UVs/material slots instead of stretched hull atlas UVs.
obj.data.update()
uv=obj.data.uv_layers.active
for face in obj.data.polygons:
 if face.material_index!=wall_index:continue
 center=game(face.center)
 if center.y<1.57:face.material_index=timber_index
 for loop in face.loop_indices:
  p=game(obj.data.vertices[obj.data.loops[loop].vertex_index].co)
  uv.data[loop].uv=(p.x*.35,p.z*.35) if face.material_index==timber_index else (p.x*.4,p.y*.4)

# The supplied generated mesh has uneven/non-manifold wall thickness. Fit an interior
# lining to the edited cabin so Boolean fragments cannot expose the outside through cracks.
# These are mesh edits to the supplied cabin, not blockout GameObjects in Unity.
lining=[]
def panel(name,vertices,normal,material):
 mesh=bpy.data.meshes.new(name);points=[source(*p) for p in vertices]
 wanted=Vector((-normal[2]/5.263639,normal[0]/5.124989,normal[1]/5.279435)).normalized()
 order=list(range(4))
 if (points[1]-points[0]).cross(points[2]-points[0]).dot(wanted)<0:order.reverse()
 mesh.from_pydata(points,[],[order]);mesh.materials.append(material);mesh.update()
 layer=mesh.uv_layers.new(name='UVMap')
 for loop in mesh.polygons[0].loop_indices:
  p=Vector(vertices[mesh.loops[loop].vertex_index]);layer.data[loop].uv=(p.x*.35,p.z*.35) if abs(normal[1])>.5 else (p.z*.35,p.y*.35)
 part=bpy.data.objects.new(name,mesh);bpy.context.collection.objects.link(part);bpy.context.view_layer.objects.active=part
 shell=part.modifiers.new('Lining thickness','SOLIDIFY');shell.thickness=.004;shell.offset=-1
 bpy.ops.object.modifier_apply(modifier=shell.name)
 edge=part.modifiers.new('Soft panel edges','BEVEL');edge.width=.0015;edge.segments=2
 bpy.ops.object.modifier_apply(modifier=edge.name);lining.append(part)
def side(name,x,y0,y1,z0,z1,n):panel(name,[(x,y0,z0),(x,y1,z0),(x,y1,z1),(x,y0,z1)],(n,0,0),wall)
def end(name,z,x0,x1,y0,y1,n):panel(name,[(x0,y0,z),(x1,y0,z),(x1,y1,z),(x0,y1,z)],(0,0,n),wall)
panel('Cabin timber sole',[(-.72,1.525,1.40),(.68,1.525,1.40),(.68,1.525,3.16),(-.72,1.525,3.16)],(0,1,0),timber)
panel('Cabin overhead',[(-.75,3.53,1.38),(.71,3.53,1.38),(.71,3.53,3.19),(-.75,3.53,3.19)],(0,-1,0),wall)
for x,n in [(-.72,1),(.68,-1)]:
 side('Window sill lining',x,1.50,2.45,1.38,3.19,n)
 side('Window header lining',x,3.22,3.56,1.38,3.19,n)
 side('Aft jamb lining',x,2.44,3.23,1.38,1.85,n)
 side('Fore jamb lining',x,2.44,3.23,2.82,3.19,n)
end('Front sill',3.16,-.75,.71,1.50,2.42,-1)
end('Front header',3.16,-.75,.71,3.30,3.56,-1)
end('Front port jamb',3.16,-.75,-.57,2.41,3.31,-1)
end('Front starboard jamb',3.16,.55,.71,2.41,3.31,-1)
end('Door port jamb',1.40,-.72,-.53,1.525,3.53,1)
end('Door starboard jamb',1.40,.53,.68,1.525,3.53,1)
end('Door header',1.40,-.53,.53,3.48,3.53,1)
bpy.ops.object.select_all(action='DESELECT');obj.select_set(True)
for part in lining:part.select_set(True)
bpy.context.view_layer.objects.active=obj;bpy.ops.object.join()

bpy.ops.object.select_all(action='DESELECT');obj.select_set(True);bpy.context.view_layer.objects.active=obj
bpy.ops.wm.save_as_mainfile(filepath=root+'/Source/Art/TurquoiseHarborTug_Interior.blend')
bpy.ops.export_scene.fbx(filepath=root+'/Assets/_Game/Art/ThirdParty/UserBoat/TurquoiseHarborTug.fbx',use_selection=True,object_types={'MESH'},axis_forward='-Z',axis_up='Y',add_leaf_bones=False,path_mode='STRIP',bake_anim=False)
print('INTERIOR_MESH',json.dumps({'faces':len(obj.data.polygons),'gameBounds':[list(game(Vector(v))) for v in obj.bound_box],'materials':[m.name if m else None for m in obj.data.materials]}))

scene=bpy.context.scene;scene.render.engine='BLENDER_EEVEE';scene.render.resolution_x=1400;scene.render.resolution_y=1000;scene.render.resolution_percentage=100
scene.world.color=(.4,.4,.4)
bpy.ops.object.light_add(type='AREA',location=(1,-3,4));bpy.context.object.data.energy=500;bpy.context.object.data.size=5
bpy.ops.object.camera_add(location=source(0,2.65,-.3));camera=bpy.context.object
camera.rotation_euler=(source(0,2.6,2.6)-camera.location).to_track_quat('-Z','Y').to_euler();camera.data.lens=28;scene.camera=camera
scene.render.filepath=root+'/Docs/Screenshots/BoatInteriorBlender.png';bpy.ops.render.render(write_still=True)
