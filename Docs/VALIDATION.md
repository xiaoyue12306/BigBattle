# v0.13.0 validation record

2026-10-21 (Asia/Shanghai), Tuanjie 1.9.3 / 2022.3.62t11, Windows x64 / Direct3D 11 / NVIDIA GeForce RTX 4060 Ti.

## Final build, source and standalone

- Final warm build **0 errors / 0 warnings / 138,069,995 bytes**, no C# / shader errors or exceptions in the final log. Existing imported hero clips and Timeline **10 assets / 44 clips / editorScrub=PASS** validate. Three source compilation configurations and **53** input/layout/XInput ABI checks pass. All four physical controller slots remain disconnected.
- Final native standalone regression **332 passed / 0 failed / zero recorded runtime errors**, UTC **2026-10-21T12:29:00.6993230Z**, [versioned report](Validation/smoke-report-v0130.json). Retains original combat, Mixamo, clash/mute/pause, souls/charge/summons, Timeline, Witch preview guards and Ali bubble/200ms-frame tests.
- New checks exercise the actual random draft, two guaranteed-change rerolls, Witch exclusion, nine AI choices, three unique offers / one fresh redraw, input navigation / confirmation, all **30** augment hooks, four paused time-based rounds, 300-second inhibitor return, tower/minion priority and provoked hero aggro, full six-objective chains, solid footprints, scaled respawn and reinforcement growth, relic pickup / delayed heal, frostgate trip / cooldown, brush targeting / blind damage, base no-heal, and mode-exit cleanup.
- Actual enhanced skills produce three skulls, a >2.3s remaining bind, a 126-HP / 52-damage / 28s Ali spirit, one **700**-damage doom release after three real kills, seven pending bubbles, seven-unit range, 1.35s fuse and **108**-damage chain-triggered bubbles. Banana finisher bonus is applied at the actual third melee contact and cannot leak into other damage; spin bonus is applied only to its own pulses.
- First expanded run was **321/5**, without runtime errors: four assertions needed isolated fixtures (prior hit stun, real hit-pause clock, release guard after spawn, rotated bubble axes); Ali's HUD also instantiated an empty bubble service after cleanup. The final code uses a side-effect-free pending-count query. Subsequent review added dedicated finisher-leak and tower-aggro tests, fixes target ranking, and validates the AI's normal held-button release path after a three-soul respawn.

## Full matches and actual rendering

- [Final three-hero AI runs](Validation/lane-simulation-v0130.json), UTC **2026-10-21T12:28:58.2110747Z**, all conclude without errors: Banana **Defeat, 478.18 game seconds / 19 waves / kills 88:101 / enemy nexus 1581.40 HP**; Yueyue **Defeat, 478.50s / 19 waves / 96:99 / enemy nexus 1800 HP**; Ali **Victory, 444.86s / 18 waves / 97:96 / allied nexus 1800 HP**. Report also preserves each side's six objective health values.
- Fixed 0.1s simulation steps, drawing disabled, 900-game-second observation cap. Earlier fixed-eight-second respawn versions reached the cap in some/all rosters despite continued fighting. Time-based 8→32s respawns, bounded stronger reinforcements and minion-supported objective priority now create actual siege windows; AI Yueyue releases stale ultimate input after spawn rather than repeatedly failing to charge. An intermediate three-run set also concluded normally. Outcomes vary; these are completion checks, not manual wins, balance proof or realtime performance.
- Inspected actual bridge overview, random draft, augment cards, normal combat and a nine-frame motion contact sheet. [Actual GIF](Images/mayhem-v0130.gif): **210 captured frames → 106 selected frames / 18.04s encoded / 7,770,453 bytes**, source clock **0.024243→17.969831s**. Every second actual 1600×900 frame plus the final frame is resized to 800×450 and encoded at measured timings; no generated / interpolated frames. Includes base approach / gate return, fighting, death and respawn countdown; no staged invulnerability. [Timing/hash record](Validation/mayhem-preview-v0130.json).
- The recording uses fixed diagnostic Yueyue, AI input and automatic augment choices; full-map still is a diagnostic overview. Separate screenshots exercise the real random draft and human three-choice panel. Capture count is not game FPS. Preview log ends **LANE_PREVIEW_OK frames=210**, without errors.

## Package, scope and handoff

- Portable **Builds/BigBattle-v0.13.0-Windows.zip: 49,816,440 bytes / 137 entries**. Complete ZIP CRC passes; launcher, Data, gameplay and Timeline hashes match the final tested folder. [Build/package hashes](Validation/build-v0130.json). Reproducible packaging: `Tools/PackageWindows.py --version 0.13.0`. Generated binaries / raw recordings remain local and ignored.
- [MAYHEM.md](MAYHEM.md) records the agreed snowy-map direction, rules, all augments, controls, compatibility names and primary Riot design references. Original procedural bridge / towers / crystals / minions use no extracted League assets. This is an adapted first version, **not a pixel-identical map or complete current-patch League simulation**. Specific timings / stats and mixed augment tiers are project choices; summoner spells are not added.
- Still **one human + nine AI**, three combat heroes; Witch remains preview-only. No super minions, gold, XP/levels, skill ranks, shop, equipment, networking or local joining. Garden practice, sparring and three waves remain separate. Late minion growth is a match rule, not hero progression.
- Building avoidance is local steering on a flat bridge, fog uses a coarse mesh and 0.15s updates, and projectiles/effects lack per-team render compositing. Procedural art, readability and long-session balance need further iteration. Physical Xbox/rumble/reconnect, two physical keyboards, target-TV audio/brightness, low-end load and five-player split screen remain untested/unimplemented.

---

# Historical v0.12.0 validation record

2026-10-20 (Asia/Shanghai), Tuanjie 1.9.3 / 2022.3.62t11, Windows x64 / Direct3D 11 / NVIDIA GeForce RTX 4060 Ti.

## Final build and source

