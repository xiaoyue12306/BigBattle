"""Blender 5.2: game derivative of the supplied static Ali figurine, original untouched."""
import bpy,bmesh,math,json,hashlib
from pathlib import Path
from mathutils import Vector
r=Path(__file__).resolve().parent.parent;source=Path(r'C:\Users\xy\Pictures\bb\ali.fbx');out=r/'Assets/BigBattle/Resources/Ali';out.mkdir(parents=True,exist_ok=True)
bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False);bpy.ops.import_scene.fbx(filepath=str(source))
obj=next(o for o in bpy.context.scene.objects if o.type=='MESH');bpy.context.view_layer.objects.active=obj;bpy.ops.object.transform_apply(location=True,rotation=True,scale=True)
lo=Vector(tuple(min(v.co[i] for v in obj.data.vertices) for i in range(3)));hi=Vector(tuple(max(v.co[i] for v in obj.data.vertices) for i in range(3)))
for v in obj.data.vertices:v.co=(v.co-Vector(((lo.x+hi.x)/2,(lo.y+hi.y)/2,lo.z)))*(2.3/(hi.z-lo.z))
# Remove the thin figurine display plinth rather than sliding it with the feet.
bm=bmesh.new();bm.from_mesh(obj.data);remove=[v for v in bm.verts if v.co.z<.105];removed=len(remove);bmesh.ops.delete(bm,geom=remove,context='VERTS')
sole=[e for e in bm.edges if e.is_boundary and all(v.co.z<.24 for v in e.verts)]
for e in sole:
    for v in e.verts:v.co.z=.105
bmesh.ops.holes_fill(bm,edges=sole,sides=0);bmesh.ops.recalc_face_normals(bm,faces=list(bm.faces));bm.to_mesh(obj.data);bm.free()
ground=min(v.co.z for v in obj.data.vertices)
for v in obj.data.vertices:v.co.z-=ground
obj.name='Ali supplied red fox body';im=bpy.data.images['texture_pbr_20250901.png'];im.scale(2048,2048);im.filepath_raw=str(out/'AliColor.png');im.file_format='PNG';im.save()
mat=bpy.data.materials.new('Ali original color');mat.use_nodes=True;tex=mat.node_tree.nodes.new('ShaderNodeTexImage');tex.image=im;mat.node_tree.links.new(tex.outputs['Color'],mat.node_tree.nodes['Principled BSDF'].inputs['Base Color']);obj.data.materials.clear();obj.data.materials.append(mat)
bpy.ops.object.armature_add();rig=bpy.context.object;rig.name='AliRig';bpy.ops.object.mode_set(mode='EDIT');bones=rig.data.edit_bones;bones.remove(bones[0])
defs=[('Pelvis',(0,0,.48),(0,0,.7),None),('Spine',(0,0,.7),(0,0,.9),'Pelvis'),('Chest',(0,0,.9),(0,0,1.12),'Spine'),('Neck',(0,0,1.12),(0,0,1.24),'Chest'),('Head',(0,0,1.24),(0,0,1.95),'Neck')]
for side,s in [('Left',-1),('Right',1)]:
    defs.extend([(side+'UpperArm',(s*.24,0,1.04),(s*.32,-.03,.86),'Chest'),(side+'Forearm',(s*.32,-.03,.86),(s*.37,-.06,.72),side+'UpperArm'),(side+'Hand',(s*.37,-.06,.72),(s*.37,-.06,.65),side+'Forearm'),(side+'Thigh',(s*.12,0,.49),(s*.12,0,.28),'Pelvis'),(side+'Shin',(s*.12,0,.28),(s*.12,0,.07),side+'Thigh'),(side+'Foot',(s*.12,0,.07),(s*.12,-.12,.035),side+'Shin'),(side+'Tail',(s*.18,.12,.65),(s*.38,.37,1.1),'Pelvis')])
for name,head,tail,parent in defs:
    b=bones.new(name);b.head=head;b.tail=tail
    if parent:b.parent=bones[parent]
