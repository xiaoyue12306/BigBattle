# 越越大王 · 动作与灵魂焰流

v0.8.0，2026-10-17。原生团结 1.9.3 / Built-in / Windows x64。

## 动作

这版按用户模型一手下垂、一手在胸前竖拇指的实际姿势修正左上臂 / 前臂 / 手的静态骨骼与区域蒙皮。仍为原用户外形、原 2048² 贴图和 13 骨架，重分配 6072 个顶点的手臂权重；FBX 为 1,972,060 字节，校验见 [rig report](Validation/yueyue-rig-v080.json)。原始用户 FBX、纹理、既有 .meta 保留。`RefineYueyueRig.py` 在本机原 PrepareYueyue 输出上加工，Blender 中间文件留在 Artifacts。

- 走路采用实际位移推进步相：抬膝、收小腿、髋部换重、肩膀反摆、手腕松动与头部反向平衡。起停平滑混合，转向带轻微躯干惯性；贴边不移动时不继续原地滑步，传送 / 复活不会变成大跨步。
- 连续骷髅攻击轮换右手引魂、左手挥送、双手推送三种手势，包含准备、出手与收势；骷髅悬停结束时对应挥送阶段，攻击次数 / 伤害 / 连发间隔沿用原规则。
- 铁索是迅速拧腰、后收、前送的甩索动作；召唤是双手收拢再打开法阵；虚无是收身与轻微浮行。骨骼动画叠加实际行走，不锁玩家移动。
- 魔剑分举剑蓄力、稳住重心、双臂持剑、下劈与收势。两段关节求解使双手靠近握柄，剑跟随实际手骨；取消旧版四肢 1.18 倍拉伸。黑色剑面补齐背面与蓝绿符文，让通常看到的背面也能读成黑绿魔剑。
- `YueyueVisual` 统一拥有身体 / 骨骼呈现，`ActorVisual` 保留闪白和受击回弹，避免两个 LateUpdate 互相覆盖。暂停冻结姿态，重开 / 复活重置惯性、手势与持剑状态。召回的越越大王也使用同一行走和施法手势。

这些是为该 13 骨架编写的程序关键姿势与关节适配，不是新下载的 Mixamo 动捕，不宣称专业逐顶点蒙皮、脚部 IK 或布料模拟。大头比例和原始交叠衣袖仍约束极端动作，后续可继续精修。

## 新网络特效

实际获取 [Pavel Kutejnikov 的 Auras](https://opengameart.org/content/auras)、[Cethiel 的 Arcane Magic Effect](https://opengameart.org/content/arcane-magic-effect) 和 [para 的 Animated particle effects #2](https://opengameart.org/content/animated-particle-effects-2)。三者作者页面标注 CC0，Auras 还随包提供 CC0 ReadMe；来源、压缩包 / 每帧 / 图集 SHA-256 留在 [素材记录](../AssetLibrary/VFX/SpiritAnimations/SOURCE.md)。

新增 **7 张图集 / 212 个实际源帧**：两套 60 帧流焰 / 放射光环，四套七帧魂核 / 漩涡 / 爆发 / 闪电，及 64 帧火焰。图集只做既有素材的缩放、居中和留边；着色器在运行时转为角色的青绿 / 蓝 / 红色，并插值相邻源帧，没有 AI 生成补帧。其他未选贴图留在本地储备。

| 使用位置 | 新表现 |
| --- | --- |
| 骷髅 | 卷动奥术魂核，64 帧火焰随实际弹体留下尾流。 |
| 铁索 / 束缚 | 钩头闪电爆发，六个入口使用动态漩涡，脚下放射光环流动。 |
| 召唤 / 亡魂 / 虚无 | 扩张流焰法阵与持续动态光环，和原魂雾 / 残影叠加。 |
| 魔剑 | 双层逐帧燃烧法阵、聚能魂核、原三色电弧；释放加入闪电爆发，剑气增加漩涡和帧动画火焰。 |
| 命中 | 使用奥术连续帧星爆，随原命中时刻产生并渐隐回收。 |

保持 v0.7 共享材质 / 网格、粒子数量与短效根节点上限、HDR 柔光、一次伤害判定和静音路径。图集采样走 MaterialPropertyBlock，粒子火焰走 Shuriken Texture Sheet Animation；新增贴图不要求新插件或渲染管线。暂停入口显式暂停原生粒子，防止切 timeScale 的当前帧仍执行一次排队的粒子更新；恢复仅继续此前暂停的活动发射器。

## 重建与实际预览

普通构建直接使用已提交的 FBX / 图集，不需要重新下载。重建网络图集：`Tools/FetchSpiritAssets.ps1` 验证固定哈希并解压（仅重建 7z 需要本机 7-Zip），再用带 Pillow 的 Python 运行 `Tools/PrepareSpiritSheets.py`。重建模型：先依 YUEYUE.md 生成基础模型，再运行 Blender `--background --python Tools/RefineYueyueRig.py`；不覆盖用户原文件。

`Tools/PreviewYueyue.ps1` 录制约 17.6 秒真实近景，包含往返行走 / 转身、三次普攻、铁索、召唤、浮行和四秒蓄力释放。近景镜头仅用于此诊断录制，正常游玩镜头不变。`Tools/MakeYueyueGif.py` 依实际时间戳生成技能 GIF，并依记录的角色屏幕坐标裁切前 3.6 秒行走近景；这是实际帧裁切，不是制作替代游戏的动画。

![实际行走近景](Images/yueyue-walk-v080.gif)

![实际技能与特效](Images/yueyue-combat-v080.gif)

最终构建、验收与限制见 [VALIDATION.md](VALIDATION.md)。真实 Xbox、电视、长时间 / 多视口粒子负载仍需目标硬件测试。
