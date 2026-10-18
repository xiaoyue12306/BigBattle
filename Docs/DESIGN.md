# BigBattle — MVP design contract

Confirmed 2026-10-16 (Asia/Shanghai).

## Product

Bright, friendly, cute 3D arena combat viewed from a fixed, tilted overhead camera. Original banana assassin, banana-peel blades, expressive face and small limbs. References inform color/readability only; no copied assets. Native Windows x64 using installed Tuanjie 1.9.3 (2022.3.62t11). No mouse is required, including menus.

## MVP

One player, with keyboard and Xbox input interchangeable. Garden arena, immortal training dummy, three finite waves, victory/defeat, retry, pause, hit effects, synthesized sound, health and cooldown HUD. v0.5.0 opens with keyboard / Xbox hero selection, then safe practice; pressing F / View starts the battle. Practice and hero selection remain available from the menu.

Move sets facing; releasing movement retains facing. Basic attack, thrust and peel use facing with a small front-cone assist. Dodge uses raw movement/facing without assistance. Ultimate attacks around the character. No mana, XP, loot, equipment or persistent progression. Movement is camera-relative. Four skill names are independent of keyboard labels.

| Action | Keyboard | Xbox |
| --- | --- | --- |
| Move/facing | WASD | Left stick |
| Basic attack | J (hold to repeat) | X (hold to repeat) |
| Thrust | K | A |
| Banana peel | L | B |
| Dodge | Space | LB |
| Whirlwind | I | Y |
| Start arena / return to practice | F | View |
| Enter / reset blade sparring | G | D-pad right |
| Pause / resume | Escape | Menu |
| Menu navigation / confirm | W/S or arrows, Enter/J | D-pad/stick, A |

Thrust: short damaging lunge. Peel: toss a trap with a visible landing zone; one enemy slips and takes damage. Dodge: brief invulnerability and a quick dash. Whirlwind: short multi-hit spinning area attack. Brief hit flash, restrained camera shake, optional controller rumble.

Skills have a 150 ms input buffer so a button press during short hit stun can execute on recovery. Peel stun visibly tips the mochi onto its side. The floor is a constrained plane with a circular boundary; obstacles, elevation and general-purpose physics locomotion are outside this MVP.

## Future five-player contract

Player slots own an input binding, actor/team, camera and viewport. Keyboard + three individually addressed XInput controllers supports four players; the fifth keyboard player needs either a distinct key layout or physical-device Raw Input support. MVP does NOT claim separate physical keyboards. Windows normally merges keyboard input. No online networking is planned.

Split screen: one fullscreen, two side-by-side, three/four grid, five in two rows (two above, three below). Viewport utility exists for future cameras, but joining, multiplayer UI, five active players and physical multi-controller acceptance are future work. Modes will include cooperative PvE and versus PvP. Combat filtering already uses team IDs rather than hard-coded player/enemy tags.

## Implementation choices

Built-in forward renderer, custom meshes and Unity primitives; v0.7.0 adds online Kenney CC0 particle artwork and the engine-provided Shuriken module, without a third-party runtime plugin. The banana is an original curved mesh with separate peel blades and articulated torso / limbs. v0.2.0 samples 13 locally licensed Mixamo tracks into a proportional pose library, blends locomotion and actions, and keeps procedural fallback for source-only checkouts. Raw FBX and baked motion data are excluded from public Git; see Docs/MIXAMO.md. Unity keyboard input plus Windows XInput isolates controller IDs without shared global gamepad axes. Chinese UI uses Windows Microsoft YaHei dynamically, not a redistributed font. Assets generated at runtime have explicit cleanup.

v0.2.0 combat feel: three-step held-button combo (24 / 27 / 36 damage), contact events at 27% of retimed action duration, swept thrust path, 28–44ms hit pause at 0.08 time scale with unscaled recovery / pause precedence, damped knockback, short transparent slash ribbons / blade trails / afterimages, squash and recoil. Audio uses four variants for twelve original layered cues, with separate swing / hit sounds, a bounded voice pool, per-cue spacing, sample headroom and immediate mute. These improve the one-player MVP; multiplayer ownership and camera rules remain future work.

v0.3.0: combo cadence is 0.30 / 0.34 / 0.52 seconds with 240ms tap buffering, a 1.05s idle reset, distinct swing cues and mint / blue / gold ribbons. Audio now has fifteen cues / sixty variants: smooth air layers without saturation, tuned impact bodies and inharmonic damped steel resonance. Ten ordinary voices plus two reserved steel-tail voices respect mute and spacing. Samples are linearly peak-normalized to at most 0.76, rather than driven through a saturator.

Direct weapon attacks are declared during windup through MeleeResolver and resolved in GameRoot.LateUpdate after actor updates. Opposing teams clash when contact timestamps differ by at most 75ms, positions are 0.2–2.3 units apart, both aims face the other (dot > 0.35), and their aims oppose (dot < -0.15). Both pending attacks are consumed before any damage; emit one steel contact, sparks, brief recoil and hit pause. Slash and thrust share this path; ultimate, trap and unarmed mochi damage do not. This prevents update-order damage for reciprocal strikes, without granting immunity to unrelated attacks. Damage / dodge / replacement / reset cancel stale weapon contacts. A declared future strike within the window may be consumed when the earlier contact resolves. A strike can clash with only one opposing strike.

