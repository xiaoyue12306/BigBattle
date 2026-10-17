import bpy, json
from pathlib import Path
bpy.ops.object.select_all(action='SELECT'); bpy.ops.object.delete(use_global=False)
bpy.ops.import_scene.fbx(filepath=r'C:\Users\xy\Pictures\bb\yueyue.fbx')
items=[]
for obj in bpy.context.scene.objects:
    items.append(dict(name=obj.name,type=obj.type,dimensions=list(obj.dimensions),bounds=[list(obj.matrix_world @ __import__('mathutils').Vector(v)) for v in obj.bound_box] if obj.type=='MESH' else [],vertices=len(obj.data.vertices) if obj.type=='MESH' else 0,materials=[m.name for m in obj.data.materials] if obj.type=='MESH' else []))
report=dict(objects=items,images=[dict(name=i.name,path=i.filepath,packed=bool(i.packed_file),size=list(i.size)) for i in bpy.data.images])
Path('Artifacts/yueyue-model-inspection.json').write_text(json.dumps(report,indent=2),encoding='utf8')
bpy.ops.wm.save_as_mainfile(filepath=str(Path('Artifacts/yueyue-source.blend').resolve()))
print('YUE_MODEL_INSPECT_OK')
