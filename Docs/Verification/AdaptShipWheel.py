"""Convert the downloaded CC0 wheel, repairing its rim/axle orientations. No generated geometry."""
import bpy
import math
from mathutils import Vector
from pathlib import Path

root = Path('/Users/trexoinnovation/salvage')
bpy.ops.wm.read_factory_settings(use_empty=True)
bpy.ops.import_scene.gltf(filepath=str(root/'Source/ThirdParty/ShipWheel/ShipsWheel.glb'))
for obj in list(bpy.data.objects):
    obj.animation_data_clear()
wheel = bpy.data.objects['wheel']
wheel.rotation_mode='XYZ'
wheel.rotation_euler=(0,0,0)
for name in ['ships-wheel_1','ships-wheel_2','ships-wheel_3']:
    bpy.data.objects[name].rotation_mode='XYZ'
    bpy.data.objects[name].rotation_euler.z += math.pi/2
mesh = bpy.data.objects['wheel/deck']
bpy.ops.object.select_all(action='DESELECT')
mesh.select_set(True)
bpy.context.view_layer.objects.active = mesh
bpy.ops.object.mode_set(mode='EDIT')
bpy.ops.mesh.select_all(action='SELECT')
bpy.ops.mesh.remove_doubles(threshold=.0003)
bpy.ops.mesh.separate(type='LOOSE')
bpy.ops.object.mode_set(mode='OBJECT')
for obj in list(bpy.context.selected_objects):
    coords = [v.co for v in obj.data.vertices]
    lo = Vector(tuple(min(v[i] for v in coords) for i in range(3)))
    hi = Vector(tuple(max(v[i] for v in coords) for i in range(3)))
    size = hi-lo
    print('PART', obj.name, tuple(size))
    # The source's two wooden rims lie flat while its spokes are upright.
    if size.x > 1.2 and size.y > 1.0 and size.z < .3:
        center = (hi+lo)/2
        from mathutils import Matrix
        rot = Matrix.Rotation(math.pi/2, 3, 'X')
        for v in obj.data.vertices:
            v.co = rot @ (v.co-center) + Vector((0, center.z, 0))

# Rejoin the repaired wooden pieces into one draw mesh while keeping its wheel pivot.
parts=list(bpy.context.selected_objects)
bpy.context.view_layer.objects.active=parts[0]
bpy.ops.object.join()

# Source wheel brass boss is also laid flat; orient it with the wheel plane.
boss=bpy.data.objects['wheel/brass']
boss.rotation_mode='XYZ'
boss.rotation_euler.x += math.pi/2
# Export no baked automatic spin: Unity animates only the wheel from real steering.
for action in list(bpy.data.actions): bpy.data.actions.remove(action)
out=root/'Assets/_Game/Art/ThirdParty/ShipWheel'
for img in bpy.data.images:
    if img.size[0] and img.name != 'Render Result':
        img.filepath_raw=str(out/'WheelWood.png');img.file_format='PNG';img.save()
bpy.ops.wm.save_as_mainfile(filepath=str(root/'Source/ThirdParty/ShipWheel/ShipsWheelAdapted.blend'))
bpy.ops.export_scene.fbx(filepath=str(out/'ShipsWheel.fbx'), use_selection=False,
    add_leaf_bones=False, bake_anim=False, axis_forward='-Z',axis_up='Y',path_mode='STRIP')