- Final warm build: **0 errors / 0 warnings, 138,021,555 bytes**, no C# / shader errors or exceptions in the final build log. `ALI_IMPORT_OK clips=4` and existing Timeline **10 assets / 44 clips / editorScrub=PASS** succeed. Earlier first-import missing-new-type diagnostics disappear after refresh/warm build.
- Runtime, Editor-conditional runtime and Editor C# compilation pass, as do **53** pure input/layout/XInput ABI checks. Four physical controller slots are disconnected; this is automated input validation.
- Supplied Ali source: **24,987 vertices / zero bones / zero animations**, unchanged SHA-256 `cf7165ed67c4a266d6516288164bcbde784850b91112cac1cc793719eff7db7b`. Blender derivative: **23,136 vertices / 19 bones / four authored FBX clips / 2048² original color**. The 1,851-vertex figurine plinth is removed; exposed sole boundary is flattened/capped after near-view inspection. [Asset hashes](Validation/ali-assets-v0120.json).
- Gold cross reference is the user's local baozha.png, used as a visual reference for original meshes/shader, with no distributed reference pixels. [Reference hash and usage](Validation/ali-reference-v0120.json). Bubble placement/burst add eight original sound variants; all **68** clips are finite and peak-limited to 0.76.

## Standalone regression

- Final **274 passed / 0 failed / zero recorded runtime errors**, report UTC **2026-10-20T13:53:50.7157598Z**, [versioned report](Validation/smoke-report-v0120.json). Existing banana / Yueyue combat, Mixamo, soul/charge interruption, exact doom damage, Timeline, Witch preview-only guards and lane objective checks remain.
- New coverage verifies four-card wrapping, Ali mesh/four clips/shader, one-frame attack input and .22s rejection, fixed-position breathing, paused fuse/pose/shader/expansion, five pending per owner, independent owners, full 1.8s fuse, immediate slot return, once-per-bubble center/four-arm damage, diagonal/out-of-range/friendly/self/invulnerability filtering, early multi-hop chains and cross-owner team/damage attribution.
- A **200ms simulation step** deliberately jumps across the entire .15s propagation; all four distant targets and the center still each receive exactly 80 once. Afterglow cannot hit again. Dense pending reservations obey 96, finite visuals retire, warmed material count stays stable, mute suppresses new sounds, retry removes pending/wave accounting, and a lethal enemy bubble immediately clears effects when combat ends.
- Ali enters both arena and lane; two explicit Ali bots join only Ali-led rosters (one human + nine AI), bombs use the rotated map axes and bounded arms, and lane respawn retains placement cooldown/armed bombs. Real defeated Ali returns as a 90-health / 40-damage autonomous bubble summon; restored Ali training opponents award no souls.
- First run was 271/1 with zero errors: a screenshot stall made a realtime-duration held-attack assertion unreliable. The final test uses one attack-input frame and separately checks movement/cooldown, avoiding timing ambiguity while still exercising the actual input path. Final report above is authoritative.

## Complete matches, actual preview and package

- [Final three-hero AI simulation](Validation/lane-simulation-v0120.json), UTC **2026-10-20T13:52:18.2478437Z**, all conclude normally, zero errors. Banana: **Defeat**, 465.27 game seconds / 39 waves / kills 69:94 / enemy core 1261.20 HP. Yueyue: **Victory**, 418.45 seconds / 35 waves / kills 86:69 / allied core 1095 HP. Ali: **Victory**, 674.66 seconds / 56 waves / kills 162:142 / allied core 146.60 HP.
- Fixed 0.1s steps, drawing disabled, 900-game-second cap; these verify completion, not real-time performance, balance or unaided human wins. Prior runs had different outcomes/timings.
- Reviewed the actual model close-up, four-card UI, bubble/cross screenshots and captured-motion contact sheet. [Final actual recording](Images/ali-bubble-v0120.gif): **164 actual frames / 11.83s encoded / 7,401,652 bytes**, raw clock 0 → 11.76344s. Includes moving placement, five-bubble rows, breathing and multiple directional chains. The final gold/cream billows have less white noise than the first visual iteration. [Timing/hash record](Validation/ali-preview-v0120.json).
- Video frames are real 1600×900 player output resized to 960×540, encoded at measured timings with rounding/final hold; no generated/interpolated frames. Diagnostic close-up follows the hero, injected input demonstrates the ability. Counts are not game FPS. Preview log has no errors and ends `ALI_PREVIEW_OK frames=164`.
- Portable **Builds/BigBattle-v0.12.0-Windows.zip: 49,798,850 bytes / 137 entries**, complete CRC passes; launcher/Data and gameplay/Timeline assembly hashes equal the final tested folder. [Build/package/runtime hashes](Validation/build-v0120.json). Binaries, original models, Blender intermediates and raw frames remain local/ignored.

## Scope and limits

- [ALI.md](ALI.md) records confirmed controls, damage, range, ownership and rebuild hooks. Ali has exactly one designed ability; other action slots stay disabled. Witch remains preview-only. Real-time clocks, shared meshes/materials, 96 combined pending/wave reservations and finish/reset cleanup are bounded, without claiming multiplayer performance.
- The model uses basic region weights/authored motions, no professional cloth/foot IK/finger/facial work. Concurrent gold crosses may obscure targets momentarily; brightness, composition and throughput need target-TV testing. Physical Xbox/rumble/reconnection, two physical keyboards, television audio/brightness, low-end systems and five-player split screen remain untested/unimplemented as previously recorded.

---

# Historical v0.11.0 validation record

2026-10-18 (Asia/Shanghai), Tuanjie 1.9.3 / 2022.3.62t11, Windows x64 / Direct3D 11 / NVIDIA GeForce RTX 4060 Ti.

## Model, build and source

