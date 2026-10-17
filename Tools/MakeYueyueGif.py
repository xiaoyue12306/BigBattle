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
out=root/'Docs/Images/yueyue-combat-v050.gif'
frames[0].save(out,save_all=True,append_images=frames[1:],duration=durations,loop=0,disposal=2,optimize=True)
print(f'YUEYUE_GIF_OK frames={len(frames)} ms={sum(durations)} bytes={out.stat().st_size}')
