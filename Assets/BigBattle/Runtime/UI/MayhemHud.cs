using UnityEngine;

namespace BigBattle
{
    public sealed partial class GameHud
    {
        void MayhemPanel()
        {
            Fill(new Rect(0,0,1600,900),new Color(.025f,.055f,.10f,.87f));
            Text(new Rect(300,105,1000,65),world.MayhemLobby?"命运已选定你的英雄":"选择你的海克斯强化",38,new Color(.85f,.94f,1),TextAnchor.MiddleCenter,true);
            Text(new Rect(300,172,1000,36),world.MayhemLobby?"一名真人 + 九名 AI · 随机角色 · 冰雪单路推基地":"三选一 · 强化只在本局生效 · 选择期间已暂停",19,new Color(.56f,.74f,.86f),TextAnchor.MiddleCenter);
            if(world.MayhemLobby)
            {
                string name=world.DraftHero==HeroKind.Banana?"香蕉刺客":world.DraftHero==HeroKind.Yueyue?"越越大王":"阿狸";
                Box(new Rect(510,280,580,300),new Color(.12f,.21f,.31f));Text(new Rect(530,310,540,70),name,44,Art.Cream,TextAnchor.MiddleCenter,true);
                string description=world.DraftHero==HeroKind.Banana?"双刃近战 · 三段连招 · 拼刀":world.DraftHero==HeroKind.Yueyue?"追魂骷髅 · 铁索禁锢 · 万魂魔剑":"原地爆泡 · 十字扩散 · 触泡连锁";
                Text(new Rect(540,405,520,40),description,22,new Color(.75f,.87f,.95f),TextAnchor.MiddleCenter);
                Text(new Rect(540,485,520,35),"重随机剩余  "+world.HeroRerolls+" / 2",22,Art.Yellow,TextAnchor.MiddleCenter,true);
            }
            else
            {
                for(int i=0;i<3;i++)
                {
                    var card=world.Lane.Offer[i];float x=285+i*350;bool selected=i==world.Lane.OfferSelection;Color tier=card.Tier==0?new Color(.68f,.8f,.9f):card.Tier==1?Art.Yellow:new Color(.65f,.5f,1);
                    Box(new Rect(x-4,278,338,315),selected?tier:new Color(.18f,.27f,.36f));Box(new Rect(x,282,330,307),new Color(.10f,.18f,.27f));
                    Text(new Rect(x+20,300,290,30),(card.Tier==0?"白银":card.Tier==1?"黄金":"棱彩")+(card.Hero>=0?" · 角色专属":" · 通用强化"),17,tier,TextAnchor.MiddleCenter);
                    Text(new Rect(x+15,355,300,50),card.Name,29,new Color(.94f,.97f,1),TextAnchor.MiddleCenter,true);
                    label.wordWrap=true;Text(new Rect(x+30,424,270,105),card.Description,19,new Color(.73f,.84f,.92f),TextAnchor.MiddleCenter);label.wordWrap=false;
                    Text(new Rect(x+15,540,300,30),selected?"›  已选中 · 确认获得":"切换查看",18,selected?tier:Muted,TextAnchor.MiddleCenter,true);
                }
                Text(new Rect(400,625,800,30),"第 "+(world.Lane.HumanRounds+1)+" / 4 轮 · 本轮刷新剩余 "+world.Lane.OfferRerolls,20,Art.Yellow,TextAnchor.MiddleCenter);
            }
            Text(new Rect(250,690,1100,40),world.Input.DeviceLabel=="Xbox"?"方向键 / 左摇杆选择 · A 确认 · Y 重随机 / 刷新":"W/S / 方向键选择 · Enter 确认 · R 重随机 / 刷新",21,Color.white,TextAnchor.MiddleCenter,true);
            Text(new Rect(350,755,900,32),"Esc / Menu 取消并返回练习 · 本模式无金币、等级、装备及超级兵",16,new Color(.57f,.70f,.8f),TextAnchor.MiddleCenter);
        }
        void AugmentStrip()
        {
            if(!world.IsLane || world.Player.Augments==null)return;
            Box(new Rect(345,150,390,83),new Color(.08f,.16f,.25f,.9f));string names="";
            foreach(int id in world.Player.Augments.Chosen)names+=(names.Length>0?" · ":"")+MayhemAugments.Cards[id].Name;
            float until=Mathf.Max(0,world.Lane.HumanRounds*180-world.Lane.Elapsed);
            Text(new Rect(358,156,364,22),"本局强化  "+world.Lane.HumanRounds+" / 4"+(world.Lane.HumanRounds<4?" · 下轮 "+Mathf.CeilToInt(until)+" 秒":" · 已全部获得"),15,new Color(.85f,.93f,1),TextAnchor.MiddleLeft,true);
            label.wordWrap=true;Text(new Rect(358,182,364,40),names,13,new Color(.64f,.79f,.9f));label.wordWrap=false;
        }
    }
}
