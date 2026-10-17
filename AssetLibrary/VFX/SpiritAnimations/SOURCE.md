# Spirit animation sources · v0.8.0

Actually acquired from the three authors' OpenGameArt attachment links on 2026-10-17. These are animated artworks used on 3D particles/cards, not claimed purchases of professional Unity prefabs. All three author pages specify **CC0**. No account, paid library mirror or storefront authentication was used.

| Author / collection | Original publisher page | Integrated content |
| --- | --- | --- |
| Pavel Kutejnikov / Auras | https://opengameart.org/content/auras | Two 60-frame white sequences: AuraRays and AuraFlames. Original ReadMe.txt with CC0 declaration preserved as Auras-ReadMe.txt. |
| Cethiel / Arcane Magic Effect | https://opengameart.org/content/arcane-magic-effect | Four seven-frame sequences: 02 orb, 04 vortex, 05 impact, 06 lightning. Recoloured by the runtime shader, without modifying source colours in the atlases. |
| para / Animated particle effects #2 | https://opengameart.org/content/animated-particle-effects-2 | Original fire_02.png, 8×8 / 64 frames, copied unchanged as ParaFire.png. |

Seven integrated atlases contain **212 frames**. Archive URL, byte counts / SHA-256, every selected source frame hash and atlas hash/layout are in download-report.json. Original archives and unselected assets stay local under Artifacts/SpiritAssets. They are acquired reserves, not all integrated. Attribution is voluntarily retained even though CC0 does not require it; see https://creativecommons.org/publicdomain/zero/1.0/.

Tools/FetchSpiritAssets.ps1 obtains the pinned archives from those public attachment URLs. Tools/PrepareSpiritSheets.py packs existing images only: Auras frames are downscaled from 512 to fit 252 pixels inside a 256-pixel padded cell; Arcane frames are centred inside 128-pixel cells. Atlas sizes are 2048² / 512×256. The original Para sheet remains 1024². Frame padding and interpolation avoid seams; this process does not invent new frames or artwork. The game does not require Python, 7-Zip, Blender, Substance or any external VFX plugin to run.

CodeManu's pack was also reviewed, but its description and license metadata differ and its pixel-art style is less suited to this character. It was not acquired or integrated. Earlier unacquired Asset Store candidates remain unacquired.
