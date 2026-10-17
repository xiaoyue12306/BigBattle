"""Crop actual standalone capture, retaining measured frame timing."""
import json
from pathlib import Path
from PIL import Image
root=Path(__file__).resolve().parent.parent
source=root/'Artifacts/YueyuePreview'
times=json.loads((source/'timing.json').read_text(encoding='utf-8'))['seconds']
frames=[]
for i in range(len(times)):
    with Image.open(source/f'frame{i:04d}.png') as frame:
        frames.append(frame.crop((490,205,1130,720)).resize((768,618),Image.Resampling.LANCZOS).convert('P',palette=Image.Palette.ADAPTIVE,colors=128))
ticks=[round((t-times[0])*100) for t in times]
durations=[max(10,(ticks[i+1]-ticks[i])*10) for i in range(len(ticks)-1)];durations.append(durations[-1])
out=root/'Docs/Images/yueyue-combat-v080.gif'
frames[0].save(out,save_all=True,append_images=frames[1:],duration=durations,loop=0,disposal=2,optimize=True)
print(f'YUEYUE_GIF_OK frames={len(frames)} ms={sum(durations)} bytes={out.stat().st_size}')

# A second actual-frame crop follows the caster only during the walking segment.
# This is a technical crop/scale of the recording, never a generated animation.
pixels=json.loads((source/'timing.json').read_text(encoding='utf-8'))['actorPixels']
walking=[];walk_times=[]
for i,t in enumerate(times):
    if t>=3.6:break
    p=pixels[i];x,y=p['x'],p['y']
    with Image.open(source/f'frame{i:04d}.png') as frame:
        walking.append(frame.crop((round(x-180),round(y-215),round(x+180),round(y+215))).resize((432,516),Image.Resampling.LANCZOS).convert('P',palette=Image.Palette.ADAPTIVE,colors=128))
    walk_times.append(t)
walk_ticks=[round((t-walk_times[0])*100) for t in walk_times]
walk_durations=[max(10,(walk_ticks[i+1]-walk_ticks[i])*10) for i in range(len(walk_ticks)-1)];walk_durations.append(walk_durations[-1])
walk_out=root/'Docs/Images/yueyue-walk-v080.gif'
walking[0].save(walk_out,save_all=True,append_images=walking[1:],duration=walk_durations,loop=0,disposal=2,optimize=True)
print(f'YUEYUE_WALK_GIF_OK frames={len(walking)} ms={sum(walk_durations)} bytes={walk_out.stat().st_size}')
