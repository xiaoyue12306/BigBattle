# 技能特效 Timeline

v0.10.0，2026-10-18。团结 1.9.3 / Windows x64 / Built-in。

## 本轮采用 Timeline

项目正式接入 **官方 Timeline 1.7.7** 和 Director 模块，使用真实 TimelineAsset、TrackAsset、PlayableAsset、PlayableDirector 编排技能。不是自制计时器改名为 Timeline，也不是只安装包。资源位于 `Assets/BigBattle/Resources/SkillTimelines`，共 **10 份 .playable / 44 个片段**，轨道、参数和 .meta 一起提交。

当前项目保持 Built-in。VFX Graph 的官方兼容说明以 URP / HDRP 为路径，本轮选择独立于渲染管线的 Timeline；没有迁移场景到 SRP，也没有声称使用 VFX Graph / Shader Graph。新的 SequenceEnergy 是本项目手写 Shader。包由团结官方注册源解析，manifest 与 lock 固定 1.7.7，不提交 Library 中的官方包源码。

视觉参考 Riot 对游戏信息、形状、明暗、色彩和时序的要求：重要事件有清晰亮芯，装饰放在外围，蓄势、瞬间冲击和低强度余波错开，保留场景的自然明亮与可爱形状。没有提取或再分发英雄联盟的模型、贴图、音效或技能文件。