- User-supplied xgx.fbx is a static 24,800-vertex mesh with zero bones/actions. Original SHA-256 is unchanged. Blender preparation adds a 17-bone skin, three baked Idle / Walk / Greeting clips and a 2048² original-color texture. No downloaded/Mixamo Witch animation or invented skills. [Asset facts and hashes](Validation/witch-assets-v0110.json); [implementation / rebuild / limits](WITCH.md).
- Final warm Windows build: **0 errors / 0 warnings, 132,011,455 bytes**. Final log has no C# / shader errors or exceptions; `WITCH_IMPORT_OK clips=3` and the existing `BIGBATTLE_TIMELINES_OK assets=10 clips=44 editorScrub=PASS` both pass. Runtime, Editor-conditional runtime and Editor C# compilation plus **53** pure input/layout/XInput ABI checks pass.
- Three-card selection includes Witch as **preview only**. Walking/facing is supported; attacks are disabled, damage is rejected, and arena/lane/sparring entry is blocked. The preview's internal placeholder HP/speed are not confirmed combat stats. Existing two combat heroes, lane rosters, resources, damage and effects remain intact.

## Standalone and full matches

- Final standalone: **236 passed / 0 failed / zero errors**, UTC **2026-10-18T11:40:42.0702802Z**, [smoke-report-v0110.json](Validation/smoke-report-v0110.json). New coverage includes backwards three-card wrapping, actual Witch mesh / texture / FBX clips, animated leg movement, stop blending, paused pose/clock/position, no inherited skills/damage, all battle-entry guards, shared map clamp, reset and switching back to each combat hero. The report also retains the prior two-hero combat, Timeline, soul/charge/summon and lane checks.
- [Final full-match simulation](Validation/lane-simulation-v0110.json), UTC **2026-10-18T11:40:39.5770672Z**: Banana lead **Defeat**, 438.12 simulated seconds / 37 waves / kills 75:88 / enemy core 1292 HP; Yueyue lead **Victory**, 352.57 seconds / 30 waves / kills 69:61 / allied core 900 HP. Both end normally, zero errors.
- Simulation retains fixed 0.1-second steps, drawing disabled and a 900-game-second observation cap. Outcomes vary and are not manual gameplay, balance or real-time performance evidence. Witch intentionally has no simulated battle.

## Actual rendering and package

- Reviewed three-card selection, the rendered textured Witch and actual-motion contact sheet. [Actual Witch recording](Images/witch-motion-v0110.gif): **140 captured/encoded frames / 11.87 seconds encoded / 3,151,166 bytes**, raw clock 0 → 11.79385s. Shows entry greeting, forward/side/back walking, turning and idle; no generated or interpolated frames. First rendering is warmed before resetting the clip/capture clock so the greeting is included. [Timing/hash record](Validation/witch-preview-v0110.json).
- Capture uses a diagnostic portrait camera, cropped from actual 1600×900 player frames; normal gameplay keeps its overhead camera. Frame count is not a game-FPS measurement. The final preview log ends `WITCH_PREVIEW_OK frames=140` without errors.
- Portable **Builds/BigBattle-v0.11.0-Windows.zip: 47,194,610 bytes / 137 entries**. Full CRC passes; launcher/Data are present, and gameplay / Timeline assembly hashes equal the final tested folder. [Build/package hashes](Validation/build-v0110.json). Raw source, Blender intermediate files, captures and generated binaries stay local/ignored.

## Limits and next steps

- Witch's initial authored motions use region-based weights, with the long skirt mainly attached to the pelvis. No professional cloth simulation, foot IK, finger/facial animation, attack, hit or death clips are claimed; skill poses will need further weight/animation refinement after the user's skill design.
- Four physical XInput slots were disconnected. Physical Xbox/rumble/reconnection, two physical keyboards, television brightness/audio, low-end hardware and five-player split screen remain untested/unimplemented as previously recorded. Automated injected input does not replace that testing.

---

# Historical v0.10.0 validation record

2026-10-18 (Asia/Shanghai), Tuanjie 1.9.3 / 2022.3.62t11, Windows x64 Mono / Direct3D 11 / NVIDIA GeForce RTX 4060 Ti.

## Final build and authoring

- Official Timeline **1.7.7** resolves through the Tuanjie package registry. Ten real TimelineAssets contain **44 custom clips**; the Windows player includes Unity.Timeline.dll (127,488 bytes), and no editor Timeline assembly. Built-in rendering remains active; no VFX Graph / Shader Graph or SRP migration is claimed.
- Final warm build: **0 errors / 0 warnings, 125,343,511 bytes**. Final log has no C# / shader errors or authoring-preview exceptions. Editor evaluation at 0.07 → 0.9 → 0.07 seconds verifies multiple visible layers, inactive-clip clearing and backwards restoration. Log: `BIGBATTLE_TIMELINES_OK assets=10 clips=44 editorScrub=PASS`.
- The authoring check initially exposed a material-parameter initialization exception; lazy MaterialPropertyBlock creation fixes it. The validation now captures error/exception logs and rejects the build if any occur, even if the underlying director callback swallows an exception. This is batch editor evaluation, not a claim of manual Timeline-window or hardware testing.
- All ten authored file hashes remain identical across repeated builds; EnsureAssets creates missing files only. [Asset hashes / authoring record](Validation/timeline-assets-v0100.json). Existing artist curves are preserved.
- Runtime, Editor-conditional runtime and Editor source compilation pass; **53** pure input/layout/XInput ABI checks pass. CheckSource now references the resolved Timeline assembly; a fresh checkout must resolve/build the official package first.

## Standalone and full-match regression

