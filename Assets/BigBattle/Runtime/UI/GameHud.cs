using UnityEngine;

namespace BigBattle
{
    public sealed class GameHud : MonoBehaviour
    {
        GameRoot world;
        Font font;
        Texture2D rounded;
        GUIStyle panel,label;
        float scale,offsetX,offsetY;
        const float Width=1600,Height=900;
        static readonly Color Paper=new Color(.99f,.985f,.95f,.97f);
        static readonly Color Muted=new Color(.40f,.49f,.46f);
        public void Setup(GameRoot game)
        {
            world=game;font=Font.CreateDynamicFontFromOSFont(new[]{"Microsoft YaHei","SimHei","Arial"},24);
            rounded=new Texture2D(32,32,TextureFormat.RGBA32,false);rounded.name="Original rounded UI";
            for(int y=0;y<32;y++)for(int x=0;x<32;x++)
            {
                float dx=Mathf.Max(0,Mathf.Abs(x-15.5f)-7.5f),dy=Mathf.Max(0,Mathf.Abs(y-15.5f)-7.5f);
                rounded.SetPixel(x,y,new Color(1,1,1,Mathf.Clamp01(8-Mathf.Sqrt(dx*dx+dy*dy))));
            }
            rounded.Apply();
            panel=new GUIStyle {border=new RectOffset(12,12,12,12)};panel.normal.background=rounded;
            label=new GUIStyle {font=font,richText=false,wordWrap=false};
        }
        void Box(Rect rect,Color color)
        {
            if(Event.current.type!=EventType.Repaint)return;
            if(rect.height<24 || rect.width<24){Fill(rect,color);return;}
            GUI.color=color;panel.Draw(rect,GUIContent.none,false,false,false,false);GUI.color=Color.white;
        }
        void Text(Rect rect,string text,int size,Color color,TextAnchor alignment=TextAnchor.MiddleLeft,bool bold=false)
        {
            label.fontSize=size;label.normal.textColor=color;label.alignment=alignment;label.fontStyle=bold?FontStyle.Bold:FontStyle.Normal;
            GUI.Label(rect,text,label);
        }
        void Fill(Rect rect,Color color){GUI.color=color;GUI.DrawTexture(rect,Texture2D.whiteTexture);GUI.color=Color.white;}
        void OnGUI()
        {
            if(world==null || world.Player==null || panel==null || world.CameraRig.MapOverview)return;
            scale=Mathf.Min(Screen.width/Width,Screen.height/Height);offsetX=(Screen.width-Width*scale)*.5f;offsetY=(Screen.height-Height*scale)*.5f;
            GUI.matrix=Matrix4x4.TRS(new Vector3(offsetX,offsetY,0),Quaternion.identity,new Vector3(scale,scale,1));
            Header();WorldLabels();Bottom();
            if(world.IsLane)LaneOverlay();
            else if(!world.Paused && !world.Selecting && !world.PreviewOnly){Box(new Rect(30,693,300,45),Paper);Text(new Rect(44,693,280,45),world.Input.DeviceLabel=="Xbox"?"十字键 ↑  花溪大乱斗":"N  花溪大乱斗 · 全新单路地图",16,Art.Ink,TextAnchor.MiddleCenter,true);}
            var mage=world.Player as YueyuePlayer;
            if(mage!=null && mage.Charging)
            {
                Fill(new Rect(0,0,1600,9),GhostArt.Teal);Fill(new Rect(0,891,1600,9),GhostArt.Blue);
                Box(new Rect(480,610,640,64),new Color(.025f,.06f,.09f,.92f));
                Text(new Rect(490,613,620,35),mage.ChargeProgress>=1?"万魂魔剑已蓄满 · 松开 I / Y 斩出":"万魂归剑 · 按住 I / Y · 可被控制打断",23,mage.ChargeProgress>=1?Art.Yellow:GhostArt.Teal,TextAnchor.MiddleCenter,true);
                Fill(new Rect(500,658,600*mage.ChargeProgress,5),GhostArt.Teal);
            }
            if(Time.unscaledTime<world.Fx.DoomFlashUntil && !world.Paused)Fill(new Rect(0,0,Width,Height),new Color(.03f,.65f,.47f,Mathf.Clamp01((world.Fx.DoomFlashUntil-Time.unscaledTime)/.24f)*.28f));
            if(Time.unscaledTime<world.Fx.NoticeUntil && !world.Paused && (world.State==SessionState.Practice || world.State==SessionState.Battle))
            {
                Box(new Rect(460,145,680,46),new Color(.12f,.25f,.22f,.90f));
                Text(new Rect(470,145,660,46),world.Fx.NoticeText,18,Color.white,TextAnchor.MiddleCenter);
            }
            if(world.Selecting)HeroMenu();
            else if(world.Paused)PauseMenu();
            else if(world.State==SessionState.Victory || world.State==SessionState.Defeat)Result();
            GUI.matrix=Matrix4x4.identity;
        }
        void Header()
        {
            Box(new Rect(30,27,332,92),Paper);
            Box(new Rect(45,44,44,44),Art.Yellow);
            Text(new Rect(45,44,44,44),"B",29,Art.Ink,TextAnchor.MiddleCenter,true);
            Text(new Rect(103,37,250,38),"BANANA BRAWL",26,Art.Ink,TextAnchor.MiddleLeft,true);
            Text(new Rect(104,77,230,25),"香蕉花园   /   战斗原型 01",14,Muted);
            Box(new Rect(650,30,300,71),Paper);
            string title=world.IsLane?"花溪大乱斗 · 5 对 5":world.State==SessionState.Practice?(world.PracticeFoe!=null?"英雄陪练":"自由练习"):world.State==SessionState.Battle?"花园守护战":"挑战结束";
            Text(new Rect(650,37,300,28),title,22,Art.Ink,TextAnchor.MiddleCenter,true);
            string subtitle=world.State==SessionState.Battle?"第 "+world.Wave+" / 3 波    ·    剩余 "+world.EnemyCount+" 名敌人":world.State==SessionState.Practice?(world.Duelist!=null?"相向出刀 · 双方免伤 · 金属交锋":world.PracticeFoe is AliPlayer?"十字爆泡 · 留意连锁范围":world.PracticeFoe!=null?"追魂骷髅 · 留意幽冥铁索":"熟悉身手，然后开始三波挑战"):"按 Enter / A 再战一次";
            if(world.IsLane)subtitle="英雄击败 "+world.Lane.HeroKills[0]+" : "+world.Lane.HeroKills[1]+"    "+Mathf.FloorToInt(world.BattleSeconds/60).ToString("00")+":"+Mathf.FloorToInt(world.BattleSeconds%60).ToString("00");
            Text(new Rect(650,69,300,22),subtitle,14,Muted,TextAnchor.MiddleCenter);
            Box(new Rect(1270,30,300,70),Paper);
            Text(new Rect(1288,36,268,27),"P1  ·  "+world.Input.DeviceLabel+"控制",18,Art.Ink,TextAnchor.MiddleLeft,true);
            Text(new Rect(1288,66,268,23),world.Input.DeviceLabel=="Xbox"?"左摇杆移动    Menu 暂停":"WASD 移动    Esc 暂停",14,Muted);
        }
        void Bottom()
        {
            Box(new Rect(30,758,300,111),Paper);
            Text(new Rect(48,770,260,27),world.Player.HeroName,21,Art.Ink,TextAnchor.MiddleLeft,true);
            Text(new Rect(48,804,260,22),world.PreviewOnly?"外观预览 · 基础动作已接入":"生命值   "+Mathf.CeilToInt(world.Player.Health)+" / "+world.Player.MaxHealth,14,Muted);
            Box(new Rect(48,837,264,12),new Color(.85f,.89f,.82f));
            float health=world.Player.Health/world.Player.MaxHealth;
            if(health>0)Box(new Rect(48,837,264*health,12),health>.3f?Art.Mint:Art.Coral);
            string[] keys=world.Input.DeviceLabel=="Xbox"?new[]{"X","A","B","LB","Y"}:new[]{"J","K","L","空格","I"};
            string[] icons={"/","»","~","↗","◎"};
            for(int i=0;i<5;i++)
            {
                float x=487+i*128;float remaining=world.Player.Remaining(i);
                Box(new Rect(x,748,116,123),Paper);
                Box(new Rect(x+10,759,38,27),i==3?Art.Mint:Art.Yellow);
                Text(new Rect(x+10,759,38,27),keys[i],14,Art.Ink,TextAnchor.MiddleCenter,true);
                Text(new Rect(x+56,753,48,40),world.PreviewOnly || world.Player is AliPlayer && i>0?"—":remaining>.05f?remaining.ToString("0.0"):world.Player is AliPlayer?"●":icons[i],remaining>.05f?21:28,world.PreviewOnly || remaining>.05f?Muted:Art.Ink,TextAnchor.MiddleCenter,true);
                Text(new Rect(x+5,800,106,26),world.Player.ActionNames[i],16,Art.Ink,TextAnchor.MiddleCenter,true);
                Text(new Rect(x+5,832,106,22),remaining>.05f?"冷却中":world.Player.ActionHint(i),12,remaining>.05f?Muted:new Color(.16f,.53f,.39f),TextAnchor.MiddleCenter);
                if(remaining>0)Fill(new Rect(x+10,862,96*(1-remaining/world.Player.Cooldown(i)),3),Art.Mint);
            }
            Box(new Rect(1250,758,320,111),Paper);
            string sessionKey=world.Input.DeviceLabel=="Xbox"?"View":"F";
            Text(new Rect(1270,771,280,28),world.PreviewOnly?"女巫 · 技能待设计":sessionKey+"  "+(world.State==SessionState.Battle?"返回自由练习":world.PreferredMode==MatchMode.FlowerLane?"开始花溪大乱斗":"开始三波挑战"),20,Art.Ink,TextAnchor.MiddleLeft,true);
            Text(new Rect(1270,808,280,22),world.PreviewOnly?"WASD / 左摇杆 · 移动与转向":world.Player is AliPlayer?"J / X · 原地放泡 · 最多五枚":world.IsLane?"花灵支援 · 逐座推进 · 摧毁花心":"方向箭头 = 技能释放方向",15,Muted);
            Text(new Rect(1270,835,280,20),world.PreviewOnly?"Esc / Menu · 切换角色":world.Player is AliPlayer?"1.8 秒引爆 · 地图四向触泡连锁":world.IsLane?"按住普攻 · 阵亡 8 秒后在基地复活":world.Player is YueyuePlayer?(world.Input.DeviceLabel=="Xbox"?"十字键右：陪练 · 左：练习充能":"G：英雄陪练 · H：练习充能"):(world.Input.DeviceLabel=="Xbox"?"十字键右：拼刀陪练":"G：拼刀陪练  ·  单人"),13,Muted);
            if(world.Player.ComboActive)
            {
                Box(new Rect(619,694,362,39),new Color(.12f,.25f,.22f,.9f));
                Text(new Rect(619,694,362,39),BananaPlayer.ComboNames[world.Player.ComboStep],19,world.Player.ComboStep==2?Art.Yellow:Art.Mint,TextAnchor.MiddleCenter,true);
            }
            Text(new Rect(480,879,650,18),world.PreviewOnly?"女巫 · 当前仅开放模型与基础动作预览，普攻和技能待设计":world.Player is AliPlayer?"原地放泡  ·  五枚上限  ·  十字扩散  ·  串起金色连锁":"向前挥刃  ·  留意红圈  ·  闪避穿梭  ·  享受一场香蕉乱斗",12,new Color(.17f,.31f,.29f),TextAnchor.MiddleCenter);
        }
        Vector2 Project(Vector3 point)
        {
            Vector3 p=world.CameraRig.Camera.WorldToScreenPoint(point);
            return new Vector2((p.x-offsetX)/scale,(Screen.height-p.y-offsetY)/scale);
        }
        void WorldLabels()
        {
            foreach(var actor in world.Actors)
            {
                if(actor==null || !actor.Alive || actor==world.Player)continue;
                Vector2 p=Project(actor.transform.position+Vector3.up*(actor.IsStructure?3.4f:actor is LaneMinion?1.2f:actor.IsDummy?2.1f:1.65f));
                if(p.x<0 || p.x>Width || p.y<110 || p.y>740)continue;
                Box(new Rect(p.x-35,p.y,70,7),new Color(.24f,.32f,.26f,.70f));
                float health=actor.Health/actor.MaxHealth;
                if(health>0)Box(new Rect(p.x-33,p.y+2,66*health,3),actor.Team==world.Player.Team?GhostArt.Teal:actor.IsDummy?Art.Yellow:Art.Coral);
                if(actor is SpiritAlly)Text(new Rect(p.x-70,p.y-25,140,22),"归来亡魂 · 半属性",14,new Color(.03f,.4f,.3f),TextAnchor.MiddleCenter,true);
                if(Time.time<actor.BoundUntil)Text(new Rect(p.x-60,p.y-48,120,22),"幽冥禁锢",16,new Color(.02f,.40f,.32f),TextAnchor.MiddleCenter,true);
                if(actor is LaneStructure){var building=(LaneStructure)actor;Text(new Rect(p.x-90,p.y-28,180,24),building.Core?"花心核心":building.Protected?"内侧守卫 · 保护中":"花灵守卫",15,Art.Ink,TextAnchor.MiddleCenter,true);}
                if(actor.IsDummy)
                {
                    Text(new Rect(p.x-90,p.y-30,180,25),actor is YueyueDuelist?"越越大王 · 幽冥陪练":actor is BananaDuelist?"香蕉陪练 · 红圈后出刀":"训练木桩",16,Art.Ink,TextAnchor.MiddleCenter,true);
                    if(actor.TotalDamageReceived>0)Text(new Rect(p.x-100,p.y+14,200,24),"累计伤害 "+Mathf.RoundToInt(actor.TotalDamageReceived),14,Muted,TextAnchor.MiddleCenter);
                }
            }
            foreach(var item in world.Fx.Numbers)
            {
                float age=Time.time-item.Born;Vector2 p=Project(item.Position+Vector3.up*age*1.5f);
                Text(new Rect(p.x-39,p.y-30,80,40),item.Text,24,new Color(.18f,.22f,.17f),TextAnchor.MiddleCenter,true);
                Text(new Rect(p.x-40,p.y-32,80,40),item.Text,24,item.Color,TextAnchor.MiddleCenter,true);
            }
        }
        void PauseMenu()
        {
            Fill(new Rect(-offsetX/scale,-offsetY/scale,Screen.width/scale,Screen.height/scale),new Color(.09f,.20f,.19f,.65f));
            Box(new Rect(555,122,490,650),Paper);
            Text(new Rect(585,142,430,48),"休息一下",32,Art.Ink,TextAnchor.MiddleCenter,true);
            Text(new Rect(585,195,430,27),"W/S 或方向键选择 · Enter 确认",15,Muted,TextAnchor.MiddleCenter);
            string[] options={"继续游戏","重新开始所选模式","返回自由练习","音效："+(world.Sound.Muted?"关":"开"),"选择角色","陪练："+(world.OpponentHero==HeroKind.Banana?"香蕉刺客":world.OpponentHero==HeroKind.Ali?"阿狸":"越越大王"),"模式："+(world.PreferredMode==MatchMode.FlowerLane?"花溪大乱斗":"三波挑战"),"退出游戏"};
            for(int i=0;i<options.Length;i++)
            {
                Rect rect=new Rect(592,239+i*55,416,47);
                if(i==world.MenuSelection)Box(rect,Art.Yellow);
                Text(rect,(i==world.MenuSelection?"›  ":"")+options[i],21,Art.Ink,TextAnchor.MiddleCenter,i==world.MenuSelection);
            }
            Text(new Rect(575,721,450,26),"Xbox：方向键选择 · A 确认 · Menu 返回",14,Muted,TextAnchor.MiddleCenter);
        }
        void HeroMenu()
        {
            Fill(new Rect(0,0,1600,900),new Color(.05f,.13f,.16f,.83f));
            Text(new Rect(300,105,1000,60),"选择你的英雄",42,Art.Cream,TextAnchor.MiddleCenter,true);
            Text(new Rect(300,167,1000,35),"W/S / 方向键 / 左摇杆选择 · Enter / A 确认",19,Art.Cream,TextAnchor.MiddleCenter);
            for(int i=0;i<GameRoot.HeroCount;i++)
            {
                float x=170+i*320;bool selected=world.HeroSelection==i;
                Box(new Rect(x-5,260,310,365),selected?(i==0 || i==3?Art.Yellow:i==1?GhostArt.Teal:new Color(.67f,.53f,.83f)):new Color(.3f,.4f,.43f));
                Box(new Rect(x,265,300,355),Paper);
                Text(new Rect(x+10,285,280,50),i==0?"香蕉刺客":i==1?"越越大王":i==2?"女巫":"阿狸",30,Art.Ink,TextAnchor.MiddleCenter,true);
                Text(new Rect(x+10,350,280,35),i==0?"灵巧双刀 · 近战连招":i==1?"追魂召唤 · 幽冥控制":i==2?"模型与动作 · 外观预览":"爆泡达人 · 十字连锁",19,Muted,TextAnchor.MiddleCenter);
                string[] lines=i==0?new[]{"三段普攻 · 月牙终结","突刺 / 香蕉皮 / 闪避","相向拼刀 · 旋转连斩"}:i==1?new[]{"追踪骷髅 · 铁索禁锢","半属性亡魂 · 三秒虚无","三魂充能 · 蓄力万魂魔剑"}:i==2?new[]{"原始外观 · 新制骨骼","待机 / 行走 / 入场招呼","普攻与技能等待设计"}:new[]{"J / X · 原地放下泡泡","1.8 秒呼吸 · 最多五枚","十字爆炸 · 碰泡连锁"};
                for(int j=0;j<3;j++)Text(new Rect(x+10,410+j*48,280,35),lines[j],17,Art.Ink,TextAnchor.MiddleCenter);
                Text(new Rect(x+10,565,280,35),selected?(i==2?"›  已选中 · 确认预览":"›  已选中 · 确认出战"):"切换选择",18,selected?new Color(.02f,.48f,.34f):Muted,TextAnchor.MiddleCenter,true);
            }
            Text(new Rect(300,680,1000,40),world.HeroSelection==2?"女巫当前仅支持移动与外观预览 · 技能确定后开放战斗":"单人模式 · 进入练习后按 N / 十字键上开启花溪大乱斗",18,Art.Cream,TextAnchor.MiddleCenter);
        }
        void Result()
        {
            Fill(new Rect(0,0,Width,Height),new Color(.10f,.22f,.19f,.48f));
            Box(new Rect(530,258,540,350),Paper);
            bool win=world.State==SessionState.Victory;
            Text(new Rect(550,284,500,37),world.IsLane?"FLOWER CREEK":win?"BANANA VICTORY!":"再来一根香蕉？",27,win?new Color(.15f,.53f,.39f):Art.Ink,TextAnchor.MiddleCenter,true);
            Text(new Rect(550,340,500,56),world.IsLane?(win?"花溪守护成功！":"花心需要休息啦"):win?"花园守护成功":"这次先休息一下",36,Art.Ink,TextAnchor.MiddleCenter,true);
            Text(new Rect(550,415,500,32),world.IsLane?"摧毁花心决胜 · 英雄击败 "+world.Lane.HeroKills[0]+" : "+world.Lane.HeroKills[1]:"击败 "+world.Score+" 名敌人   ·   用时 "+Mathf.FloorToInt(world.BattleSeconds)+" 秒",20,Muted,TextAnchor.MiddleCenter);
            Box(new Rect(606,485,388,54),Art.Yellow);
            Text(new Rect(606,485,388,54),"Enter / A  再战一次",22,Art.Ink,TextAnchor.MiddleCenter,true);
            Text(new Rect(550,555,500,26),"Esc / Menu 打开菜单，可返回练习",15,Muted,TextAnchor.MiddleCenter);
        }
        void OnDestroy(){if(rounded!=null)Destroy(rounded);if(font!=null)Destroy(font);}
        void LaneOverlay()
        {
            var mode=world.Lane;
            Box(new Rect(30,145,280,133),Paper);
            Text(new Rect(47,154,246,29),"花心守护 · 逐座推进",18,Art.Ink,TextAnchor.MiddleLeft,true);
            for(int team=0;team<2;team++)
            {
                var core=mode.Cores[team];float health=core!=null?core.Health/core.MaxHealth:0;float y=192+team*38;
                Text(new Rect(47,y-5,64,24),team==0?"我方":"敌方",14,Muted);
                Fill(new Rect(110,y+3,132,9),new Color(.79f,.84f,.74f));Fill(new Rect(110,y+3,132*health,9),team==0?Art.Mint:Art.Coral);
                for(int i=0;i<2;i++){bool alive=mode.Towers[team,i]!=null && mode.Towers[team,i].Alive;Box(new Rect(280-i*19,y-1,12,15),alive?(team==0?Art.Mint:Art.Coral):new Color(.7f,.74f,.67f));}
            }
            Text(new Rect(30,282,280,26),"无花灵支援时，建筑减伤 80%",12,Muted,TextAnchor.MiddleCenter);
            Box(new Rect(1395,156,170,235),Paper);
            Text(new Rect(1400,162,160,28),"花溪路线",17,Art.Ink,TextAnchor.MiddleCenter,true);
            for(int i=0;i<51;i++)Fill(new Rect(1428+i*2,335-i*2,4,4),new Color(.84f,.79f,.60f));
            foreach(var actor in world.Actors)
            {
                if(actor==null || !actor.Alive || !actor.gameObject.activeInHierarchy)continue;
                float x=1480+actor.transform.position.x*2.1f,y=285-actor.transform.position.z*2.1f;
                float size=actor==world.Player?10:actor.IsStructure?9:actor is LaneMinion?4:7;
                Box(new Rect(x-size*.5f,y-size*.5f,size,size),actor==world.Player?Art.Yellow:actor.Team==0?Art.Mint:Art.Coral);
            }
            Text(new Rect(1400,362,160,23),"花灵 "+mode.NextWaveIn.ToString("0")+" 秒后出发",13,Muted,TextAnchor.MiddleCenter);
            if(!world.Player.Alive && world.State==SessionState.Battle)
            {
                Box(new Rect(530,300,540,145),Paper);Text(new Rect(550,316,500,40),"休息一下，再次守护花溪",27,Art.Ink,TextAnchor.MiddleCenter,true);
                Text(new Rect(550,367,500,46),"复活倒计时  "+Mathf.CeilToInt(mode.RespawnIn(world.Player))+" 秒",30,new Color(.17f,.57f,.41f),TextAnchor.MiddleCenter,true);
            }
        }
    }
}
