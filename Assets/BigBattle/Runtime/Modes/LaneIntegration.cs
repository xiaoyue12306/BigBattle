using UnityEngine;

namespace BigBattle
{
    public enum MatchMode { Garden,FlowerLane }
    public sealed partial class GameRoot
    {
        public MatchMode PreferredMode {get;set;}
        public MatchMode ActiveMode {get;private set;}
        public LaneMatch Lane {get;private set;}
        public bool IsLane=>ActiveMode==MatchMode.FlowerLane && Lane!=null;
        GameObject gardenMap,laneMap;
        public Vector3 Clamp(Vector3 point)=>IsLane?Lane.ClampSolid(point):GardenArena.Clamp(point);
        public static readonly HeroKind[] BattleHeroes={HeroKind.Banana,HeroKind.Yueyue,HeroKind.Ali};
        public bool MayhemLobby {get;private set;}public HeroKind DraftHero {get;private set;}public int HeroRerolls {get;private set;}
        public bool ChoosingAugment=>IsLane && Lane.Offer!=null;
        public bool Damageable(Combatant actor)=>actor!=null && actor.Alive && actor.gameObject.activeInHierarchy && (!(actor is LaneStructure) || !((LaneStructure)actor).Protected);
        public bool Targetable(Combatant actor,Combatant observer=null)=>Damageable(actor) && (!IsLane || Lane.VisibleTo(observer!=null?observer.Team:Player.Team,actor));
        public void StartLaneBattle()
        {
            if(PreviewOnly){Fx.Notice("女巫当前为外观预览 · 选择其他英雄进入海克斯大乱斗");return;}
            EnterPractice();DraftHero=BattleHeroes[Random.Range(0,3)];HeroRerolls=2;MayhemLobby=true;SetPaused(true);
        }
        public bool RerollHero()
        {if(!MayhemLobby || HeroRerolls<=0)return false;int i=System.Array.IndexOf(BattleHeroes,DraftHero);DraftHero=BattleHeroes[(i+Random.Range(1,3))%3];HeroRerolls--;return true;}
        public void ConfirmMayhem(){if(MayhemLobby)BeginMayhem(DraftHero);}
        public void BeginMayhem(HeroKind kind,bool automaticChoices=false)
        {
            if(kind==HeroKind.Witch || PreviewOnly){Fx.Notice("女巫技能待设计 · 暂不能参加海克斯大乱斗");return;}
            if(SelectedHero!=kind)SelectHero(kind);
            PreferredMode=MatchMode.FlowerLane;ClearSession();ActiveMode=MatchMode.FlowerLane;State=SessionState.Battle;
            if(laneMap==null){laneMap=new GameObject("Howling ice bridge • original geometry");laneMap.transform.SetParent(transform);laneMap.AddComponent<FlowerLaneMap>().Build();}
            gardenMap.SetActive(false);laneMap.SetActive(true);
            var go=new GameObject("Hex mayhem match");go.transform.SetParent(Transients);Lane=go.AddComponent<LaneMatch>();Lane.Setup(this,automaticChoices);CameraRig.Snap();
            Fx.Notice("海克斯大乱斗 · 选择强化 · 推进冰桥摧毁敌方基地");
        }
        void ClearLane()
        {
            MayhemLobby=false;if(Lane!=null)Lane.Shutdown();Lane=null;ActiveMode=MatchMode.Garden;
            if(CameraRig!=null)CameraRig.MapOverview=false;
            if(laneMap!=null)laneMap.SetActive(false);if(gardenMap!=null)gardenMap.SetActive(true);
        }
    }
}