- Final actual standalone acceptance: **222 passed / 0 failed / zero recorded errors**, UTC **2026-10-18T03:33:11.5569848Z**. [Versioned report](Validation/smoke-report-v0100.json). Adds real resource/clip survival, manual Director staging, clock advancement, paused sampling/shader clocks, effect expiry, director/root budgets, shared material reuse, reset cleanup and a held full-charge crown without automatic gameplay release.
- All prior 212 checks remain: both heroes, 13 actual local Mixamo tracks, movement/bones, combo/clash/audio mute, summons, real-kill souls, cancellation on early release/control/death, exactly 500 once per near/distant doom target, 20s cooldown, upright blade/fracture cleanup and lane objectives/respawn rules.
- [Final AI simulation](Validation/lane-simulation-v0100.json), UTC **2026-10-18T03:33:09.0927029Z**, both matches end normally with zero errors. Banana lead: **Defeat**, 317.01 simulated seconds / 27 waves, kills 45:59, enemy core 1800 HP. Yueyue lead: **Defeat**, 337.54 seconds / 28 waves, kills 52:77, enemy core 1583.20 HP.
- Simulation uses actual controllers, a fixed 0.1-second step, drawing disabled and a 900-second observation cap. Winners/lengths vary between runs; this does not establish balance, manual skill or real-time performance. Match stats, damage and input rules were not retuned for the presentation update.

## Actual preview and package

- Reviewed the final 25 acceptance screenshots, both captured-motion contact sheets and charge/summon/release stills. New rune seals, spiral soul winds, orbiting fragments, staged shock rings and contact glints render alongside existing licensed flame/arcane particles. The main doom blade stays upright and its original swept damage remains covered.
- [Yueyue recording](Images/timeline-yueyue-v0100.gif): **183 actual frames / 17.62s encoded / 8,987,074 bytes**, raw last frame **17.51606s**. The final player captures real walking, normal casts, bind, summon, moving phase, full charge and release. The diagnostic close-up camera follows the actor; real defeat fixtures prepare three souls before recording. This is a showcase, not an unaided match.
- [Banana recording](Images/timeline-banana-v0100.gif): **97 actual frames / 6.22s encoded / 2,279,449 bytes**, raw last frame **6.16748s**. Includes normal three-step contacts, thrust/whirlwind, a deliberately timed real weapon clash and further contacts. Cropped/scaled from the actual 1600×900 player capture.
- GIFs retain measured frame timing, rounding and final-frame hold; no synthesized or interpolated frames. Capture counts are not game FPS. Both preview logs end successfully without errors. [Media hashes/timing](Validation/timeline-previews-v0100.json).
- Portable **Builds/BigBattle-v0.10.0-Windows.zip: 44,878,588 bytes / 138 entries**, CRC passes; exe/Data and the Timeline runtime assembly are present. Packaged gameplay assembly equals the final tested folder. [Build / package / runtime hashes](Validation/build-v0100.json). Executables, raw frames and package caches remain local/ignored.

## Scope and limits

- [TIMELINE_VFX.md](TIMELINE_VFX.md) documents the ten assets, custom tracks, editable fields and menus. Visual tracks do not deal damage or play audio. Manual directors follow scaled game time, and fully charged holds sample the completed visual frame until combat releases or cancels it. Shared meshes/materials and 96-instance / 220-feedback-root caps bound creation; previous 192-fracture and projectile limits remain.
- Original user model/texture/meta, local-only Mixamo exclusions and CC0 provenance are unchanged. New Timeline stages/geometry/shader are original project work; Riot's official shape/value/color/timing and gameplay-clarity principles are references, not extracted game assets.
- One human plus nine lane AI remains the mode limit. Physical Xbox, rumble/reconnection, two physical keyboards, television brightness/audio, low-end hardware and multiple viewports are **not tested**. Caps and native recordings do not prove a multiplayer performance budget. Further artist timing/shape refinement is now possible in Timeline; GPU VFX or pipeline migration remains separate future work.

---

# Historical v0.9.0 validation record

2026-10-17 (Asia/Shanghai), Tuanjie 1.9.3 / 2022.3.62t11, Windows x64 Mono / Direct3D 11 / NVIDIA GeForce RTX 4060 Ti.

## Final build and regression

- Final warm Windows build: **0 errors / 0 warnings, 125,138,519 bytes**. No C# or shader compilation diagnostics in the final log; the first new-script import had a transient missing-type diagnostic and refreshed successfully. [Build / package / runtime assembly hashes](Validation/build-v090.json).
- Final actual standalone acceptance: **212 passed / 0 failed / zero recorded errors**, UTC **2026-10-17T13:10:58.0179917Z**. [Versioned report](Validation/smoke-report-v090.json); the current smoke-report.json is the same report. Earlier iterations also passed 211/0 and 212/0 before final visual refinements; only this final report describes the packaged player.
- Eight additional checks cover the towering main volume and retained doom shader, forward/side/reverse/diagonal world-up orientation, shared mesh reuse, simultaneous distance-overlapping ground marks, paused emissive clocks, complete effect expiry, warmed material reuse and retry clearing the fracture budget. Existing 204 checks retain the real 13 Mixamo tracks, banana combo/clash/mute, Yueyue motions, three souls / four-second hold / early/control/death interruption, half-stat autonomous summons, exact 500 once per near/distant target, 20s release cooldown and lane objective/respawn rules.
- Final CheckSource: all three C# compile configurations and **53** input/layout/XInput ABI checks pass. All four physical controller slots are disconnected. PreviewDoom.ps1 and MakeDoomGif.py executed successfully against the final player/capture.

## Full matches

- Final [900-second-cap AI simulation](Validation/lane-simulation-v090.json), UTC **2026-10-17T13:10:04.7785720Z**, completed both matches with zero errors. Banana lead: **Defeat**, 684.86 simulated seconds / 57 waves, hero kills 125:120, enemy core 185.40 HP. Yueyue lead: **Defeat**, 456.27 seconds / 38 waves, kills 82:98, enemy core 794.40 HP.
- These use the actual combat controllers with fixed 0.1-second simulation steps and drawing disabled. Earlier runs had different winners; completion checks do not establish balance, manual skill or real-time performance. No match parameters or scripted building kills were introduced by this VFX update.

## Actual rendering and package

