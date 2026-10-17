"""Prepare the user's static FBX: normalized scale, compact texture and simple authored rig."""
import bpy, math, json
from pathlib import Path
from mathutils import Vector
root=Path(__file__).resolve().parent.parent
out=root/'Assets/BigBattle/Resources/Yueyue'; out.mkdir(parents=True,exist_ok=True)
bpy.ops.wm.open_mainfile(filepath=str(root/'Artifacts/yueyue-source.blend'))
obj=next(o for o in bpy.context.scene.objects if o.type=='MESH')
bpy.context.view_layer.objects.active=obj;obj.select_set(True)
bpy.ops.object.transform_apply(location=True,rotation=True,scale=True)
lo=Vector((min(v.co.x for v in obj.data.vertices),min(v.co.y for v in obj.data.vertices),min(v.co.z for v in obj.data.vertices)))
hi=Vector((max(v.co.x for v in obj.data.vertices),max(v.co.y for v in obj.data.vertices),max(v.co.z for v in obj.data.vertices)))
factor=2.25/(hi.z-lo.z)
for v in obj.data.vertices:v.co=(v.co-Vector(((lo.x+hi.x)/2,(lo.y+hi.y)/2,lo.z)))*factor
obj.name='Yueyue textured body'
image=bpy.data.images.get('texture_pbr_20250901.png');image.scale(2048,2048);image.filepath_raw=str(out/'YueyueColor.png');image.file_format='PNG';image.save()
mat=bpy.data.materials.new('Yueyue cloth and face');mat.diffuse_color=(1,1,1,1);mat.use_nodes=True
bs=mat.node_tree.nodes.get('Principled BSDF');tex=mat.node_tree.nodes.new('ShaderNodeTexImage');tex.image=image
mat.node_tree.links.new(tex.outputs['Color'],bs.inputs['Base Color']);bs.inputs['Roughness'].default_value=.82
obj.data.materials.clear();obj.data.materials.append(mat)
bpy.ops.object.armature_add();rig=bpy.context.object;rig.name='YueyueRig'
bpy.ops.object.mode_set(mode='EDIT');eb=rig.data.edit_bones;eb.remove(eb[0])
defs=[('Pelvis',(0,0,.83),(0,0,1.10),None),('Spine',(0,0,1.10),(0,0,1.54),'Pelvis'),('Head',(0,0,1.54),(0,0,2.10),'Spine')]
for side,x in [('Left',-.29),('Right',.29)]:
    defs.extend([(side+'UpperArm',(x,0,1.43),(x*1.36,-.01,1.14),'Spine'),(side+'Forearm',(x*1.36,-.01,1.14),(x*1.32,-.05,.88),side+'UpperArm'),(side+'Hand',(x*1.32,-.05,.88),(x*1.30,-.05,.78),side+'Forearm'),(side+'Thigh',(x*.50,0,.87),(x*.56,0,.48),'Pelvis'),(side+'Shin',(x*.56,0,.48),(x*.62,-.04,.12),side+'Thigh')])
for name,head,tail,parent in defs:
    b=eb.new(name);b.head=head;b.tail=tail
    if parent:b.parent=eb[parent]
bpy.ops.object.mode_set(mode='OBJECT')
groups={name:obj.vertex_groups.new(name=name) for name,*_ in defs}
for v in obj.data.vertices:
    x,y,z=v.co;ax=abs(x);side='Left' if x<0 else 'Right'
    # Soft region weights preserve the face and scarf; a compact game rig,
    # deliberately independent of the banana's proportion-specific Mixamo data.
    weights={}
    arm=max(0,min(1,(ax-.245)/.10))*max(0,min(1,(1.56-z)/.18))*max(0,min(1,(z-.74)/.10))
    if z<.87:
        thigh=max(0,min(1,(z-.36)/.28));leg=max(0,min(1,(.94-z)/.20))
        weights={side+'Thigh':leg*thigh,side+'Shin':leg*(1-thigh),'Pelvis':1-leg}
    else:
        head=max(0,min(1,(z-1.46)/.16));spine=max(0,min(1,(z-.90)/.25))
        weights={'Head':head,'Spine':(1-head)*spine,'Pelvis':(1-head)*(1-spine)}
    if arm:
        weights={k:w*(1-arm) for k,w in weights.items()}
        upper=max(0,min(1,(z-1.03)/.24));hand=max(0,min(1,(.99-z)/.15))
        weights[side+'UpperArm']=arm*upper;weights[side+'Forearm']=arm*(1-upper)*(1-hand);weights[side+'Hand']=arm*(1-upper)*hand
    total=sum(weights.values())
    for name,w in weights.items():
        if w>0:groups[name].add([v.index],w/total,'REPLACE')
mod=obj.modifiers.new('Authored game skeleton','ARMATURE');mod.object=rig;obj.parent=rig
bpy.ops.object.select_all(action='DESELECT');obj.select_set(True);rig.select_set(True);bpy.context.view_layer.objects.active=rig
bpy.ops.export_scene.fbx(filepath=str(out/'Yueyue.fbx'),use_selection=True,add_leaf_bones=False,bake_anim=False,axis_forward='-Z',axis_up='Y',path_mode='STRIP',use_mesh_modifiers=True)
bpy.ops.wm.save_as_mainfile(filepath=str(root/'Artifacts/yueyue-rigged.blend'))
# Render the actual textured model from its forward (-Y) direction for inspection.
scene=bpy.context.scene;scene.render.engine='CYCLES';scene.cycles.samples=16
scene.render.resolution_x=700;scene.render.resolution_y=850;scene.render.resolution_percentage=100
scene.world.color=(.65,.65,.65)
bpy.ops.object.camera_add(location=(3,-7,3));cam=bpy.context.object;cam.rotation_euler=(Vector((0,0,1.2))-cam.location).to_track_quat('-Z','Y').to_euler();cam.data.type='ORTHO';cam.data.ortho_scale=3.0;scene.camera=cam
bpy.ops.object.light_add(type='AREA',location=(1,-4,5));bpy.context.object.data.energy=450;bpy.context.object.data.shape='DISK';bpy.context.object.data.size=5
scene.render.filepath=str(root/'Artifacts/yueyue-model.png');bpy.ops.render.render(write_still=True)
print('YUE_PREPARE_OK vertices',len(obj.data.vertices),'bones',len(defs))
