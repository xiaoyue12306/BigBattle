"""Rig and animate the user's static FBX; preserve the original file. Blender 5.2."""
import bpy,math,json,hashlib
from pathlib import Path
from mathutils import Vector
r=Path(__file__).resolve().parent.parent;source=Path(r'C:\Users\xy\Pictures\bb\xgx.fbx');out=r/'Assets/BigBattle/Resources/Witch';out.mkdir(parents=True,exist_ok=True)
bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False);bpy.ops.import_scene.fbx(filepath=str(source))
obj=next(o for o in bpy.context.scene.objects if o.type=='MESH');bpy.context.view_layer.objects.active=obj;obj.select_set(True);bpy.ops.object.transform_apply(location=True,rotation=True,scale=True)
lo=Vector(tuple(min(v.co[i] for v in obj.data.vertices) for i in range(3)));hi=Vector(tuple(max(v.co[i] for v in obj.data.vertices) for i in range(3)));factor=2.4/(hi.z-lo.z)
for v in obj.data.vertices:v.co=(v.co-Vector(((lo.x+hi.x)/2,(lo.y+hi.y)/2,lo.z)))*factor
obj.name='Witch supplied textured body';image=bpy.data.images.get('texture_pbr_20250901.png');image.scale(2048,2048);image.filepath_raw=str(out/'WitchColor.png');image.file_format='PNG';image.save()
mat=bpy.data.materials.new('Witch original dress and face');mat.use_nodes=True;bs=mat.node_tree.nodes.get('Principled BSDF');bs.inputs['Roughness'].default_value=.8;tex=mat.node_tree.nodes.new('ShaderNodeTexImage');tex.image=image;mat.node_tree.links.new(tex.outputs['Color'],bs.inputs['Base Color']);obj.data.materials.clear();obj.data.materials.append(mat)
bpy.ops.object.armature_add();rig=bpy.context.object;rig.name='WitchRig';bpy.ops.object.mode_set(mode='EDIT');bones=rig.data.edit_bones;bones.remove(bones[0])
defs=[('Pelvis',(0,0,1.02),(0,0,1.18),None),('Spine',(0,0,1.18),(0,0,1.43),'Pelvis'),('Chest',(0,0,1.43),(0,0,1.65),'Spine'),('Neck',(0,0,1.65),(0,0,1.78),'Chest'),('Head',(0,0,1.78),(0,0,2.24),'Neck')]
for side,s in [('Left',-1),('Right',1)]:
    defs.extend([(side+'UpperArm',(s*.245,0,1.60),(s*.315,-.015,1.28),'Chest'),(side+'Forearm',(s*.315,-.015,1.28),(s*.35,-.035,1.00),side+'UpperArm'),(side+'Hand',(s*.35,-.035,1.00),(s*.35,-.035,.90),side+'Forearm'),(side+'Thigh',(s*.105,0,1.02),(s*.115,0,.57),'Pelvis'),(side+'Shin',(s*.115,0,.57),(s*.12,0,.16),side+'Thigh'),(side+'Foot',(s*.12,0,.16),(s*.12,-.17,.10),side+'Shin')])
for name,head,tail,parent in defs:
    b=bones.new(name);b.head=head;b.tail=tail
    if parent:b.parent=bones[parent]
bpy.ops.object.mode_set(mode='OBJECT');groups={name:obj.vertex_groups.new(name=name) for name,*_ in defs}
def clamp(x):return max(0,min(1,x))
for v in obj.data.vertices:
    x,y,z=v.co;side='Left' if x<0 else 'Right';ax=abs(x)
    if z<.37:
        foot=1-clamp((z-.12)/.13);weights={side+'Foot':foot,side+'Shin':1-foot}
    elif z<1.12:
        # Keep the long skirt together; only the exposed calves/feet stride independently.
        weights={'Pelvis':1}
    else:
        head=clamp((z-1.66)/.12)
        if y>.045 and z>1.52:head=max(head,clamp((z-1.52)/.12))
        chest=clamp((z-1.27)/.20);weights={'Head':head,'Chest':(1-head)*chest,'Spine':(1-head)*(1-chest)}
    arm=clamp((ax-(.285 if z<1.2 else .225))/(.05 if z<1.2 else .09))*clamp((1.69-z)/.13)*clamp((z-.86)/.12)
    if y>.045 and z>1.52:arm*=1-clamp((z-1.52)/.12)
    if arm:
        weights={k:w*(1-arm) for k,w in weights.items()};upper=clamp((z-1.20)/.19);hand=1-clamp((z-.94)/.14)
        weights[side+'UpperArm']=arm*upper;weights[side+'Forearm']=arm*(1-upper)*(1-hand);weights[side+'Hand']=arm*(1-upper)*hand
    total=sum(weights.values())
    for name,w in weights.items():
        if w>0:groups[name].add([v.index],w/total,'REPLACE')
