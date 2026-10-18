"""Actual player capture, measured timing; diagnostic portrait camera, no synthesized motion."""
import json
from pathlib import Path
from PIL import Image,ImageDraw
r=Path(__file__).resolve().parent.parent;s=r/'Artifacts/WitchPreview';times=json.loads((s/'timing.json').read_text(encoding='utf-8'))['seconds'];frames=[]
crop=(500,180,1100,740)
for i in range(len(times)):
    with Image.open(s/f'frame{i:04d}.png') as im:frames.append(im.crop(crop).resize((720,672),Image.Resampling.LANCZOS).convert('P',palette=Image.Palette.ADAPTIVE,colors=192))
ticks=[round((t-times[0])*100) for t in times];durations=[max(10,(ticks[i+1]-ticks[i])*10) for i in range(len(ticks)-1)];durations.append(durations[-1]);out=r/'Docs/Images/witch-motion-v0110.gif'
frames[0].save(out,save_all=True,append_images=frames[1:],duration=durations,loop=0,disposal=2,optimize=True)
sheet=Image.new('RGB',(1440,810))
for j,t in enumerate([.3,1,2.3,3.4,4.8,5.5,6.8,9,10.9]):
    i=min(range(len(times)),key=lambda n:abs(times[n]-t))
    with Image.open(s/f'frame{i:04d}.png') as im:
        thumb=im.crop(crop).resize((480,270));ImageDraw.Draw(thumb).text((8,120),f'{times[i]:.2f}s',fill='white');sheet.paste(thumb,(j%3*480,j//3*270))
sheet.save(r/'Artifacts/witch-motion-contact.jpg')
for name,t in [('front',10.9),('greeting',1),('walk',6.8)]:
    i=min(range(len(times)),key=lambda n:abs(times[n]-t))
    with Image.open(s/f'frame{i:04d}.png') as im:im.crop(crop).save(r/f'Docs/Images/witch-{name}-v0110.png')
print(f'WITCH_GIF_OK frames={len(frames)} ms={sum(durations)} bytes={out.stat().st_size}')
