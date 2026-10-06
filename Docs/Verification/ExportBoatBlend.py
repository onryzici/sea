import bpy, os, json, math
from mathutils import Vector
root='/Users/trexoinnovation/salvage/Assets/_Game/Art/ThirdParty/UserBoat'
os.makedirs(root,exist_ok=True)
obj=next(o for o in bpy.data.objects if o.type=='MESH')
mat=obj.data.materials[0]
info=[]
for n in mat.node_tree.nodes:
    if n.type=='TEX_IMAGE':
        image=n.image
        target=os.path.join(root,image.name+'.png')
        image.filepath_raw=target;image.file_format='PNG';image.save()
        info.append({'image':image.name,'links':[(l.to_node.name,l.to_socket.name) for l in n.outputs['Color'].links]})
print('TEXTURES '+json.dumps(info))
bpy.ops.object.select_all(action='DESELECT');obj.select_set(True);bpy.context.view_layer.objects.active=obj
modifier=obj.modifiers.new('Runtime mesh reduction','DECIMATE');modifier.ratio=.18
bpy.ops.object.modifier_apply(modifier=modifier.name)
print('RUNTIME_POLYGONS '+str(len(obj.data.polygons)))
bpy.ops.export_scene.fbx(filepath=os.path.join(root,'TurquoiseHarborTug.fbx'),use_selection=True,object_types={'MESH'},axis_forward='-Z',axis_up='Y',add_leaf_bones=False,path_mode='STRIP',bake_anim=False)
# A source inspection render only; never imported as the game's model.
scene=bpy.context.scene;scene.render.engine='BLENDER_EEVEE';scene.render.resolution_x=800;scene.render.resolution_y=600;scene.render.resolution_percentage=100
scene.world.color=(.35,.35,.35)
bpy.ops.object.light_add(type='AREA',location=(1,-3,4));bpy.context.object.data.energy=500;bpy.context.object.data.shape='DISK';bpy.context.object.data.size=5
bpy.ops.object.camera_add(location=(2.5,-2.5,1.8));camera=bpy.context.object;camera.rotation_euler=(Vector((0,0,0))-camera.location).to_track_quat('-Z','Y').to_euler();camera.data.type='ORTHO';camera.data.ortho_scale=2.8;scene.camera=camera
scene.render.filepath='/Users/trexoinnovation/salvage/Docs/Screenshots/UserBoatSource.png';bpy.ops.render.render(write_still=True)
