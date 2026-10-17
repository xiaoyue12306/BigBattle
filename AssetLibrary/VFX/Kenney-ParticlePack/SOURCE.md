# Kenney Particle Pack

Acquired 2026-10-17 from the original publisher, with no account or store mirror.

- Official page: https://kenney.nl/assets/particle-pack
- Direct download linked by that page: https://kenney.nl/media/pages/assets/particle-pack/f8fe0f8cb8-1677578741/kenney_particle-pack.zip
- Archive SHA-256: `B631D4B07F7002549FDCF155F01141AD482F79F3440E4E301EED49CE5F1D8958`.
- Archive license calls the pack **1.1**; the web page lists release 1.0 (2018). Preserve that distinction rather than guessing a store version.
- License: **Creative Commons Zero / CC0**; the original License.txt is retained here and alongside the runtime textures. Personal and commercial use / redistribution are permitted. Credit: Kenney Vleugels (Kenney.nl); additional template credits are in the license.
- Entire downloaded archive remains in local `Artifacts/Kenney/`; 80 base transparent 512×512 PNGs plus black-background / rotated variants and a Unity sample package. These reserve files are downloaded, not all integrated.
- **13 unmodified transparent PNGs are integrated**, in `Assets/BigBattle/Resources/SoulVfx/`: circle_02, fire_01, flame_01, light_02, magic_01, magic_03, smoke_04, spark_01, spark_06, star_04, twirl_02, slash_02, trace_05.
- `download-report.json` records archive and per-file hashes / dimensions. `Tools/PrepareSoulVfx.ps1` reproduces the selected files from the pinned original archive without touching existing Unity metadata.

These are online particle artworks adapted by BigBattle into native layered effects, not a claimed acquisition of a paid professional store prefab pack. None of the previously unacquired Asset Store VFX candidates has been silently relabeled as downloaded. Built-in Shuriken, shared mesh cards and the project-authored SoulParticle shader implement the skill compositions; no external runtime plugin or render-pipeline switch.
