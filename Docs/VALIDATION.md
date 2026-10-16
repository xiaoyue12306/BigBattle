# v0.2.0 validation record

Date: 2026-10-16 (Asia/Shanghai). Tuanjie 1.9.3 / 2022.3.62t11, Windows x64 Mono, Direct3D 11.

## Build and runtime

- Windows BuildPipeline: Succeeded, 0 errors, 0 warnings. Final build report: 105,897,471 bytes. Output: Builds/Windows/BigBattle.exe and adjacent runtime / data files.
- Runtime and Editor offline C# compilation passed; 53 independent input / viewport / XInput ABI checks passed. Actual Editor compilation additionally passed during the Windows build.
- Standalone acceptance: **55 passed, 0 failed, no runtime errors or exceptions**. Report timestamp: 2026-10-16T14:47:54Z. Copy: Docs/Validation/smoke-report.json. The subsequent build only optimized the opt-in preview recorder, leaving gameplay, input, sound, shaders and the acceptance runner unchanged.
- Original v0.1.0 coverage remains: boot, move / facing, all five combat actions, damage / cooldowns / invulnerability, assist cone, enemy chase / telegraph / strike, three finite waves / nine enemies, win / loss, reset, pause, bounds and 1–5 viewport geometry.
- New coverage: all thirteen real Mixamo tracks loaded; 48 finite, amplitude-limited sound variants; successful contact produces slash / impact / audio / hit pause; all three held-button combo steps used; swept thrust deals 42 damage; pause wins over hit pause; clock restores; mute suppresses impacts; temporary effects retire.

## Animation and visual evidence

- User supplied Sword and Shield Pack.zip (49 animations). Thirteen selected clips were imported as Generic FBX, sampled at 60Hz, proportionally mapped to banana joints, and loaded in the actual player.
- Idle, walk and run are blended; hands and feet are articulated and connected; horizontal source root motion is removed; action contact timing is retimed with hit events. The renderer remains the project's original banana, not an Adobe character replacement.
- Eight final full-size screenshots were checked: practice, combat practice / slip, pause, telegraph, win, loss, reset and combo contact. Chinese HUD / menus fit, floor does not z-fight, no magenta shaders, and ribbons / afterimages are visible.
- An opt-in standalone preview records real rendered frames and actual wall-clock timestamps. Docs/Images/combat-motion-v020.gif crops / enlarges those frames and retains timing; no AI-generated animation or interpolated motion is inserted. PNG recording overhead is not a performance benchmark.

## Sound evidence

- Twelve original DSP cues, four variants each. Separate filtered-noise whooshes, transients, low thumps, body resonance, soft footfalls and a short victory arpeggio replace the previous simple sine pips.
- Individual samples are finite and soft-limited below 0.781 amplitude. A ten-voice pool, per-cue spacing, bounded volume and mute handling constrain playback.
- The four-second exported example is mono, 44.1kHz, PCM16; measured peak 0.7841, RMS 0.0865, zero samples at clipping threshold. This verifies sample integrity, not subjective loudspeaker / headphone quality or every possible simultaneous mix.
- Docs/Audio/combat-audio-v020.wav is an authored demonstration sequence, not synchronized audio recorded with the GIF.

## Repairs during this update

- Replaced sphere-only thrust contact with a capsule swept from the attack origin, preventing targets passed during a fast frame from being missed.
- Preserved pause precedence over unscaled hit-pause recovery and reset stale pending strikes / animation state on retry.
- Removed FBX horizontal root drift before foot mapping and used a common height scale across clips.
- Fixed ghost placement to use source world transforms; faded short-lived effects and retained the custom transparent FX shader explicitly.

## Remaining practical limits

- No physical Xbox controllers were connected for this run. Mapping, rumble, unplug / reconnect and TV output still require hardware testing.
- Automated acceptance injects gameplay actions, not actual USB keyboard / controller events. A manual Editor Play Mode session is not claimed.
- Mixamo raw files and baked motion assets are local only; the public source checkout requires the user's own download to recreate the full animation build. See Docs/MIXAMO.md. The local portable build includes embedded animation data.
- Adaptation uses torso / hand / foot joint poses with rigid banana pieces, not full-body skinned humanoid deformation. Dodge uses a crouching source, throw a casting gesture, and spin adds procedural rotation. They can be replaced by dedicated clips later.
- Five simultaneous players, real split-screen cameras, co-op / versus and separate physical keyboard devices are not implemented by this update.
