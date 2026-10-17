"""Real standalone frames, original measured timing; no generated/interpolated frames."""
import json
from pathlib import Path
from PIL import Image, ImageDraw
root=Path(__file__).resolve().parent.parent
source=root/'Artifacts/DoomPreview'
times=json.loads((source/'timing.json').read_text(encoding='utf-8'))['seconds']
frames=[]
for i in range(len(times)):
    with Image.open(source/f'frame{i:04d}.png') as frame:
        frames.append(frame.resize((960,540),Image.Resampling.LANCZOS).convert('P',palette=Image.Palette.ADAPTIVE,colors=128))
ticks=[round((t-times[0])*100) for t in times]
durations=[max(10,(ticks[i+1]-ticks[i])*10) for i in range(len(ticks)-1)];durations.append(durations[-1])
out=root/'Docs/Images/yueyue-doom-v090.gif'
frames[0].save(out,save_all=True,append_images=frames[1:],duration=durations,loop=0,disposal=2,optimize=True)
print(f'DOOM_GIF_OK frames={len(frames)} ms={sum(durations)} bytes={out.stat().st_size}')
selected=[.5,3.0,4.8,5.2,5.35,5.5,5.7,6.2,7.4]
sheet=Image.new('RGB',(1440,810))
for j,t in enumerate(selected):
    i=min(range(len(times)),key=lambda n:abs(times[n]-t))
    with Image.open(source/f'frame{i:04d}.png') as frame:
        thumb=frame.resize((480,270));ImageDraw.Draw(thumb).text((8,120),f'{times[i]:.2f}s',fill='white');sheet.paste(thumb,(j%3*480,j//3*270))
sheet.save(root/'Artifacts/doom-contact-v090.jpg')
for name,t in [('charge',4.8),('wave',5.45),('rift',6.2)]:
    i=min(range(len(times)),key=lambda n:abs(times[n]-t))
    with Image.open(source/f'frame{i:04d}.png') as frame:
        frame.save(root/f'Docs/Images/doom-{name}-v090.png')
