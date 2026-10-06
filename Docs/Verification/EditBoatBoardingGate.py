import bpy, os
from mathutils import Vector

# Edit only the supplied model; the original .blend is never overwritten.
obj=next(o for o in bpy.data.objects if o.type=='MESH')
bpy.ops.object.select_all(action='DESELECT');obj.select_set(True);bpy.context.view_layer.objects.active=obj
reduce=obj.modifiers.new('Runtime mesh reduction','DECIMATE');reduce.ratio=.18
bpy.ops.object.modifier_apply(modifier=reduce.name)
# Temporary Boolean tool, not a visible primitive in the exported scene.
# Coordinates derived from the imported model's actual Unity transform at the existing boarding gate.
bpy.ops.mesh.primitive_cube_add(size=1,location=(.475,-.415,.16))
cutter=bpy.context.object;cutter.name='Temporary boarding opening cutter';cutter.dimensions=(.38,.34,.475)
bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
bpy.context.view_layer.objects.active=obj
cut=obj.modifiers.new('Walk-through port boarding gate','BOOLEAN');cut.operation='DIFFERENCE';cut.solver='EXACT';cut.object=cutter
bpy.ops.object.modifier_apply(modifier=cut.name)
bpy.data.objects.remove(cutter,do_unlink=True)
bpy.ops.object.select_all(action='DESELECT');obj.select_set(True);bpy.context.view_layer.objects.active=obj
root='/Users/trexoinnovation/salvage'
bpy.ops.wm.save_as_mainfile(filepath=root+'/Source/Art/TurquoiseHarborTug_BoardingGate.blend')
bpy.ops.export_scene.fbx(filepath=root+'/Assets/_Game/Art/ThirdParty/UserBoat/TurquoiseHarborTug.fbx',use_selection=True,object_types={'MESH'},axis_forward='-Z',axis_up='Y',add_leaf_bones=False,path_mode='STRIP',bake_anim=False)
scene=bpy.context.scene;scene.render.engine='BLENDER_EEVEE';scene.render.resolution_x=1000;scene.render.resolution_y=700;scene.render.resolution_percentage=100
scene.world.color=(.35,.35,.35)
bpy.ops.object.light_add(type='AREA',location=(1,-3,4));bpy.context.object.data.energy=500;bpy.context.object.data.size=5
bpy.ops.object.camera_add(location=(2.5,-2.5,1.8));camera=bpy.context.object;camera.rotation_euler=(Vector((0,0,0))-camera.location).to_track_quat('-Z','Y').to_euler();camera.data.type='ORTHO';camera.data.ortho_scale=2.8;scene.camera=camera
scene.render.filepath=root+'/Docs/Screenshots/BoatBoardingGateSource.png';bpy.ops.render.render(write_still=True)
print('EDITED_POLYGONS',len(obj.data.polygons))
