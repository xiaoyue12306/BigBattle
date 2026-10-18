"""Encode actual measured standalone captures. Never synthesize or interpolate frames."""
import json
from pathlib import Path
from PIL import Image,ImageDraw
root=Path(__file__).resolve().parent.parent
def make(source,name,crop=None,size=(960,540)):
    source=root/'Artifacts'/source
    times=json.loads((source/'timing.json').read_text(encoding='utf-8'))['seconds']
    frames=[]
    for i in range(len(times)):
        with Image.open(source/f'frame{i:04d}.png') as frame:
            if crop:frame=frame.crop(crop)
            frames.append(frame.resize(size,Image.Resampling.LANCZOS).convert('P',palette=Image.Palette.ADAPTIVE,colors=128))
    ticks=[round((t-times[0])*100) for t in times]
    duration=[max(10,(ticks[i+1]-ticks[i])*10) for i in range(len(ticks)-1)];duration.append(duration[-1])
    out=root/f'Docs/Images/timeline-{name}-v0100.gif'
    frames[0].save(out,save_all=True,append_images=frames[1:],duration=duration,loop=0,disposal=2,optimize=True)
    print(f'TIMELINE_GIF_OK {name} frames={len(frames)} ms={sum(duration)} bytes={out.stat().st_size}')
    return source,times
source,times=make('YueyuePreview','yueyue')
sheet=Image.new('RGB',(1440,810))
for j,t in enumerate([4.25,5.2,6,6.9,8.5,12,14.6,15.4,15.7]):
    i=min(range(len(times)),key=lambda n:abs(times[n]-t))
    with Image.open(source/f'frame{i:04d}.png') as frame:
        thumb=frame.resize((480,270));ImageDraw.Draw(thumb).text((8,120),f'{times[i]:.2f}s',fill='white');sheet.paste(thumb,(j%3*480,j//3*270))
sheet.save(root/'Artifacts/timeline-yueyue-contact.jpg')
for name,t in [('summon',6.9),('charge',14.6),('release',15.4)]:
    i=min(range(len(times)),key=lambda n:abs(times[n]-t))
    with Image.open(source/f'frame{i:04d}.png') as frame:frame.save(root/f'Docs/Images/timeline-{name}-v0100.png')
make('MotionPreview','banana',(455,110,1165,710),(852,720))
