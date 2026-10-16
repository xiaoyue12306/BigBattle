"""Encode actual player frames, preserving their measured wall-clock timing."""
import json
from pathlib import Path
from PIL import Image

root = Path(__file__).resolve().parent.parent
source = root / "Artifacts" / "MotionPreview"
times = json.loads((source / "timing.json").read_text(encoding="utf-8"))["seconds"]
frames = []
for index in range(len(times)):
    with Image.open(source / f"frame{index:04d}.png") as image:
        frame = image.crop((545, 220, 1055, 665)).resize((714, 623), Image.Resampling.LANCZOS)
        frames.append(frame.convert("P", palette=Image.Palette.ADAPTIVE, colors=128))
# Quantize cumulative timestamps, avoiding frame-by-frame rounding drift.
ticks = [round((stamp - times[0]) * 100) for stamp in times]
durations = [max(10, (ticks[i + 1] - ticks[i]) * 10) for i in range(len(times) - 1)]
durations.append(durations[-1])
output = root / "Docs" / "Images" / "combat-motion-v030.gif"
frames[0].save(output, save_all=True, append_images=frames[1:], duration=durations, loop=0, disposal=2, optimize=True)
print(f"MOTION_GIF_OK frames={len(frames)} duration_ms={sum(durations)} bytes={output.stat().st_size}")
