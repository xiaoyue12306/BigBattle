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
        public Vector3 Clamp(Vector3 point)=>IsLane?FlowerLaneMap.Clamp(point):GardenArena.Clamp(point);
        public bool Targetable(Combatant actor)=>actor!=null && actor.Alive && actor.gameObject.activeInHierarchy && (!(actor is LaneStructure) || !((LaneStructure)actor).Protected);
        public void StartLaneBattle()
        {
            PreferredMode=MatchMode.FlowerLane;ClearSession();ActiveMode=MatchMode.FlowerLane;State=SessionState.Battle;
            if(laneMap==null){laneMap=new GameObject("Flower creek • original single-lane map");laneMap.transform.SetParent(transform);laneMap.AddComponent<FlowerLaneMap>().Build();}
            gardenMap.SetActive(false);laneMap.SetActive(true);
            var go=new GameObject("Flower creek match");go.transform.SetParent(Transients);Lane=go.AddComponent<LaneMatch>();Lane.Setup(this);CameraRig.Snap();
            Fx.Notice("花溪大乱斗 · 跟随花灵推进 · 摧毁敌方花心核心获胜");
        }
        void ClearLane()
        {
            if(Lane!=null)Lane.Shutdown();Lane=null;ActiveMode=MatchMode.Garden;
            if(CameraRig!=null)CameraRig.MapOverview=false;
            if(laneMap!=null)laneMap.SetActive(false);if(gardenMap!=null)gardenMap.SetActive(true);
        }
    }
}