- Reviewed the final 25-screenshot acceptance contact sheet and full-resolution charge/wave/rift frames. The blade travels as a tall curved slash instead of a horizontal crescent. Fractures now have a broad dark bed, lower-frequency branches, raised stones and animated blue-green flames; ground stamps stop at the map floor while the actual piercing shot continues its original long range.
- [Real skill recording](Images/yueyue-doom-v090.gif): **157 frames / 9.52s encoded / 8,821,976 bytes**, source last frame **9.46009s**. Captured from the final Windows player at 1600×900 with normal time, then resized to 960×540 and encoded with measured durations. The diagnostic camera uses a wider forward view; three real-defeat fixtures prepare souls before recording. This is a skill showcase, not an unaided battle or gameplay FPS benchmark. No generated or interpolated frames.
- Actual selected frames: [charge](Images/doom-charge-v090.png), [vertical wave](Images/doom-wave-v090.png), [ground rift](Images/doom-rift-v090.png). Preview log reports DOOM_PREVIEW_OK; preview/smoke/simulation logs contain no runtime exceptions or shader errors.
- Portable **Builds/BigBattle-v0.9.0-Windows.zip: 44,807,366 bytes / 137 entries**, complete CRC pass, exe/Data present, packaged runtime assembly hash equals the final tested folder. Original captures and executables remain local/ignored. [Media timing record](Validation/doom-preview-v090.json).

## Limits and handoff

- Details and rebuild hooks: [DOOM_VFX.md](DOOM_VFX.md). Main blade and ground stamps use cached meshes and three shared doom materials; native particle clocks follow the existing explicit pause route. Ground marks have a global 192-segment cap and finite fade/destruction; retry also clears the accounting. No extra damage, terrain collision, permanent ground deformation or actual scene lights are added.
- Reuses the already licensed CC0 texture/flipbook sources; no new store acquisition is claimed. Source artwork, user FBX/texture and local Mixamo exclusions are preserved. Old v0.8 reports are now versioned separately to retain their original observations.
- One human plus nine lane AI remains the limit. Physical Xbox/rumble/reconnect, television brightness/audio, two physical keyboards, low-end hardware and multiple rendered viewports are **not tested**. The intentionally large HDR effect adds drawing cost and can obscure a nearby target momentarily; target-TV brightness and performance tuning remain next steps.

---

# Historical v0.8.0 validation record

2026-10-17 (Asia/Shanghai), Tuanjie 1.9.3 / 2022.3.62t11, Windows x64 Mono / Direct3D 11 / NVIDIA GeForce RTX 4060 Ti.

## Final tested build

- Windows build **0 errors / 0 warnings**, **125,117,879 bytes**; final log has no C# / shader compilation errors. [Build and portable hashes](Validation/build-v080.json), including the actual runtime assembly (the Unity bootstrap exe itself is shared across releases).
- Final standalone acceptance **204 passed / 0 failed / zero recorded runtime errors/exceptions**, UTC **2026-10-17T10:24:43.1989511Z**. [Report](Validation/smoke-report-v080.json). The 191 prior checks remain, with actual movement driving hip/opposite arm bones, distance-based step phase, idle recovery, paused full skeleton, three normal-attack variants and reset coverage.
- Imported model still has 13 bones / 24,932 vertices. Actual charge aligns the sword with both skin hand joints above shoulder height; all limb scales remain one. Normal damage, hard-bind/death interruption, early release, three-soul spending, half-stat autonomous summons, 20s cooldown and exactly one 500-damage hit on each near/distant swept target still pass. Banana's original Mixamo/combo/clash/sound/mute rules remain covered.
- Seven new atlases import at usable sizes; actual charge advances multiple animated layers. Pause freezes atlas frame, native particle time and seal rotation. One early run was 203/1 on the combined pause assertion; explicit emitter Pause/Play in GameRoot's pause path fixes the pending native step, with two later full runs passing 204/0. Final measured particle and seal deltas are both **0.000000**. Resume only plays previously paused active emitters; reset disables reusable effects and repeated impacts reuse shared materials and retire.
- Final CheckSource passes runtime / Editor-conditional runtime / Editor compilation and **53** pure input/layout/XInput ABI checks. FetchSpiritAssets.ps1 was executed against the pinned cached author archives, hash validation and extraction passed, and its PowerShell syntax parses cleanly.

## Complete AI matches

- [Final report](Validation/lane-simulation-v080.json), UTC **2026-10-17T10:24:40.6624086Z**, both matches complete normally, zero recorded errors. Banana lead: **Defeat**, 661.38 game seconds / 55 waves, hero kills 104:129, enemy core 155.20 HP. Yueyue lead: **Defeat**, 495.86 seconds / 42 waves, kills 92:96, enemy core 27.60 HP.
- An earlier [600-second diagnostic report](Validation/lane-simulation-600s-v080.json) stopped banana at Battle with allied core 97.40 / enemy core 405.00 HP; Yueyue reached defeat. The runner's observation limit was extended to **900 simulated seconds**, recorded in the final report, to observe full core resolution. Actual match rules were not retuned or given scripted building kills. No pass is claimed for that earlier capped run.
- The fixed 0.1-second simulation disables camera/HUD drawing. It verifies long-match completion and cleanup, not manual skill, real-time frame rate or competitive balance. Initial AI/composition balance and variable match lengths remain open issues.

## Actual animation and VFX review

