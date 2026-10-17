"""Pack acquired CC0 frames for the native Unity shader; no generated art.

Sources are publisher attachments cached in Artifacts/SpiritAssets. This only
resizes/centres/pads existing frames and copies the selected original Para sheet.
"""
from pathlib import Path
from PIL import Image
import hashlib,json,shutil

root=Path(__file__).resolve().parent.parent
cache=root/'Artifacts/SpiritAssets'
out=root/'Assets/BigBattle/Resources/SoulVfx/Flipbooks';out.mkdir(parents=True,exist_ok=True)
record=root/'AssetLibrary/VFX/SpiritAnimations';record.mkdir(parents=True,exist_ok=True)
sources=[('Kutejnikov','auras-001-002.7z','https://opengameart.org/content/auras'),('Cethiel','Arcane_Effect.zip','https://opengameart.org/content/arcane-magic-effect'),('para','para_CC0_particlefx-2.zip','https://opengameart.org/content/animated-particle-effects-2')]
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
report={'acquired':'2026-10-17','license':'CC0','archives':[],'atlases':[]}
for author,name,page in sources:
 p=cache/name;report['archives'].append({'author':author,'page':page,'download':'https://opengameart.org/sites/default/files/'+name,'bytes':p.stat().st_size,'sha256':sha(p)})

def pack(name,files,cols,rows,cell):
 assert len(files)<=cols*rows and files
 sheet=Image.new('RGBA',(cols*cell,rows*cell))
 originals=[]
 for i,p in enumerate(files):
  with Image.open(p) as original:
   im=original.convert('RGBA');im.thumbnail((cell-4,cell-4),Image.Resampling.LANCZOS)
   sheet.paste(im,((i%cols)*cell+(cell-im.width)//2,(i//cols)*cell+(cell-im.height)//2))
   originals.append({'path':p.relative_to(cache).as_posix(),'sha256':sha(p)})
 target=out/(name+'.png');sheet.save(target)
 report['atlases'].append({'file':target.relative_to(root).as_posix(),'sha256':sha(target),'size':list(sheet.size),'columns':cols,'rows':rows,'frames':len(files),'operation':'centred RGBA frames; max cell minus 4; bilinear runtime; no recolouring','originals':originals})

pack('AuraRays',sorted((cache/'Auras/001/001_white').glob('*.png')),8,8,256)
pack('AuraFlames',sorted((cache/'Auras/002/002_white').glob('*.png')),8,8,256)
for folder,name in [('02','ArcaneOrb'),('04','ArcaneVortex'),('05','ArcaneImpact'),('06','ArcaneLightning')]:
 pack(name,sorted((cache/'Arcane'/folder).glob('*.png')),4,2,128)
p=cache/'Para/fire_02.png';target=out/'ParaFire.png';shutil.copyfile(p,target)
report['atlases'].append({'file':target.relative_to(root).as_posix(),'sha256':sha(target),'size':[1024,1024],'columns':8,'rows':8,'frames':64,'operation':'unmodified original sheet','originals':[{'path':p.relative_to(cache).as_posix(),'sha256':sha(p)}]})
shutil.copyfile(cache/'Auras/ReadMe.txt',record/'Auras-ReadMe.txt')
(record/'download-report.json').write_text(json.dumps(report,indent=2)+'\n',encoding='utf8')
print('SPIRIT_SHEETS_READY',len(report['atlases']),'atlases,',sum(a['frames'] for a in report['atlases']),'frames')
