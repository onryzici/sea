import bpy, json
from mathutils import Vector
objects=[]
for o in bpy.data.objects:
    if o.type=='MESH':
        corners=[o.matrix_world @ Vector(c) for c in o.bound_box]
        objects.append({'name':o.name,'vertices':len(o.data.vertices),'polygons':len(o.data.polygons),'min':[min(c[i] for c in corners) for i in range(3)],'max':[max(c[i] for c in corners) for i in range(3)],'materials':[m.name for m in o.data.materials if m]})
print('BOAT_INSPECT '+json.dumps({'objects':objects,'images':[{'name':i.name,'size':list(i.size),'packed':bool(i.packed_file),'path':i.filepath} for i in bpy.data.images],'materials':[{'name':m.name,'nodes':[(n.name,n.type) for n in m.node_tree.nodes] if m.use_nodes else []} for m in bpy.data.materials]}))
