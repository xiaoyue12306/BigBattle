"""Refine the user's existing 13-bone skin to match the raised left thumb arm.

Run after PrepareYueyue.py with Blender; original FBX/texture remain untouched.
"""
import bpy,math,json,hashlib
from pathlib import Path
from mathutils import Vector
root=Path(__file__).resolve().parent.parent
bpy.ops.wm.open_mainfile(filepath=str(root/'Artifacts/yueyue-rigged.blend'))
obj=next(o for o in bpy.context.scene.objects if o.type=='MESH');rig=obj.parent
bpy.context.view_layer.objects.active=rig;rig.select_set(True)
bpy.ops.object.mode_set(mode='EDIT')
positions={
 'LeftUpperArm':((-.285,-.01,1.43),(-.34,-.09,1.15)),
 'LeftForearm':((-.34,-.09,1.15),(-.11,-.29,1.20)),
 'LeftHand':((-.11,-.29,1.20),(-.025,-.33,1.27)),
 'RightUpperArm':((.285,0,1.43),(.32,-.015,1.13)),
 'RightForearm':((.32,-.015,1.13),(.30,-.07,.91)),
 'RightHand':((.30,-.07,.91),(.29,-.08,.80))}
for name,(a,b) in positions.items():
 bone=rig.data.edit_bones[name];bone.head=a;bone.tail=b
bpy.ops.object.mode_set(mode='OBJECT')
def smooth(a,b,x):
 t=max(0,min(1,(x-a)/(b-a)));return t*t*(3-2*t)
def distance(p,a,b):
 a,b=Vector(a),Vector(b);delta=b-a;t=max(0,min(1,(p-a).dot(delta)/delta.length_squared));return (p-(a+delta*t)).length
changed=0
for v in obj.data.vertices:
 x,y,z=v.co
 if not(.75<z<1.57):continue
 side='Left' if x<0 else 'Right'
 arm=smooth(.22,.35,abs(x))*(1-smooth(1.42,1.57,z))*smooth(.77,.92,z)*(1-smooth(.06,.18,y))
 front=smooth(.16,.27,-y)*smooth(1.04,1.12,z)*(1-smooth(1.36,1.44,z))*(1-smooth(.065,.14,x))
 if front>arm:side='Left';arm=front
 if arm<=.001:continue
 weights={obj.vertex_groups[g.group].name:g.weight for g in v.groups}
 # Start from the old non-arm allocation so cloth/head parts retain their body.
 body={n:w for n,w in weights.items() if 'Arm' not in n and 'Forearm' not in n and 'Hand' not in n}
 total=sum(body.values());body={n:w/max(total,.0001)*(1-arm) for n,w in body.items()} if total else {'Spine':1-arm}
 names=[side+'UpperArm',side+'Forearm',side+'Hand']
 scores=[math.exp(-(distance(v.co,*positions[n])/.105)**2) for n in names]
 total=sum(scores)
 if total<1e-10:continue
 for n,w in zip(names,scores):body[n]=arm*w/total
 for group in obj.vertex_groups:
  try:group.remove([v.index])
  except RuntimeError:pass
 for n,w in body.items():
  if w>.00001:obj.vertex_groups[n].add([v.index],w,'REPLACE')
 changed+=1
bpy.ops.object.select_all(action='DESELECT');obj.select_set(True);rig.select_set(True);bpy.context.view_layer.objects.active=rig
out=root/'Assets/BigBattle/Resources/Yueyue/Yueyue.fbx'
bpy.ops.export_scene.fbx(filepath=str(out),use_selection=True,add_leaf_bones=False,bake_anim=False,axis_forward='-Z',axis_up='Y',path_mode='STRIP',use_mesh_modifiers=True)
bpy.ops.wm.save_as_mainfile(filepath=str(root/'Artifacts/yueyue-rigged-v080.blend'))
report={'version':'0.8.0','vertices':len(obj.data.vertices),'bones':len(rig.data.bones),'reweightedVertices':changed,'fbxBytes':out.stat().st_size,'fbxSHA256':hashlib.sha256(out.read_bytes()).hexdigest(),'refinedArmRestPositions':positions,'source':'same user model; matching raised left forearm and hand; regional weights remain a prototype'}
(root/'Docs/Validation/yueyue-rig-v080.json').write_text(json.dumps(report,indent=2)+'\n',encoding='utf8')
print('YUE_RIG_REFINED',changed,'vertices')