- Reviewed the final 25-screenshot acceptance contact sheet plus walking/casting/charge/release stills and the normal-clock close-up recording. The supplied character still has its original appearance; the back of the magic blade now exposes an onyx face/channel instead of a solid green rectangle. New animated rings, portals, energy bursts and flame particles render without shader errors or missing textures.
- Skill/motion showcase: **181 actual captured frames / 17.68s encoded timing / 4,879,326 bytes**, [GIF](Images/yueyue-combat-v080.gif). Recorded at 1600×900 with a diagnostic follow/close-up view; actual walking out/back, turning, three basic gestures, chain, summon, moving phase, four-second hold and release. A real-defeat fixture prepares the soul record before recording; this is not an unassisted playthrough.
- Walking close-up: **37 actual source frames / 3.65s / 496,458 bytes**, [GIF](Images/yueyue-walk-v080.gif). Cropped/scaled from the same captured first 3.6s using recorded actor screen coordinates. No synthetic frames, mocap replacement or reordered animation. Sampling/encoding frame counts are not gameplay FPS measurements.
- Raw capture ends at 17.583s; GIF rounded durations include the final frame hold. Preview log reports YUEYUE_PREVIEW_OK and no exceptions/errors. Selected actual crops: yueyue-walk-still-v080.png, yueyue-cast-v080.png, yueyue-charge-v080.png and yueyue-doom-v080.png.
- Portable **Builds/BigBattle-v0.8.0-Windows.zip: 44,800,655 bytes / 137 entries**, full archive CRC pass, exe and Data present. It contains the same final Windows folder used for acceptance/preview/simulation; binaries and raw captures remain local/ignored.

## Provenance and practical limits

- Original user FBX and YueyueColor.png hashes verified unchanged against the v0.5 record. Refined game FBX is 1,972,060 bytes, 6072 arm vertices reweighted; [rig details/hash](Validation/yueyue-rig-v080.json). Original FBX, Blender intermediates and local Mixamo source/baked data remain excluded from public Git.
- Three actual CC0 author archives total 97,871,977 bytes. Seven integrated atlases contain 212 source frames; selected source and output hashes, CC0 evidence and original Auras ReadMe are in [the asset record](../AssetLibrary/VFX/SpiritAnimations/SOURCE.md). Author artwork is packed/adapted by the project; these are not claimed purchases of store VFX prefabs.
- Animation is authored for the existing compact rig, with pose blends and two-arm grip IK. No new Mixamo mocap library, professional per-vertex cleanup, planted-foot IK or cloth simulation is claimed. Large-head proportions and overlapping sleeves limit extreme poses; further skin polish remains useful.
- One human input plus nine lane AI remains the limit. Four XInput slots were disconnected: physical Xbox play/rumble/reconnect, two physical keyboards, television presentation/audio and five-view rendering are **not tested**. Shader/particle caps and a close-up clip do not prove a multiplayer performance budget. Combat stats, ownership and controls were not intentionally retuned.

---

# Historical v0.7.0 validation record

Date: 2026-10-17 (Asia/Shanghai), Tuanjie 1.9.3 / 2022.3.62t11, Windows x64 Mono / Direct3D 11 / NVIDIA GeForce RTX 4060 Ti.

## Final build and automated checks

- Final warm Windows build succeeds with **0 errors / 0 warnings**, **115,138,983 bytes**. Its log has no C# or shader errors. Earlier imports first exposed the missing built-in particle module and later new-script import ordering; the module is enabled and the final warm build is clean. Build summary / executable / portable archive hashes: [build-v070.json](Validation/build-v070.json).
- Final standalone acceptance: **191 passed / 0 failed / zero runtime errors or exceptions**, UTC **2026-10-17T08:57:32.2815471Z**. [Actual report](Validation/smoke-report.json). The old banana, Mixamo, combo, clash, sound/mute, hero, summon, charge/interruption, objective and reset coverage remains.
- Added coverage verifies all 13 downloaded textures and both retained native shaders, HDR compositor availability, actual skull emitters / cage portals / phase wake / charge particles, frozen particle clocks and rotating seals while paused, reusable charge/phase reset, impact expiry and material reuse. Existing swept ultimate tests still deliver exactly 500 once to each near / distant target, retaining three-soul cost and 20s cooldown.
- Diagonal tests verify the camera projects allied base lower-left and enemy base upper-right; lane-space bridge/courtyard bounds, actual movement, AI hero/sprout advance, defense projectile priority, two-team respawn with retained cooldowns, objective ordering / support reduction, core victory/defeat and garden return pass.
- Final CheckSource: runtime / Editor-conditional runtime / Editor C# compilation and **53** pure input / layout / XInput ABI checks pass. PrepareSoulVfx.ps1 also parses without errors. Four physical XInput slots are disconnected; these are automated input checks, not controller acceptance.

## Complete objective matches and rendered review

- Final [accelerated AI report](Validation/lane-simulation.json), UTC **2026-10-17T08:57:51.5233390Z**, completes both matches without recorded errors. Banana lead: **Defeat**, 577.54 game seconds / 48 waves, hero kills 94:109, enemy core 366.40 HP left. Yueyue lead: **Victory**, 371.50 seconds / 31 waves, kills 68:82, allied core 697.20 HP left. No direct building-kill commands are inserted in this runner. Earlier passes produced different outcomes; these are flow checks, not balance guarantees.
- Simulation disables camera/HUD drawing and uses fixed 0.1-second steps. It is not a manual playthrough, real-time performance benchmark or hardware-controller test.
- Reviewed the 25 final acceptance screenshots as a contact sheet and key effects / map frames at full size, plus actual normal-clock recordings. Corrected additive washout on the light ground, replaced solid charge lines with textured electric ribbons, and added modest quarter-resolution HDR glow. UI remains readable; the overview fits the full diagonal island; normal gameplay follows the player without rotating the route back to vertical.
- **Yueyue preview:** 137 captured frames / **11.63 seconds** encoded timing / **3,462,735 bytes**, [actual GIF](Images/yueyue-combat-v070.gif). Shows real skull/chain casts, a half-stat banana summon, moving phase, four-second hold and release. Real enemy defeat fixtures prepare the three souls before recording; this is a skill showcase, not an unassisted battle.
- **Lane preview:** 212 captured frames / **18.07 seconds** encoded timing / **9,836,621 bytes**, [actual GIF](Images/flower-lane-combat-v070.gif). One human slot is controlled by the same hero AI planner, starting from its normal base and reaching a real bridge fight. No lead teleport, scripted kills or generated frames. Both preview logs complete without errors/exceptions. Recorded frame counts describe capture sampling, not gameplay FPS.
- Actual crops: [diagonal map](Images/flower-lane-map-v070.png), [charge](Images/yueyue-charge-v070.png), [doom](Images/yueyue-doom-v070.png), [guardian defense](Images/flower-lane-defense-v070.png). Cropping/encoding uses the captured Windows frames and measured timestamps.
- Tested portable **Builds/BigBattle-v0.7.0-Windows.zip**: **43,209,002 bytes / 137 entries**, archive CRC passes, executable and Data directory present. Packaged from the same final tested Windows folder; binaries remain local/ignored.