bpy.ops.object.mode_set(mode='OBJECT');groups={n:obj.vertex_groups.new(name=n) for n,*_ in defs}
def clamp(x):return max(0,min(1,x))
for v in obj.data.vertices:
    x,y,z=v.co;side='Left' if x<0 else 'Right';ax=abs(x)
    if z<.36:
        foot=1-clamp((z-.09)/.13);weights={side+'Foot':foot,side+'Shin':1-foot}
    elif z<.66:weights={'Pelvis':1}
    else:
        head=clamp((z-1.07)/.13);chest=clamp((z-.72)/.20);weights={'Head':head,'Chest':(1-head)*chest,'Spine':(1-head)*(1-chest)}
    arm=clamp((ax-.26)/.09)*clamp((1.1-z)/.12)*clamp((z-.59)/.14)*(1-clamp((y-.07)/.08))
    tail=clamp((y-.13)/.10)*clamp((1.25-z)/.16)*clamp((z-.45)/.14)
    if arm:
        weights={k:w*(1-arm) for k,w in weights.items()};upper=clamp((z-.79)/.15);hand=1-clamp((z-.66)/.10);weights.update({side+'UpperArm':arm*upper,side+'Forearm':arm*(1-upper)*(1-hand),side+'Hand':arm*(1-upper)*hand})
    if tail:weights={**{k:w*(1-tail) for k,w in weights.items()},side+'Tail':tail}
    total=sum(weights.values())
    for name,w in weights.items():
        if w>0:groups[name].add([v.index],w/total,'REPLACE')
mod=obj.modifiers.new('Ali authored skin','ARMATURE');mod.object=rig;obj.parent=rig;scene=bpy.context.scene;scene.render.fps=30
for b in rig.pose.bones:b.rotation_mode='XYZ'
for name,length in [('Idle',60),('Walk',30),('Place',15),('Defeat',30)]:
    action=bpy.data.actions.new(name);action.use_fake_user=True;rig.animation_data_create();rig.animation_data.action=action
    for f in range(1,length+2):
        t=(f-1)/length;p=t*math.tau
        for b in rig.pose.bones:b.rotation_euler=(0,0,0);b.location=(0,0,0)
        def rot(n,x=0,y=0,z=0):rig.pose.bones[n].rotation_euler=tuple(math.radians(a) for a in (x,y,z))
        rot('Chest',math.sin(p)*1.4);rot('Head',0,0,math.sin(p)*1.2)
        for side,s in [('Left',1),('Right',-1)]:
            rot(side+'Tail',math.sin(p)*3,math.sin(p)*s*3,0)
            if name=='Walk':
                a=math.sin(p)*s;rot(side+'Thigh',a*15);rot(side+'Shin',max(0,-a)*20);rot(side+'Foot',-a*6);rot(side+'UpperArm',-a*9);rot(side+'Forearm',-3)
            if name=='Place':
                a=math.sin(math.pi*t);rot(side+'UpperArm',-a*25,0,s*a*6);rot(side+'Forearm',-a*18);rot('Chest',a*8);rig.pose.bones['Pelvis'].location.y=-a*.05
        if name=='Defeat':rot('Pelvis',min(1,t*2)*65);rig.pose.bones['Pelvis'].location.y=-min(1,t*2)*.22
        for b in rig.pose.bones:
            b.keyframe_insert('rotation_euler',frame=f,group=b.name)
            if b.name=='Pelvis':b.keyframe_insert('location',frame=f,group=b.name)
rig.animation_data.action=None
for b in rig.pose.bones:b.rotation_euler=(0,0,0);b.location=(0,0,0)
bpy.ops.object.select_all(action='DESELECT');obj.select_set(True);rig.select_set(True);bpy.context.view_layer.objects.active=rig
bpy.ops.export_scene.fbx(filepath=str(out/'Ali.fbx'),use_selection=True,add_leaf_bones=False,bake_anim=True,bake_anim_use_all_actions=True,bake_anim_use_nla_strips=False,bake_anim_simplify_factor=0,axis_forward='-Z',axis_up='Y',path_mode='STRIP')
bpy.ops.wm.save_as_mainfile(filepath=str(r/'Artifacts/ali-rigged.blend'))
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
report=dict(source=str(source),sourceBytes=source.stat().st_size,sourceSHA256=sha(source),sourceVertices=24987,sourceBones=0,sourceActions=0,removedPlinthVertices=removed,vertices=len(obj.data.vertices),bones=len(defs),height=max(v.co.z for v in obj.data.vertices),authoredActions=['Idle','Walk','Place','Defeat'],fbxBytes=(out/'Ali.fbx').stat().st_size,fbxSHA256=sha(out/'Ali.fbx'),textureSHA256=sha(out/'AliColor.png'))
(r/'Docs/Validation/ali-assets-v0120.json').write_text(json.dumps(report,indent=2)+'\n');print('ALI_PREPARE_OK',report)
