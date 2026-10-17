# v0.4.0 validation record

Date: 2026-10-17 (Asia/Shanghai). Tuanjie 1.9.3 / 2022.3.62t11, Windows x64 Mono, Direct3D 11, NVIDIA GeForce RTX 4060 Ti.

## Build and runtime

- Windows BuildPipeline: Succeeded, **0 errors / 0 warnings**, 105,930,703 bytes. Output: Builds/Windows/BigBattle.exe with adjacent data / runtime files. Portable package: Builds/BigBattle-v0.4.0-Windows.zip (38,566,588 bytes, 136 entries, executable and data directory verified).
- Offline runtime, Editor-conditional runtime and Editor C# compilation passed; 53 independent input / viewport / XInput ABI checks passed. The real Editor build additionally compiled the project successfully.
- Final standalone acceptance: **88 passed / 0 failed / no runtime errors or exceptions**. Report timestamp: 2026-10-17T01:18:54.1145062Z. Copy: Docs/Validation/smoke-report.json. The tested build is also the packaged build.
- Prior coverage retained: thirteen Mixamo tracks; move / facing; five actions; damage / cooldown / invulnerability / assist; peel stun; enemy chase / telegraph / strike; three waves / nine enemies; win / loss; pause / retry; bounds; 1–5 viewport geometry; held three-step combo; effects / sound / hit-pause coordination; swept thrust; clock restoration; mute and effect cleanup.

## Golden moonblade effects

- The new BigBattle/BladeEnergy shader is explicitly retained and supported in the standalone build. All effects are original procedural meshes / shader, with no newly downloaded Asset Store VFX dependency. See Docs/BANANA_VFX.md.
- Real combo contact triggers the gold finisher; thrust creates its dedicated stream; peel landing emits leaves. A real ultimate creates a three-band vortex that follows movement, freezes its orientation on pause, retires after the action and clears on retry.
- Dense repeated slash / contact emissions stay within the 220 root-effect limit, reuse warmed materials and fully retire after their lifetime. This is a bounded allocation / cleanup check, not a five-player performance benchmark.
- Early validation found a fractional power of a tiny negative sin(PI), producing invalid mesh bounds, and a final LateUpdate rotation on pause. Nonnegative endpoint clamping and an explicit pause guard fixed both. A later rear-aim fixture was isolated from the nearby dummy after the added movement test; final coverage passes without changing aim-assist gameplay.

## Weapon clashes and combos

- Real practice-mode input creates a second animated, armed banana. AI windup and its eventual damaging attack were verified in the standalone player.
- A real Player.Cast against an opponent's announced swing produces one clash; neither actor loses health or gains damage received. Exactly one steel playback and clash effect occur, without a damage-impact sound.
- Reversed registration order with a 60ms contact offset also cancels both attacks. At a 95ms offset (outside the 75ms window), the first strike deals ordinary damage.
- Back-facing attacks cannot parry; distant weapons neither hit nor clash. Dodge cancels a pending strike without delayed damage. Pause freezes pending contacts; resetting the session removes the sparring partner and stale contacts.
- Long hold advances all three combo actions. A quick second tap during recovery is buffered and advances to the second attack after release. An idle delay returns the next attack to the opening move.
- Combo cadence: 0.30 / 0.34 / 0.52 seconds. Damage: 24 / 27 / 36. The third action has greater knockback, a wider gold ribbon, stardust and contact star. HUD displays the current move.
- Clash detection is an explicit facing / range / timestamp test, not a rigidbody blade simulation. Slash and thrust share the resolver; unarmed enemies, traps and ultimate area ticks do not participate. Cancelling a clash consumes those two attacks; it does not confer global immunity.

## Visual evidence

- Eleven standalone screenshots were reviewed, including thrust, the new vortex and the clash with full player HP and no damage number. Menus, results, telegraphs, combo HUD and input hints fit within the 1600x900 view; no magenta shaders or blank frames. Final vortex screenshot: Docs/Images/banana-vortex-v040.png.
- Docs/Images/combat-motion-v040.gif is a 114-frame, 6.21-second crop / enlargement of actual rendered frames, retaining recorded timing. It shows combos, thrust, ultimate, one controlled clash and follow-up attacks. No generated or interpolated animation is inserted. The final build differs from the recording only by isolation of an acceptance-test fixture.
- Preview-only one-shot injection was held across an Update so end-of-frame capture could not erase it before consumption. This fixed missing preview actions; the gameplay clash checks still use the real shared resolver.
- A coral scarf and ground ring distinguish the sparring partner from the player's mint accents. Thick mint / blue / gold crescents, white cores, dark teal fringes, crossed finishers, leaf particles and gold-white clash stars replace the former thin ribbons. The vortex follows the actor; art sizes do not change damage / hit ranges.

## Sound evidence

Audio implementation is unchanged from v0.3.0; its finite-sample, playback and mute checks were repeated in the v0.4.0 player. The separately archived CC0 library remains outside the game build.

- Fifteen original synthesized cues, four variants each: **60 clips**, all finite, peak <= 0.761. No saturator or hard clipping is used for individual clips. Filtered air replaces the coarse whoosh layer; combo swings have distinct cues; impacts have a short click and a deeper decaying body.
- Metal contacts combine damped inharmonic resonances at 1680 / 2635 / 3910 / 5480 / 7240Hz and a brief low body. Two reserved voices preserve their decay while the combo continues; ten ordinary voices, per-cue spacing and mute handling bound normal playback.
- Docs/Audio/combat-audio-v030.wav: 7 seconds, mono 44.1kHz PCM16. Measured peak 0.7644, RMS 0.0555, zero samples at the export clipping threshold. Metrics copy: Docs/Validation/audio-metrics.json.
- The demo is an authored sequence (three swings / impacts, two steel clashes, thrust, ultimate), not audio captured in sync with the GIF. Sample-integrity checks do not assert subjective speaker / headphone quality or every possible simultaneous mix.

## Remaining practical limits

- Real Xbox gameplay, rumble / reconnect, two physical keyboards and television output / sound remain **NOT TESTED**. XInput reported no connected controllers. Automation injects gameplay actions, rather than claiming USB event testing or a manual Editor Play session.
- This release remains single-player; the second armed character is AI. Five-player joining, ownership, actual split-screen cameras, co-op / PvP and physical keyboard separation remain future work.
- The thirteen Mixamo motions and their local licensing / raw-data exclusions remain as documented in Docs/MIXAMO.md. The public repository includes code and previews; the local playable package embeds the animation library. A fresh public clone needs the user's own pack to rebuild full animation support.
- The banana still uses articulated rigid pieces rather than full-body skinned deformation. Dedicated dual-knife / dodge / throw motion assets and subjective TV listening can be improved in subsequent iterations.
