# BigBattle — MVP design contract

Confirmed 2026-10-16 (Asia/Shanghai).

## Product

Bright, friendly, cute 3D arena combat viewed from a fixed, tilted overhead camera. Original banana assassin, banana-peel blades, expressive face and small limbs. References inform color/readability only; no copied assets. Native Windows x64 using installed Tuanjie 1.9.3 (2022.3.62t11). No mouse is required, including menus.

## MVP

One player, with keyboard and Xbox input interchangeable. Garden arena, immortal training dummy, three finite waves of melee enemies, victory/defeat, retry, pause, hit effects, synthesized sound, health and cooldown HUD. Fresh game starts in a safe practice area; pressing F / View starts the battle. Practice remains available from the menu.

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
| Pause / resume | Escape | Menu |
| Menu navigation / confirm | W/S or arrows, Enter/J | D-pad/stick, A |

Thrust: short damaging lunge. Peel: toss a trap with a visible landing zone; one enemy slips and takes damage. Dodge: brief invulnerability and a quick dash. Whirlwind: short multi-hit spinning area attack. Brief hit flash, restrained camera shake, optional controller rumble.

Skills have a 150 ms input buffer so a button press during short hit stun can execute on recovery. Peel stun visibly tips the mochi onto its side. The floor is a constrained plane with a circular boundary; obstacles, elevation and general-purpose physics locomotion are outside this MVP.

## Future five-player contract

Player slots own an input binding, actor/team, camera and viewport. Keyboard + three individually addressed XInput controllers supports four players; the fifth keyboard player needs either a distinct key layout or physical-device Raw Input support. MVP does NOT claim separate physical keyboards. Windows normally merges keyboard input. No online networking is planned.

Split screen: one fullscreen, two side-by-side, three/four grid, five in two rows (two above, three below). Viewport utility exists for future cameras, but joining, multiplayer UI, five active players and physical multi-controller acceptance are future work. Modes will include cooperative PvE and versus PvP. Combat filtering already uses team IDs rather than hard-coded player/enemy tags.

## Implementation choices

Built-in forward renderer, custom meshes and Unity primitives, no external package downloads or art dependencies. The banana is an original curved mesh with separate peel blades and procedural animation; replaceable by a Blender rig later. Unity keyboard input plus Windows XInput isolates controller IDs without shared global gamepad axes. Chinese UI uses Windows Microsoft YaHei dynamically, not a redistributed font. Assets generated at runtime have explicit cleanup.

## Acceptance

Editor opens with no compile errors; Windows build succeeds; standalone smoke test exercises moving/facing, all skills, invulnerability, traps, enemies, pause, reset, victory/defeat and viewport layouts; rendered screenshots are visually reviewed. Physical Xbox and television testing must be honestly marked pending until performed on hardware.
