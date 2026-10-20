"""Encode actual Windows-player frames at measured times; no synthesized motion."""
import json
from pathlib import Path
from PIL import Image,ImageDraw
r=Path(__file__).resolve().parent.parent;s=r/'Artifacts/AliPreview';times=json.loads((s/'timing.json').read_text())['seconds'];frames=[]
for i in range(len(times)):
    with Image.open(s/f'frame{i:04d}.png') as im:frames.append(im.resize((960,540),Image.Resampling.LANCZOS).convert('P',palette=Image.Palette.ADAPTIVE,colors=192))
ticks=[round((t-times[0])*100) for t in times];duration=[max(10,(ticks[i+1]-ticks[i])*10) for i in range(len(times)-1)];duration.append(duration[-1]);out=r/'Docs/Images/ali-bubble-v0120.gif'
frames[0].save(out,save_all=True,append_images=frames[1:],duration=duration,loop=0,disposal=2,optimize=True)
sheet=Image.new('RGB',(1440,810))
for j,t in enumerate([.3,1.3,2.4,2.85,3,3.2,5.7,6.8,7.1]):
    i=min(range(len(times)),key=lambda n:abs(times[n]-t))
    with Image.open(s/f'frame{i:04d}.png') as im:
        im=im.resize((480,270));ImageDraw.Draw(im).text((10,120),f'{times[i]:.3f}s',fill='white');sheet.paste(im,(j%3*480,j//3*270))
sheet.save(r/'Artifacts/ali-contact-v0120.jpg')
for name,t in [('bubbles',2.4),('chain',3.05)]:
    i=min(range(len(times)),key=lambda n:abs(times[n]-t))
    with Image.open(s/f'frame{i:04d}.png') as im:im.save(r/f'Docs/Images/ali-{name}-v0120.png')
print(f'ALI_GIF_OK frames={len(frames)} ms={sum(duration)} bytes={out.stat().st_size}')