参考：[Unity Director 的 Manual 时钟](https://docs.unity3d.com/Packages/com.unity.timeline@1.7/manual/play_director.html)、[VFX Graph 兼容说明](https://docs.unity3d.com/Packages/com.unity.visualeffectgraph@14.0/manual/System-Requirements.html)、[Riot 特效基本原则](https://www.riotgames.com/en/portfolio-and-reel-suggestions)、[League 游戏清晰度](https://www.leagueoflegends.com/en-us/news/dev/clarity-in-league/)。这些是编排与视觉原则参考，不是下载的成品特效来源。

## 实际接入

| Timeline | 事件与编排 |
| --- | --- |
| SoulSkull | 原地悬停聚魂 → 加速瞬间亮芯 → 随实际弹体移动的碎魂；保留原骷髅模型 / 追踪规则。 |
| SoulCage | 铁索实际命中后，锁定冲击 → 禁锢符文 → 环绕魂晶；不提前禁锢。 |
| SoulSummon | 门扉符文 → 外层涟漪 → 上升双螺旋 → 归魂碎晶 → 成形闪光。 |
| SoulPhase | 入场涟漪、三秒符文轮廓与环绕碎魂，跟随真实虚无状态回收。 |
| SoulImpact | 短亮芯 → 魂冲波纹 → 飞散碎魂，不引入第二次命中或声音。 |
| SoulCharge | 四秒内逐层显现符文、两次向内收缩的涟漪、双螺旋魂风、悬浮魂晶、剑尖亮核与蓄满外弧；曝光轨同时驱动原 SoulVfx 贴片强度。 |
| SoulRelease | 立即高光 → 内层冲击 → 延迟外层冲击 → 破碎法阵 → 魂柱 / 碎魂 → 红色余波；曝光曲线把爆发收回为余辉。 |
| BananaSlash | 真实武器接触事件同时启动刃缘回响、反向流光和叶形碎光。 |
| BananaFinisher | 第三段接触追加金色地面冲击与前端星芒，仍为原三段连招。 |
| SteelClash | 真实拼刀接触的金白亮芯、空气冲击圈和金属火星，沿用原金属音效 / 75ms 规则。 |

![越越大王真实运行录制](Images/timeline-yueyue-v0100.gif)

![香蕉刺客真实连招与拼刀录制](Images/timeline-banana-v0100.gif)

## 编辑与重建

1. 用团结打开工程，等待 Timeline 包解析。选中上述 `.playable`，打开 **Window > Sequencing > Timeline**。
2. 在 **Tools > BigBattle > 5. Create release Timeline preview** 创建已绑定的预览对象，选中其 Director 可查看 SoulRelease。该对象只是场景中的临时美术预览，可 Undo 移除。
3. 拖动片段调整开始时刻和时长；选中片段，在 Inspector 中调整 Shape、Tint（HDR）、Opacity、Offset、From / To、Tilt、Spin、Ground、Billboard、FollowSword，以及 Envelope / Expansion 曲线。改变 Shape 后重新创建预览，以重建对应网格。
4. 颜色、大小、透明度和位移属于视觉；范围 / 伤害 / 控制 / 技能冷却仍在 Combat 代码。Timeline 可以前后拖动，失活片段会清掉，拖回时重新出现。
5. 菜单 **4. Create missing skill Timelines** 只补缺失资产；正常 Build 也只补缺失文件，**不会把现有美术修改覆盖为初始曲线**。本次连续构建前后的 10 个资产 SHA-256 一致。菜单 **6. Validate Timeline authoring preview** 检查正向 / 反向求值，构建也运行此检查；最终日志必须同时无异常并出现 `BIGBATTLE_TIMELINES_OK`。

`SkillTimelineBuilder.cs` 保存最初的原创编排方案；已存在的美术版本以 `.playable` 为准。新添 Timeline 时更新 `SkillSequence.Names`、事件挂接与诊断；从源码重建使用 `Tools/Build.ps1`。独立源码检查会引用已解析的 `Library/ScriptAssemblies/Unity.Timeline.dll`，新机器应先解析 / 构建再运行 CheckSource。

## 时钟、清理和边界

- `SkillVfxClip / SkillVfxTrack` 是真实自定义片段与混合轨；`SkillSequence` 为每个实例绑定 Director。Director 使用 **Manual**，按游戏缩放时钟显式 Evaluate，暂停不继续推进，也不会在暂停末帧偷偷走一步。混合器同时清理不活动的片段，供编辑器反向拖动。
- 魔剑 Timeline 取实际蓄力进度，在四秒末保持完整魂冠；持续按住不会自动释放、扣魂或开始冷却。硬控 / 死亡 / 取消 / 重开仍由原技能系统关闭视觉。释放、命中、召唤和拼刀只播放视觉轨，所有伤害与声音仍走原战斗入口。
- 最多 **96 个存活的序列实例**，包括持有但暂时关闭的复用效果；临时反馈仍有 220 根节点上限。短效果到轨道末尾销毁 Director / 图，角色拥有的效果跟随父状态关闭或清理。不是每次命中创建永久 TimelineAsset、网格或材质。
- `Art.SequenceSurface` 共享四边形、弧带、双螺旋和魂晶网格，所有 Timeline 层共用一个 SequenceEnergy 材质，用 MPB 传颜色 / 强度 / 时钟。原 Moonblade / DoomEnergy 和 192 段地裂预算保持。后处理沿用已有 HDR 柔光。
- 原 13 张 Kenney 贴图和七张 CC0 图集继续由 SoulVfx / 粒子系统显示；素材来源与许可没有变化。新增时序、网格和 Shader 为本项目制作。

## 验证和后续

执行 `Tools/Build.ps1`、`Tools/CheckSource.ps1`、`Tools/Smoke.ps1`、`Tools/LaneSimulation.ps1`。真实画面通过 `Tools/PreviewYueyue.ps1` 和 `Tools/PreviewCombat.ps1` 录制，再用 Python/Pillow 执行 `Tools/MakeTimelineGif.py`，保留实际时间戳，无生成 / 插值帧。越越大王录制前仍使用真实击败夹具准备魂印；近景镜头只作展示。最终结果见 [VALIDATION.md](VALIDATION.md)。

Timeline 轨道本身不是 GPU 粒子图，也不会自动降低渲染成本。当前是一位真人，Xbox / 电视与多视口性能仍待实机确认。后续可继续由美术在 Timeline 调整形状与节奏；若将来要用 VFX Graph，另行评估 URP 迁移和 GPU / 多人视口预算。
