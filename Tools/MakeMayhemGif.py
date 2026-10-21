"""Encode real native-player frames; keep the historical Flower Lane media intact."""
import hashlib
import json
from pathlib import Path
from PIL import Image, ImageDraw

root = Path(__file__).resolve().parent.parent
source = root / 'Artifacts/LanePreview'
captured = json.loads((source / 'timing.json').read_text(encoding='utf8'))['seconds']
indices = list(range(0, len(captured), 2))
if indices[-1] != len(captured)-1:
    indices.append(len(captured)-1)
times = [captured[i] for i in indices]
frames = []
for i in indices:
    with Image.open(source / f'frame{i:04d}.png') as frame:
        frames.append(frame.resize((800, 450), Image.Resampling.LANCZOS)
                      .convert('P', palette=Image.Palette.ADAPTIVE, colors=128))
ticks = [round((t-times[0])*100) for t in times]
durations = [max(10, (ticks[i+1]-ticks[i])*10) for i in range(len(ticks)-1)]
durations.append(durations[-1])
output = root / 'Docs/Images/mayhem-v0130.gif'
frames[0].save(output, save_all=True, append_images=frames[1:], duration=durations,
               loop=0, disposal=2, optimize=True)
for name, filename in [('map', '21-mayhem-ice-bridge'), ('draft', '31-mayhem-random-draft'),
                       ('augments', '32-mayhem-augment-choice')]:
    with Image.open(root / f'Artifacts/Smoke/{filename}.png') as frame:
        frame.resize((1280, 720), Image.Resampling.LANCZOS).save(root / f'Docs/Images/mayhem-{name}-v0130.png')
sheet = Image.new('RGB', (1440, 810))
for j, t in enumerate([0, 2, 4, 6, 8, 10, 12, 14, 17]):
    i = min(range(len(captured)), key=lambda n: abs(captured[n]-t))
    with Image.open(source / f'frame{i:04d}.png') as frame:
        frame = frame.resize((480, 270))
        ImageDraw.Draw(frame).text((10, 110), f'{captured[i]:.3f}s', fill='white')
        sheet.paste(frame, (j % 3 * 480, j // 3 * 270))
sheet.save(root / 'Artifacts/mayhem-contact-v0130.jpg')
record = {'capturedFrames': len(captured), 'frames': len(frames), 'sourceFirstSeconds': times[0], 'sourceLastSeconds': times[-1],
          'encodedSeconds': sum(durations)/1000, 'gifBytes': output.stat().st_size,
          'gifSHA256': hashlib.sha256(output.read_bytes()).hexdigest(),
          'source': 'Actual 1600x900 native player, every second frame plus final frame, 800x450 GIF; measured unscaled timestamps; no interpolated frames',
          'scenario': 'Fixed Yueyue diagnostic hero, AI controls human slot and picks augments; normal follow camera. Still images test the actual random draft and human choice.',
          'limits': 'Capture count is not game FPS. No physical controller or manual victory claim.'}
(root / 'Docs/Validation/mayhem-preview-v0130.json').write_text(json.dumps(record, ensure_ascii=False, indent=2)+'\n', encoding='utf8')
print(json.dumps(record, ensure_ascii=False))
