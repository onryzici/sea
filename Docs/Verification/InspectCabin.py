import bpy, json
from mathutils import Vector
from mathutils.bvhtree import BVHTree
obj=next(o for o in bpy.data.objects if o.type=='MESH')
tree=BVHTree.FromObject(obj,bpy.context.evaluated_depsgraph_get())
def source(x,y,z): return Vector((-(z+.00872)/5.263639,(x+.00225)/5.124989,(y-1.97082)/5.279435))
def game(p): return (p.y*5.124989-.00225,p.z*5.279435+1.97082,-p.x*5.263639-.00872)
samples=[]
for x in [-.85,0,.85]:
 for y in [1.55,2,2.5,3,3.4,3.6,3.8]:
  hit=tree.ray_cast(source(x,y,0),Vector((-1,0,0)),1)
  samples.append({'x':x,'y':y,'hit':game(hit[0]) if hit[0] else None})
print('CABIN_SECTION',json.dumps(samples))
print('MATERIALS',[m.name if m else None for m in obj.data.materials])
