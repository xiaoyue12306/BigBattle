# 0.1.0 validation record

Date: 2026-10-16 (Asia/Shanghai). Tuanjie 1.9.3 / 2022.3.62t11, Windows x64 Mono, Direct3D 11.

## Passed

- Runtime C# compilation, UNITY_EDITOR conditional compilation, and Editor tooling compilation against the installed engine assemblies: passed via Tools/CheckSource.ps1.
- 53 engine-independent checks: one-through-five viewport counts, bounds, non-overlap and full area coverage; reject zero / six players; analog drift / neutral / full / diagonal stick values; native XInput ABI structure sizes.
- Actual Editor project import and player script compilation: passed during build.
- Tools/Build.ps1 / BigBattle.Editor.ProjectBuilder.Build: **Succeeded, 0 errors, 0 warnings, 105,748,331 bytes** reported by BuildPipeline. Output: Builds/Windows/BigBattle.exe and adjacent data/runtime files.
- Final Tools/Smoke.ps1: **45 passed, 0 failed, no runtime errors or exceptions**. Report timestamp: 2026-10-16T14:01:24Z. A copy is committed as Docs/Validation/smoke-report.json.

Standalone coverage: boot to practice, orthographic camera, movement / retained facing, basic attack, thrust damage and lunge, peel landing / damage / visible enemy stun, dodge invulnerability and expiry, buffered whirlwind and repeated damage, cooldown rejection, team filtering, aim cone and rear-target rejection, pause, enemy pursuit / telegraph / damage, all three waves / nine enemies, victory, defeat, retry cleanup, health / cooldown reset, arena bounds and five-player viewport geometry.

## Visual review

All seven final 1600×900 PNGs under Artifacts/Smoke/ were opened and checked: practice, combat practice / slip, pause, enemy telegraph, victory, defeat and return to practice. Chinese labels render, HUD and menus fit, facing / attack rings appear, and no magenta shaders or ground z-fighting remain. Two representative screenshots are committed under Docs/Images/.

## Resolved during validation

- Initially the Editor license was unavailable; user activated it in Tuanjie Hub and subsequent builds succeeded.
- Added the built-in Screen Capture module required by the actual Editor compiler.
- Removed editor-only internal text / colored shaders from Always Included Shaders; their DontSave references prevented player serialization. Standard and Unlit/Color remain explicitly retained for generated art.
- Added a 150 ms skill buffer after standalone validation exposed a button press lost during brief hit stun.
- Hidden Windows game windows did not present rendered frames. The acceptance script now opens the interactive game window and rejects blank screenshots rather than accepting black PNGs.
- Separated coplanar sand / grass surfaces to remove z-fighting, adjusted lighting, corrected short health-bar drawing and removed stale wave notices on result screens.

## Not tested / not implemented

- No XInput slots were connected during validation. Physical Xbox mapping, rumble, hot unplug / reconnect, and TV output remain pending hardware testing.
- Automatic tests inject gameplay actions; they do not simulate real keyboard USB events or physical controller buttons.
- Multiplayer joining, simultaneous players, five active cameras, PvP / cooperative sessions and independent physical keyboard input are future features. Only input binding and viewport foundations exist in this MVP.
- No manual Editor Play Mode session is claimed; project import / compilation and the actual standalone player were verified.

## Next acceptance steps

1. Run the player normally with a connected Xbox controller; test all actions, menus, retry, unplug / reconnect and vibration.
2. Test the same build on the intended TV and assess character / HUD readability and input latency.
3. Implement five player slots and joining, explicit second-keyboard layout / Raw Input choice, real split-screen cameras, then cooperative and versus rules.
4. Record each subsequent update in update.md, validate, commit and push to GitHub.
