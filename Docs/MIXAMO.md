# 本地 Mixamo 动作与重建

v0.2.0 使用用户从 Mixamo 下载的 **Sword and Shield Pack**（49 个动作），其中选择 13 个进行香蕉关节姿态适配。角色仍使用本项目原创外形，没有换成人形角色。

## 在新电脑上恢复完整动画

1. 登录 [Mixamo](https://www.mixamo.com/)，下载 Sword and Shield Pack。
2. 导出选择 FBX for Unity、T-pose、30 FPS、Keyframe Reduction=none。
3. 保存为 `Artifacts/MixamoInbox/Sword and Shield Pack.zip`，或者向准备脚本传入实际路径。
4. 运行：

```powershell
.\Tools\PrepareMixamo.ps1
.\Tools\Build.ps1
.\Tools\Smoke.ps1
```

`PrepareMixamo.ps1 -Archive 'D:\path\Sword and Shield Pack.zip'` 可使用其他位置的压缩包。已准备文件后，编辑器菜单 Tools > BigBattle > 3. Bake local Mixamo motions 也能重新烘焙。

## 适配方式

原版角色不是 Humanoid 蒙皮模型。本次采用 Generic FBX 离线采样：读取躯干、手、前臂与脚的姿态，以统一身高归一化，移除行进根位移，映射到香蕉躯干、短臂、手掌和脚部关节。以 60Hz 烘焙本地姿态数据，运行时插值并混合待机 / 走跑 / 技能。不是把模型声明成 Humanoid 后强行套用 Avatar，也没有上传香蕉模型到 Adobe。

技能动作按挥臂速度峰值定位接触段，再重定时到游戏动作时长的 27%。伤害、刀光和命中音效在相应事件点触发；移动与攻击的关节姿态平滑过渡。旋转连斩在 Mixamo 动作姿态上增加旋转，闪避姿态配合真实游戏位移与残影。

| 游戏动作 | 动作包文件（均有 sword and shield 前缀） |
| --- | --- |
| Idle / Walk / Run | idle / walk / run |
| Attack 1 / 2 / 3 | slash / slash (3) / attack (2) |
| Thrust / Throw | attack (4) / casting (2) |
| Dodge / Spin | crouching (3) / slash (4) |
| Hurt / Death / Victory | impact (2) / death / power up |

这是基于现有包的姿态适配：闪避使用蹲身移动，投掷使用施法手势，连斩增加程序旋转。没有将它们描述为下载到了专用翻滚或香蕉投掷动作。以后可替换成更贴合双刀与香蕉比例的素材。

## 素材与仓库

- 原始 ZIP：`Artifacts/MixamoInbox/`。
- 选择的 FBX：`Assets/LocalMixamoSource/`。
- 本地烘焙数据：`Assets/BigBattle/Resources/LocalMixamo/Library.asset`。
- 以上原始素材和动作数据均被 .gitignore 排除，不推送到公开 GitHub；代码、映射、导入脚本和游戏画面 / 动图可以同步。
- 本机 Windows 构建包含烘焙动作，解压即可玩，不要求玩家另行下载素材。
- 从公开仓库克隆后，如果没有本地动作库，游戏会使用程序动画回退。完整 v0.2.0 验收要求本地库存在，因此自动验收会明确检查 13 个动作已加载。

依据：[Adobe Mixamo FAQ](https://helpx.adobe.com/creative-cloud/faq/mixamo-faq.html) 允许将动作用于游戏；[Mixamo 分发说明](https://community.adobe.com/questions-696/mixamo-faq-licensing-royalties-ownership-eula-and-tos-589400?lang=en) 区分嵌入作品与原始素材公开分发。

## 动态预览与声音

独立程序参数 `--preview-capture` 会进入约三秒的自动动作录制，仅用于验收。全尺寸 PNG 与实际时间戳写入 `Artifacts/MotionPreview/`。运行 `python Tools/MakeMotionGif.py` 可编码动图；需要 Pillow。动图保留采样时间，仅裁切和放大游戏画面，没有重新生成动画。

`Tools/Smoke.ps1` 会导出 `Artifacts/Smoke/combat-audio-demo.wav`，用于试听本项目原创的分层合成音效；它是音效示例序列，不是动图的同步录音。