Single-player G / D-pad right starts an armed banana sparring partner with a coral scarf / ring. It follows, announces a 600ms windup, then attacks through the same resolver; it restores HP on depletion. The player can lose and retry. Safe dummy practice remains the default. This enables testing two armed characters while five-player device ownership and split-screen remain future work.

## Acceptance

v0.10.0 integrates the real official Timeline 1.7.7 package: ten editable TimelineAssets / 44 custom clips stage anticipation, contact, expansion and afterglow for both heroes. Manual PlayableDirectors follow scaled combat time; charge samples gameplay progress and holds the completed crown until release. Shared ring/rune/ray/arc/spiral/shard layers and a retained Built-in SequenceEnergy shader add shape and timing, while an exposure track also drives existing soul cards. Build creates only missing assets and validates editor forward/backward seeking; existing artist edits remain intact. Damage, audio, controls and hit timing stay owned by combat. See Docs/TIMELINE_VFX.md.

v0.9.0 replaces Yueyue's horizontal doom crescent with a shared 8.8-unit world-up 3D blade and a dedicated HDR black-core shader. Distance-stamped overlapping, branching fractures have scorch beds, rising stones and soul flames, appear only on the active map floor and expire individually in 4.6 seconds under a global 192-segment cap. A denser six-current charge and larger release explosion accompany a finite camera pullback. Presentation does not change fixed-direction travel, swept hit radius, once-per-target 500 damage, three souls, four-second hold or twenty-second release cooldown. Pause/reset, cardinal/diagonal volume orientation, shared resource reuse and complete matches require revalidation. Details: Docs/DOOM_VFX.md.

v0.8.0 matches Yueyue's bent left forearm/hand rig to the supplied static pose and refines 6072 arm vertices, retaining the same 13 bones and texture. Distance-driven full-body gait blends acceleration, stopping, pelvis/shoulder opposition, arm/wrist swing and turning; three cosmetic skull gestures and distinct chain/summon/phase/sword poses blend with locomotion. Two-arm IK follows the sword grip without limb scaling. Seven actually acquired CC0 atlases from Kutejnikov, Cethiel and para add 212 source frames, with padded frame sampling/interpolation and native sheet-animation particles. Explicit particle pause handles the pending native step when timeScale changes. Damage, control, cooldowns and soul ownership remain unchanged; close-up capture is diagnostic-only. Full source/limits/rebuild instructions: Docs/YUEYUE_MOTION.md.

v0.7.0 rotates the entire Flower Lane by 45 degrees in world space, keeping camera yaw fixed so its route reads lower-left to upper-right. Canonical lane/world transforms drive every spawn, formation, objective, retreat and movement boundary; camera look-ahead and the route map follow the same orientation. Yueyue now layers 13 actually downloaded Kenney CC0 textures into ghost-flame skulls, void portals, summon seals, a spirit wake, converging red/blue/green charge currents and a fiery doom wave. Shared materials/meshes, capped native particle systems, pause-safe clocks and reset cleanup preserve the existing combat rules. Provenance and implementation: Docs/SOUL_VFX.md. Prior unacquired Asset Store candidates remain unacquired.

v0.6.0 Flower Lane adds an original sunny single-lane island / stream / wooden bridge mode with five heroes per side (one human plus nine AI). Three flower sprouts per team spawn every 12s, two sequential guardian towers protect each core, and core destruction determines victory / defeat. Heroes revive at their own base after 8s while retaining resource / cooldown state; base healing and temporary spawn immunity support repeated fights. Structures have support-dependent damage reduction and prioritize sprouts. Movement now uses the active map boundary; the long-map camera follows the player and HUD adds route / objective / respawn information. Garden practice and the original three-wave challenge remain selectable. No equipment / economy / network / extra human-player bindings are introduced. Full rules, limits and testing: Docs/FLOWER_LANE.md.

v0.5.0 adds Yueyue, using the user's textured static FBX with a locally authored 13-bone skin and procedural caster poses. Homing skulls wait 0.35s, chains bind movement/actions for 2s (10s cooldown), one autonomous half-stat returned enemy lasts 20s (25s cooldown), and void form cleanses / rejects damage and control for 3s (8s cooldown) while preventing offense. Three real owner / ally kills fund a four-second hold/release sword, with 90% movement reduction, hard-control/death interruption, fixed-direction 500-damage-per-target piercing travel and 20s release cooldown. Ordinary damage does not cancel charge; cancellation spends neither souls nor cooldown. Practice-only H / D-pad left fills souls and clears that cooldown. Hero switching and reset clear ownership, projectiles, control and charge. Full rules, provenance, model pipeline and limits: Docs/YUEYUE.md.

v0.4.0 banana presentation uses authored, shared crescent / lance / star / leaf meshes and a Built-in BladeEnergy shader. Three combo contacts have jade / blue / gold energy blades; thrust has a dedicated path stream, whirlwind has a fixed-size following ribbon assembly, peel landing has a leaf bloom and weapon clashes have a gold starburst. Lifetimes follow scaled game time, pause freezes the vortex without a late-frame step, and retry removes live effects. Damage, ranges, audio, motion clips and clash timing retain v0.3.0 semantics. See Docs/BANANA_VFX.md. Store VFX packages remain unacquired and are not claimed as the source of these new effects.

Editor opens with no compile errors; Windows build succeeds; standalone smoke test exercises moving/facing, all skills, invulnerability, traps, enemies, pause, reset, victory/defeat and viewport layouts; rendered screenshots are visually reviewed. Physical Xbox and television testing must be honestly marked pending until performed on hardware.
