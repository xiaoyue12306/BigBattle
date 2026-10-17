# Unity 国际站获取记录

更新 2026-10-17：v0.7.0 另从 Kenney 官方直接获取 CC0 Particle Pack（80 张基础透明贴图，选 13 张实际接入越越大王）。见 [下载与许可记录](../../AssetLibrary/VFX/Kenney-ParticlePack/SOURCE.md) 及 [技能特效实现](../SOUL_VFX.md)。这不改变下面 Asset Store 候选仍未领取的事实。

2026-10-16，按用户要求改试 https://assetstore.unity.com/ 。国际站公开商品页可访问；内置浏览器创建页面仍超时，账户内领取和下载未能代操作。没有继续通过已停止的电脑操作工具控制浏览器，也没有访问他人的素材镜像。

## 已实际下载并归档

[FREE Casual Game SFX Pack — Dustyroom](https://assetstore.unity.com/packages/audio/sound-fx/free-casual-game-sfx-pack-54116)，免费，国际站页版本 1.1、8.6 MB、原始 Unity 2021.3.0。

发布者的[官网页面](https://dustyroom.com/free-casual-game-sounds/)公开提供[直接 ZIP 下载](https://dustyroom.com/casualgamesounds/DM-CGS.zip)，页面与随包许可证均声明 CC0。本次通过此正式入口获取，版本单独记为 `publisher-direct-undated`，不将商店版本号强加到未标版本的 ZIP。

- 已归档到 `AssetLibrary/Audio/Dustyroom-CasualGameSounds/`，含 50 个 WAV、原始 license.pdf 和 SOURCE.md。
- ZIP：8,891,352 字节，CRC 正常；50 个 WAV 全部可解码，均为 44.1kHz / 24-bit / Stereo，合计约 39.355 秒。
- 保留源文件；13 个文件中发现共 106 个满幅样本，挑选时检查听感与增益，没有声称素材无削波或已主观听审。
- 来源、SHA-256、时长、峰值、RMS 在 `download-report.json`。许可证 PDF 已提取文字并渲染检查，CC0 声明清晰可见。
- 适合后续 UI、休闲反馈、奖励和短促冲击候选，尚未导入游戏，也没有声称其中包含专业金属拼刀音。
- 此 CC0 官网直供包可以保留在公开仓库；原始 ZIP 仍放本地 Artifacts。依据为[发布者声明](https://dustyroom.com/free-casual-game-sounds/)和[CC0 说明](https://creativecommons.org/publicdomain/zero/1.0/)，不把此规则推广到其他商店素材。

## 已核对国际站入口，尚未领取或下载

| 资源 | 商店信息 | 适用方向与状态 |
| --- | --- | --- |
| [Simple FX — Cartoon Particles](https://assetstore.unity.com/packages/vfx/particles/simple-fx-cartoon-particles-67834) | 免费；235.4 KB；1.2.0；原始 Unity 2021.3.17 | 中国站 Simple FX 的国际入口，合并为同一资源条目。卡通尘土 / 烟花 / 烟雾；待领取、下载与实际材质验证。 |
| [Cartoon FX Remaster Free](https://assetstore.unity.com/packages/vfx/particles/cartoon-fx-remaster-free-109565) | 免费；12.8 MB；R 1.5.1；页面列 Unity 2020.3 / 2021.3 的 Built-in、URP | 更贴近本游戏的卡通命中、烟雾和魔法风格；团结 1.9.3 尚未实测。 |
| [Magic Effects FREE](https://assetstore.unity.com/packages/vfx/particles/spells/magic-effects-free-247933) | 免费；36.4 MB；1.6；页面列 Unity 2021.3 / 2022.3 三种管线 | 命中、斩击、火花、魔法等后续候选；团结 1.9.3 尚未实测。 |
| [SwordSoundPack](https://assetstore.unity.com/packages/audio/sound-fx/weapons/swordsoundpack-177824) | 免费；4.1 MB；1.0；原始 Unity 2020.1.3 | 刀剑声音候选；尚未取得文件，具体样本与听感待检查。与中国站付费“剑击和格挡”是不同商品。 |

另找到 Small Sound Kit 的[发布者 itch.io 页面](https://sound-works-12.itch.io/small-sound-kit)。普通公开 HTTP 获取返回 429，因此停止请求，没有获取其 ZIP；仍标记未下载。

## 继续获取的方法

用户可在自己的 Unity 国际商店账户中打开上表的免费商品，使用 Add to My Assets，再按商店 / 编辑器的 My Assets 下载入口保存 `.unitypackage`。推荐先领取 Cartoon FX Remaster Free、Magic Effects FREE、SwordSoundPack，再补 Simple FX。将文件放到 `Artifacts/AssetStoreInbox/` 后运行 `Tools/InventoryAssetInbox.ps1`。

账户领取状态目前均未核实。商店仅显示免费不代表原始素材可直接上传公开仓库；这些包继续执行本地保留与逐包许可核对。本次仅做储备，游戏 v0.3.0 未变化。
