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
            if(world==null || world.Player==null || panel==null)return;
            scale=Mathf.Min(Screen.width/Width,Screen.height/Height);offsetX=(Screen.width-Width*scale)*.5f;offsetY=(Screen.height-Height*scale)*.5f;
            GUI.matrix=Matrix4x4.TRS(new Vector3(offsetX,offsetY,0),Quaternion.identity,new Vector3(scale,scale,1));
            Header();WorldLabels();Bottom();
            if(Time.unscaledTime<world.Fx.NoticeUntil && !world.Paused && (world.State==SessionState.Practice || world.State==SessionState.Battle))
            {
                Box(new Rect(460,145,680,46),new Color(.12f,.25f,.22f,.90f));
                Text(new Rect(470,145,660,46),world.Fx.NoticeText,18,Color.white,TextAnchor.MiddleCenter);
            }
            if(world.Paused)PauseMenu();
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
            string title=world.State==SessionState.Practice?(world.Duelist!=null?"香蕉拼刀陪练":"自由练习"):world.State==SessionState.Battle?"花园守护战":"挑战结束";
            Text(new Rect(650,37,300,28),title,22,Art.Ink,TextAnchor.MiddleCenter,true);
            string subtitle=world.State==SessionState.Battle?"第 "+world.Wave+" / 3 波    ·    剩余 "+world.EnemyCount+" 只团子":world.State==SessionState.Practice?(world.Duelist!=null?"相向出刀 · 双方免伤 · 金属交锋":"熟悉身手，然后开始三波挑战"):"按 Enter / A 再战一次";
            Text(new Rect(650,69,300,22),subtitle,14,Muted,TextAnchor.MiddleCenter);
            Box(new Rect(1270,30,300,70),Paper);
            Text(new Rect(1288,36,268,27),"P1  ·  "+world.Input.DeviceLabel+"控制",18,Art.Ink,TextAnchor.MiddleLeft,true);
            Text(new Rect(1288,66,268,23),world.Input.DeviceLabel=="Xbox"?"左摇杆移动    Menu 暂停":"WASD 移动    Esc 暂停",14,Muted);
        }
        void Bottom()
        {
            Box(new Rect(30,758,300,111),Paper);
            Text(new Rect(48,770,260,27),"香蕉刺客",21,Art.Ink,TextAnchor.MiddleLeft,true);
            Text(new Rect(48,804,260,22),"生命值   "+Mathf.CeilToInt(world.Player.Health)+" / "+world.Player.MaxHealth,14,Muted);
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
                Text(new Rect(x+56,753,48,40),remaining>.05f?remaining.ToString("0.0"):icons[i],remaining>.05f?21:28,remaining>.05f?Muted:Art.Ink,TextAnchor.MiddleCenter,true);
                Text(new Rect(x+5,800,106,26),BananaPlayer.SkillNames[i],i==4?16:18,Art.Ink,TextAnchor.MiddleCenter,true);
                Text(new Rect(x+5,832,106,22),remaining>.05f?"冷却中":i==0?"按住连击":"就绪",13,remaining>.05f?Muted:new Color(.16f,.53f,.39f),TextAnchor.MiddleCenter);
                if(remaining>0)Fill(new Rect(x+10,862,96*(1-remaining/BananaPlayer.Cooldowns[i]),3),Art.Mint);
            }
            Box(new Rect(1250,758,320,111),Paper);
            string sessionKey=world.Input.DeviceLabel=="Xbox"?"View":"F";
            Text(new Rect(1270,771,280,28),sessionKey+"  "+(world.State==SessionState.Battle?"返回自由练习":"开始三波挑战"),20,Art.Ink,TextAnchor.MiddleLeft,true);
            Text(new Rect(1270,808,280,22),"方向箭头 = 技能释放方向",15,Muted);
            Text(new Rect(1270,835,280,20),world.Input.DeviceLabel=="Xbox"?"十字键右：拼刀陪练":"G：拼刀陪练  ·  单人",13,Muted);
            if(world.Player.ComboActive)
            {
                Box(new Rect(619,694,362,39),new Color(.12f,.25f,.22f,.9f));
                Text(new Rect(619,694,362,39),BananaPlayer.ComboNames[world.Player.ComboStep],19,world.Player.ComboStep==2?Art.Yellow:Art.Mint,TextAnchor.MiddleCenter,true);
            }
            Text(new Rect(480,879,650,18),"向前挥刃  ·  留意红圈  ·  闪避穿梭  ·  享受一场香蕉乱斗",12,new Color(.17f,.31f,.29f),TextAnchor.MiddleCenter);
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
                Vector2 p=Project(actor.transform.position+Vector3.up*(actor.IsDummy?2.1f:1.65f));
                Box(new Rect(p.x-35,p.y,70,7),new Color(.24f,.32f,.26f,.70f));
                float health=actor.Health/actor.MaxHealth;
                if(health>0)Box(new Rect(p.x-33,p.y+2,66*health,3),actor.IsDummy?Art.Yellow:Art.Coral);
                if(actor.IsDummy)
                {
                    Text(new Rect(p.x-90,p.y-30,180,25),actor is BananaDuelist?"香蕉陪练 · 红圈后出刀":"训练木桩",16,Art.Ink,TextAnchor.MiddleCenter,true);
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
            Box(new Rect(555,207,490,480),Paper);
            Text(new Rect(585,232,430,48),"休息一下",32,Art.Ink,TextAnchor.MiddleCenter,true);
            Text(new Rect(585,285,430,27),"W/S 或方向键选择 · Enter 确认",15,Muted,TextAnchor.MiddleCenter);
            string[] options={"继续游戏","重新开始挑战","返回自由练习","音效："+(world.Sound.Muted?"关":"开"),"退出游戏"};
            for(int i=0;i<options.Length;i++)
            {
                Rect rect=new Rect(592,329+i*58,416,47);
                if(i==world.MenuSelection)Box(rect,Art.Yellow);
                Text(rect,(i==world.MenuSelection?"›  ":"")+options[i],21,Art.Ink,TextAnchor.MiddleCenter,i==world.MenuSelection);
            }
            Text(new Rect(575,636,450,26),"Xbox：方向键选择 · A 确认 · Menu 返回",14,Muted,TextAnchor.MiddleCenter);
        }
        void Result()
        {
            Fill(new Rect(0,0,Width,Height),new Color(.10f,.22f,.19f,.48f));
            Box(new Rect(530,258,540,350),Paper);
            bool win=world.State==SessionState.Victory;
            Text(new Rect(550,284,500,37),win?"BANANA VICTORY!":"再来一根香蕉？",27,win?new Color(.15f,.53f,.39f):Art.Ink,TextAnchor.MiddleCenter,true);
            Text(new Rect(550,340,500,56),win?"花园守护成功":"这次先休息一下",36,Art.Ink,TextAnchor.MiddleCenter,true);
            Text(new Rect(550,415,500,32),"击败 "+world.Score+" 只团子   ·   用时 "+Mathf.FloorToInt(world.BattleSeconds)+" 秒",20,Muted,TextAnchor.MiddleCenter);
            Box(new Rect(606,485,388,54),Art.Yellow);
            Text(new Rect(606,485,388,54),"Enter / A  再战一次",22,Art.Ink,TextAnchor.MiddleCenter,true);
            Text(new Rect(550,555,500,26),"Esc / Menu 打开菜单，可返回练习",15,Muted,TextAnchor.MiddleCenter);
        }
        void OnDestroy(){if(rounded!=null)Destroy(rounded);if(font!=null)Destroy(font);}
    }
}