## Online asset provenance and limits

- Actual original-publisher download: [Kenney Particle Pack](https://kenney.nl/assets/particle-pack), CC0. The 15,001,764-byte ZIP passes extraction; 80 base transparent PNGs are available locally and 13 unmodified 512×512 PNGs are integrated. Per-file and archive SHA-256, original license and source links are retained in [the asset record](../AssetLibrary/VFX/Kenney-ParticlePack/SOURCE.md). Shader, Shuriken composition and glow are authored for this project. No claim is made that paid store VFX prefabs were acquired.
- One human plus nine AI remains the mode limit. Physical Xbox play/rumble/reconnect, two physical keyboards, TV presentation/audio and five-view rendering load are **not tested**. Particle/effect caps and a normal rendered clip do not establish a multiplayer performance budget.
- No gameplay damage/control/cooldown or objective rules were intentionally retuned. Prior flat-plane collisions, initial AI balance, Yueyue rig quality and local-only Mixamo reconstruction limits remain. HDR glow adds rendering cost; tune quality on target hardware before multiplayer acceptance.

---

# Historical v0.6.0 validation record

Date: 2026-10-17 (Asia/Shanghai), Tuanjie 1.9.3 / 2022.3.62t11, Windows x64 Mono / Direct3D 11 / NVIDIA GeForce RTX 4060 Ti.

## Final build and checks

- Final Windows build succeeded with **0 errors / 0 warnings**, 111,683,423 bytes; no C# / shader errors in the final warm log. Tools/CheckSource.ps1 passes runtime / Editor-conditional / Editor compilation and **53** pure input / viewport / XInput ABI checks.
- Final standalone acceptance: **181 passed / 0 failed / no runtime errors or exceptions**, report UTC 2026-10-17T02:48:13.0048576Z. Copy: Docs/Validation/smoke-report.json. All prior 146 checks remain, with new lane / rendering / objective coverage.
- Portable Builds/BigBattle-v0.6.0-Windows.zip: **42,064,420 bytes / 137 entries**, verified executable and data directory; packaged from this same tested Windows folder. Artifacts and binaries remain local.

## Objective-mode coverage

- Actual Flower Lane match creates five heroes on each team, six structures and an independent lane boundary; player spawn / movement works beyond the old garden radius, and the following camera keeps the player visible. Separate courtyard / bridge bounds are verified.
- Real AI sprouts and a hero advance down the route. One explicit wave creates three sprouts per team; pause freezes movement and wave timer. Tower fires a real seed projectile at a nearby sprout, preferring it over an adjacent hero.
- Inner guardians / cores reject attacks while earlier objectives remain. Unsupported attacks receive 80% reduction; nearby friendly sprouts remove that reduction. Buildings reject bind / stun. Destroying outer then inner guardians exposes the enemy core, which produces victory; losing the allied core produces defeat. Match end stops reinforcement spawning.
- Actual hero deaths preserve the match, award team hero score and queue both sides for eight-second revival. Pause freezes the timer. Revival restores health at the correct team base while preserving an active skill cooldown and applying temporary spawn protection.
- Retry recreates the two teams / fresh cores / zero score. Returning to practice clears lane actors, projectiles and respawn data; the old two-enemy first wave and circular garden remain selectable.

## Full AI matches and actual frames

- Final accelerated full-match report: Docs/Validation/lane-simulation.json, UTC 2026-10-17T02:48:20.4551644Z, **two completed core defeats / zero recorded runtime errors**. Banana lead: 386.19 simulated seconds, 32 waves, hero kills 64:76, enemy core 889.60 HP left. Yueyue lead: 431.08 simulated seconds, 36 waves, kills 73:75, enemy core 883.20 HP left. These are actual autonomous objective games, without directly killing buildings in the runner.
- Simulation uses fixed 0.1s capture-time steps with rendering disabled; it exercises the normal hero / damage / projectile / minion / objective logic. It is **not** a real-time FPS test or a human victory test. Both baseline teams lost; composition and human-play balance remain open for tuning. Earlier development runs also ended, with different timings.
- A pure no-graphics launch initially could not progress the splash-screen wait. The runner now skips that wait only for simulation, retains a graphics device for shader support and disables drawing. The final simulation log has no errors / exceptions. This harness repair is separate from real rendered gameplay acceptance.
- Twenty-five numbered 1600×900 acceptance screenshots were checked across the old scenes, full map, guardian defense, revival, lane victory and lane defeat. New normal-time recording starts Yueyue from the actual base and controls it using the same AI planner; it captures traversal, bridge fighting, ranged skills, damage and tactical retreat. No teleported lead hero, directly scripted kills or synthetic frames are inserted into this final clip.
- Docs/Images/flower-lane-combat-v060.gif: **191 actual frames / 18.01 seconds / 6,575,562 bytes**, preserving measured timestamps. Actual full-map screenshot crop: flower-lane-map-v060.png; defense frame: flower-lane-defense-v060.png. The final preview log completes without errors / exceptions.
- Visual review corrected the overview focus / camera height / fog so the whole island fits without a near-plane sky band, flattened the lane curb stones, separated objective pips from health bars and kept friendly banana slash colors aligned with their team. Normal gameplay uses the following camera; the full-map view is a capture tool.

## Practical limits

- One real input player plus nine AI. This release does not add local joining, five human viewports, physical keyboard separation or networking. No Xbox is connected; real controller play / rumble / reconnect and TV image / sound remain untested.
- No equipment / economy / levels / random hero pool. Two repeated hero types, simple planar movement without solid actor / building blocking or full navigation, and initial AI balance remain prototype limits. Old model-rig and locally licensed Mixamo reconstruction limits still apply.
- Sprout / projectile / transient-effect limits bound activity, but no five-local-viewport performance claim is made. Audio remains the original sixty-variant bank; no newly downloaded store asset is claimed.

---

# Historical v0.5.0 validation record

Date: 2026-10-17 (Asia/Shanghai). Tuanjie 1.9.3 / 2022.3.62t11, Windows x64 Mono, Direct3D 11, NVIDIA GeForce RTX 4060 Ti.

## Final build and acceptance

- Final BuildPipeline succeeded: **0 errors / 0 warnings**, 111,648,167 bytes. Final warm build log has no C# / shader compile errors. Local player: Builds/Windows/BigBattle.exe. Portable: Builds/BigBattle-v0.5.0-Windows.zip, 42,050,835 bytes / 137 entries; executable and data directory verified. The packaged build is the tested build.
- Standalone acceptance: **146 passed / 0 failed / no runtime errors or exceptions**, report UTC 2026-10-17T02:18:52.0038707Z. Result copy: Docs/Validation/smoke-report.json. Runtime, Editor-conditional runtime and Editor C# compilation plus **53** pure input / viewport / XInput ABI checks pass on the final source.
- The prior banana movement, attacks, Mixamo tracks, combo / buffering, reciprocal clash timing / cancellation, damage, sound / mute, pause / retry, win / loss, three waves and bounded effect cleanup remain covered. No physical Xbox was connected to the four XInput slots.

## Yueyue evidence

- Hero selection opens without a mouse, creates the new player / action set and can return to the original 180-HP banana. The user's textured FBX loads as a skinned mesh with the authored 13-bone rig. This is a new local rig and procedural pose work, not newly downloaded Mixamo animation.
- Real held attack creates multiple skulls; each waits beside the caster before launch and its accelerating homing flight damages the forward target. Real chain projectile contact adds a two-second bind and surrounding void-chain assembly; pause freezes the duration and cast immediately starts its ten-second cooldown.
- Summoning is blocked before a real defeated-enemy record. Personal kills and an actual autonomous summon kill award owner souls; immortal dummy resets do not. A recorded 80-HP / 13-attack mochi returns with 40 HP / 6.5 attack, teal aura, autonomous attack, 25-second cooldown and an actual verified 20-second expiration.
- Killable enemy banana and Yueyue fixtures can also be returned with their respective models. Real returned-banana melee deals 12 damage; real returned-Yueyue skull deals 14 damage, with 60 / 110 max HP respectively.
- Ghost form cleanses an existing bind, rejects both damage and control, retains movement, prevents all offensive casts and summoning, starts its eight-second cooldown and loses immunity after three seconds.
- Ultimate rejects fewer than three souls. Early release and hard control cancel without spending souls or cooldown; a held key does not restart a cancelled charge before release. Ordinary damage preserves the charge. Movement during charging is 10% of normal, full four-second charge waits for release, and pause freezes progress.
- Actual release consumes three souls and starts a twenty-second cooldown. Two aligned targets each lose exactly 500 HP; a third target beyond the arena at z=30 also loses 500, with no second hit on earlier targets. New kills cannot bypass the active cooldown. Ground fissures are transient visual geometry.
- Death, active-charge reset, owner / session reset clear stale sword states, records, allies and projectiles. Reset disables old session objects immediately before Unity's deferred Destroy, protecting new actors from old attacks. The new practice opponent and original banana setup both work after switching.

## Visual evidence and practical limits

- Twenty numbered 1600×900 standalone screenshots include the original scenes plus hero selection, hovering skull, void chains, returned spirit, intangible ghost, full charge, piercing doom, cracks and Yueyue opponent. No blank or magenta rendering. Current snapshots: Docs/Images/yueyue-charge-v050.png and yueyue-doom-v050.png.
- Tools/PreviewYueyue.ps1 records real standalone frames of held skulls, chain, returned banana, moving ghost and a held / released sword. Tools/MakeYueyueGif.py preserves measured timestamps when cropping / enlarging; it does not synthesize animation frames. The capture stages three actual defeated-enemy fixtures before recording; it is a controlled demonstration, not an unassisted full battle or hardware-input test.
- Final Docs/Images/yueyue-combat-v050.gif: 153 real frames, 11.67 seconds, 3,555,162 bytes. The preview completes without logged errors / exceptions on the same final build. The 500-damage contact is separately evidenced by the standalone acceptance screenshot and exact-health checks.
- First visual review found that the doom crescent was close to edge-on and its center could be hidden below the floor. Its vertical arc orientation and shared shader progress were corrected; the black arch and three color edges now read from the overhead camera. Charge sword height / tilt, soul currents and phase silhouette were also reviewed in actual rendered frames.
- Original sixty-variant audio bank is unchanged; sample, playback and mute checks were repeated. The existing audio demo remains v0.3.0 authored audio, not a synchronized track for the new GIF. Reserved store audio remains outside the game.
- Rig weights and poses are a basic authored adaptation of a static posed mesh; interlocked hands, shoulders and scarf are not a professionally polished animation set. Model provenance / SHA-256 and exact ability rules are in Docs/YUEYUE.md and Docs/Validation/yueyue-assets.json.
- Real Xbox play / vibration / reconnect, dual physical keyboards and TV output / sound remain **NOT TESTED**. This remains one human player; practice opponents and returned allies are AI. Five-player joining / actual split-screen and a five-player performance benchmark remain future work. Public rebuild of full banana animation still requires the user's local Mixamo library.

---

# Historical v0.4.0 validation record

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