mod=obj.modifiers.new('Witch authored skeleton','ARMATURE');mod.object=rig;obj.parent=rig
scene=bpy.context.scene;scene.render.fps=30
for b in rig.pose.bones:b.rotation_mode='XYZ'
for action_name,length in [('Idle',60),('Walk',30),('Greeting',60)]:
    action=bpy.data.actions.new(action_name);action.use_fake_user=True;rig.animation_data_create();rig.animation_data.action=action
    for frame in range(1,length+2):
        t=(frame-1)/length;phase=t*math.tau
        for b in rig.pose.bones:b.rotation_euler=(0,0,0);b.location=(0,0,0)
        def rot(name,x=0,y=0,z=0):rig.pose.bones[name].rotation_euler=tuple(math.radians(v) for v in (x,y,z))
        if action_name=='Idle':
            rot('Chest',1.5*math.sin(phase));rot('Head',0,1.8*math.sin(phase),1.5*math.sin(phase));rot('LeftUpperArm',1.2*math.sin(phase),0,-1);rot('RightUpperArm',-1.2*math.sin(phase),0,1)
        if action_name=='Walk':
            for side,sign in [('Left',1),('Right',-1)]:
                swing=math.sin(phase)*sign;rot(side+'Thigh',swing*12);rot(side+'Shin',max(0,-swing)*17);rot(side+'Foot',-swing*5);rot(side+'UpperArm',-swing*8,0,-sign*1.5);rot(side+'Forearm',-3-max(0,swing)*4)
            rot('Pelvis',0,1.5*math.sin(phase),2*math.sin(phase));rot('Chest',2,0,-1.5*math.sin(phase));rot('Head',-1,0,math.sin(phase));rig.pose.bones['Pelvis'].location.y=abs(math.sin(phase))*.018
        if action_name=='Greeting':
            lift=math.sin(math.pi*min(1,t/.9))**2;rot('RightUpperArm',-lift*18,0,-lift*55);rot('RightForearm',-lift*62,0,lift*12);rot('RightHand',lift*8,0,lift*math.sin(t*math.tau*3)*18);rot('Head',lift*3,0,-lift*4);rot('Chest',0,0,-lift*2)
        for b in rig.pose.bones:
            b.keyframe_insert('rotation_euler',frame=frame,group=b.name)
            if b.name=='Pelvis':b.keyframe_insert('location',frame=frame,group=b.name)
    print('AUTHORED_ACTION',action_name,len(action.layers))
rig.animation_data.action=None
for b in rig.pose.bones:b.rotation_euler=(0,0,0);b.location=(0,0,0)
bpy.ops.object.select_all(action='DESELECT');obj.select_set(True);rig.select_set(True);bpy.context.view_layer.objects.active=rig
bpy.ops.export_scene.fbx(filepath=str(out/'Witch.fbx'),use_selection=True,add_leaf_bones=False,bake_anim=True,bake_anim_use_all_actions=True,bake_anim_use_nla_strips=False,bake_anim_simplify_factor=0,axis_forward='-Z',axis_up='Y',path_mode='STRIP',use_mesh_modifiers=True)
bpy.ops.wm.save_as_mainfile(filepath=str(r/'Artifacts/witch-rigged.blend'))
report=dict(source=str(source),sourceBytes=source.stat().st_size,sourceSHA256=hashlib.sha256(source.read_bytes()).hexdigest(),sourceBones=0,sourceActions=0,vertices=len(obj.data.vertices),bones=len(defs),height=2.4,authoredActions=['Idle','Walk','Greeting'],fbxBytes=(out/'Witch.fbx').stat().st_size,fbxSHA256=hashlib.sha256((out/'Witch.fbx').read_bytes()).hexdigest(),textureSHA256=hashlib.sha256((out/'WitchColor.png').read_bytes()).hexdigest())
(r/'Docs/Validation/witch-assets-v0110.json').write_text(json.dumps(report,indent=2)+'\n',encoding='utf-8');print('WITCH_PREPARE_OK',report['fbxBytes'])
