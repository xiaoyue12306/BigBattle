# 团结资源商店素材储备

核对日期：2026-10-16。项目目标：团结 1.9.3、Windows、Built-in 渲染管线、清新可爱的俯视角战斗。下表的用途和优先级是项目选型判断；价格、包大小和兼容性来自各商品页，领取时需重新核对。

**当前收录 14 个资源条目（12 免费、2 付费），已实际获取其中 1 包、50 个 WAV。** 获取成功的是国际站同名商品的 Dustyroom 官网 CC0 直供包，保存在 `AssetLibrary/Audio/Dustyroom-CasualGameSounds/`。其余 13 项未下载；账户领取状态未核实。没有发生付款或游戏导入。完整实际下载和国际站入口见 [国际站获取记录](INTERNATIONAL.md)、[文件检查报告](download-report.json)。

中国站的账户领取 / 下载被电脑操作工具的浏览器 URL 检查阻塞；国际站公开页面可访问，但内置浏览器连接仍超时。随后通过发布者官网公开直链成功获取上述音效包。此前对 Downloads 与常用商店缓存的检查未发现可用 `.unitypackage`。下表保留中国站的 10 个候选条目，国际站新增条目与替代入口另列。

## 第一批优先领取

| 资源 / 官方商品页 | 价格 / 包大小 | 页面兼容性 | 本项目建议用途 |
| --- | --- | --- | --- |
| [小音效包](https://assetstore.u3d.cn/packages/audio/sound-fx/assetstore-package-20005577) | 免费 / 99.9 MB | 团结 1.7.0+；三种管线 | 优先挑选草地脚步、衣物运动、轻量魔法；没有将其描述成专用剑击包。44.1kHz / 16-bit WAV。 |
| [Simple FX — 卡通粒子](https://assetstore.u3d.cn/packages/vfx/particles/assetstore-package-20001064) | 免费 / 233.3 KB | 团结 1.6.5+；Built-in、URP | 卡通尘土、烟雾、烟花、爆炸和水花；优先适配闪避、命中装饰及胜利庆祝。 |
| [低聚火焰粒子](https://assetstore.u3d.cn/packages/vfx/particles/fire-explosions/assetstore-package-20004867) | 免费 / 394.5 KB | 团结 1.8.0+；Built-in、URP | 五色低多边形火焰，适合后续元素角色 / 场景。 |
| [Legacy Particle Pack](https://assetstore.u3d.cn/packages/vfx/particles/assetstore-package-20005944) | 免费 / 96.5 MB | 团结 1.9.3+；仅 Built-in | 火焰、爆炸、雨和粒子范例；优先作基础特效与参数参考，挑选符合卡通风格的效果。 |

## 免费后备：需要适配或较新编辑器

| 资源 / 官方商品页 | 价格 / 包大小 | 限制 | 本项目建议用途 |
| --- | --- | --- | --- |
| [免费音效入门包](https://assetstore.u3d.cn/packages/audio/sound-fx/assetstore-package-20006729) | 免费 / 284.1 MB | **最低团结 1.10.1，高于本项目** | 页面称含 51 个 48kHz / 24-bit WAV；后续可评估中世纪、奇幻、怪物声。纯音频是否能脱离较新编辑器使用尚未验证，不为该包升级当前工程。 |
| [Particle Pack](https://assetstore.u3d.cn/packages/vfx/particles/assetstore-package-20005957) | 免费 / 137.9 MB | 团结 1.7.0+；**仅 URP** | 火、爆炸、冰、溶解等后续参考；与 Legacy 包分开，不宣称当前 Built-in 可直接使用。 |
| [Free Fire VFX — URP](https://assetstore.u3d.cn/packages/vfx/assetstore-package-20000589) | 免费 / 8.2 MB | 团结 1.0.0+；**仅 URP** | 15 种火焰效果及循环火声；偏写实，后备优先级较低。 |
| [通用拖尾特效1 — URP](https://assetstore.u3d.cn/packages/vfx/shaders/assetstore-package-20003524) | 免费 / 315.3 KB | 团结 1.0.0+；**仅 URP** | 刀光 / 冲刺拖尾参考；Shader Graph 材质需要适配当前管线。 |

## 付费候选：已查页，未购买

| 资源 / 官方商品页 | 核对时价格 / 包大小 | 选择依据与限制 |
| --- | --- | --- |
| [剑击和格挡——战斗音效](https://assetstore.u3d.cn/packages/audio/sound-fx/weapons/assetstore-package-20000870) | ¥57.40 / 14.5 MB | 团结 1.6.0+；三管线。157 个剑击、剑对剑、挥舞、格挡、匕首等声音，44.1kHz / 16-bit WAV。最贴近香蕉刀声与拼刀需求，先试听再决定购买。 |
| [Cartoon Casual VFX Pack](https://assetstore.u3d.cn/packages/vfx/assetstore-package-20005366) | ¥99.81 / 15.9 MB | 团结 1.6.5+；Built-in / URP。231 个预制体，含受击、旋风、烟雾等多色效果。发布者另列生成式 AI 用途限制：需人工核对许可，勿把素材作为生成式 AI 输入。 |

## 领取后保存与盘点

1. 在自己的中国区商店账户中逐个打开上面的免费商品页，使用“免费获取 / 添加至我的资源”，按网站或编辑器的“我的资源”下载入口下载。页面显示的账户区域限制需自行满足。未在本轮验证登录状态和账户领取结果。
2. 将原始 `.unitypackage` / ZIP 放到 `Artifacts/AssetStoreInbox/Audio/` 或 `Artifacts/AssetStoreInbox/VFX/`。如果下载入口只允许直接导入，可先导入单独的试用工程，再保留商店缓存包；无需先导入 BigBattle。
3. 保存每个包附带的 LICENSE / README 与实际版本信息。运行 `./Tools/InventoryAssetInbox.ps1`，在 `Artifacts/AssetLibrary/inventory.json` 记录文件大小与 SHA-256。**盘点文件存在不等于领取授权、完整性或导入成功。**
4. 后续确认许可证与兼容性后，再更新 `catalog.json` 的状态；未获取条目保留 `not_acquired`。普通商店原始包只留本地；已确认 CC0 的 Dustyroom 官网包连同许可保留在公开素材目录，供后续直接使用。

本地接收目录现有 **1 个原始 ZIP**；源码素材目录另有该包的 **50 个 WAV**。不要将此数量套用到上表仍未下载的中国站条目。

## 后续接入约定

- 原始 `.unitypackage` 和未来 `Assets/ThirdPartyLocal/` 已加入忽略规则。它们应在授权范围内本地保留，不因“免费”而直接公开分发原始素材。
- 商店条款入口：[Unity 中国资源商店条款与 EULA](https://assetstore.u3d.cn/legal/terms-of-service-and-eula)。Legacy Particle Pack / Particle Pack 的商品描述还指向 Unity Companion License；以下载包内条款及实际授权核对为准。当前清单不代表已取得许可或已完成法律审查。
- 先在单独的团结 1.9.3 Built-in 试用工程检查材质、粒子、脚本和依赖，再选择性接入；不要让示例包覆盖现有 ProjectSettings 或为了储备包迁移整个项目管线。
- 适配到现有 `Feedback` / `GameSound` 接触事件，保留统一拼刀结算、静音、实例回收和暂停时钟。音效先挑破风 / 命中 / 金属格挡各 4–8 个，比较音量、尾音和高频，避免再引入沙哑层。
- 考虑未来五人分屏时的粒子数量、透明叠加、声音复音和屏幕占用。先做单人对照，再做五人压力验证。
- 本轮获取 / 整理后续资源，不修改 v0.3.0 游戏逻辑，也没有声称已用它们替换现有合成音效。
