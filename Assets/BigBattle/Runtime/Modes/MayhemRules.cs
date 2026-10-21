using System.Collections.Generic;
using UnityEngine;

namespace BigBattle
{
    public sealed partial class LaneMatch
    {
        public sealed class Relic {public Vector3 Position;public float ReadyAt,PulseAt;public Transform Visual;}
        sealed class Trip {public float At;public Vector3 Start,End;}
        public readonly List<Relic> Relics=new List<Relic>();
        public AugmentCard[] Offer {get;private set;}public int OfferSelection;public int OfferRerolls {get;private set;}
        public int HumanRounds=>World.Player.Augments!=null?World.Player.Augments.Chosen.Count:0;
        public readonly Transform[] Gates=new Transform[2];
        readonly Dictionary<BananaPlayer,Trip> trips=new Dictionary<BananaPlayer,Trip>();readonly Dictionary<BananaPlayer,float> gateReady=new Dictionary<BananaPlayer,float>();
        readonly Dictionary<Combatant,Renderer[]> visibility=new Dictionary<Combatant,Renderer[]>();
        readonly System.Random augRandom=new System.Random(91373);float visibilityAt;Mesh fog;Color[] fogColors;Vector3[] fogCenters;GameObject fogObject;
        public int DueRound=>Elapsed>=540?3:Elapsed>=360?2:Elapsed>=180?1:0;
        public Vector3 ClampSolid(Vector3 point)
        {
            point=FlowerLaneMap.Clamp(point);foreach(var structure in World.Actors)
            {
                if(structure==null || !structure.IsStructure || !structure.Alive || !structure.gameObject.activeInHierarchy)continue;
                Vector3 gap=point-structure.transform.position;gap.y=0;float radius=structure.Radius+.35f;if(gap.sqrMagnitude>=radius*radius)continue;
                point=structure.transform.position+(gap.sqrMagnitude<.0001f?FlowerLaneMap.Across:gap.normalized)*radius;
            }
            return FlowerLaneMap.Clamp(point);
        }
        public Vector3 Steer(Combatant actor,Vector3 goal)
        {
            Vector3 from=FlowerLaneMap.ToLane(actor.transform.position),to=FlowerLaneMap.ToLane(goal);float sign=Mathf.Sign(to.z-from.z);
            foreach(var building in World.Actors)
            {
                if(building==null || !building.IsStructure || building.Team!=actor.Team || !building.Alive || !building.gameObject.activeInHierarchy)continue;
                Vector3 p=FlowerLaneMap.ToLane(building.transform.position);float r=building.Radius+.7f;
                if((p.z-from.z)*sign>0 && Mathf.Abs(p.z-from.z)<3 && Mathf.Abs(p.x-from.x)<r)
                    return FlowerLaneMap.ToWorld(new Vector3(p.x+(from.x>=p.x?1:-1)*(r+.35f),0,p.z+sign*2));
            }
            return goal;
        }
        void SetupMayhem()
        {
            foreach(var hero in Heroes)MayhemAugments.Attach(hero,this);
            for(int i=0;i<4;i++)
            {
                Vector3 p=FlowerLaneMap.ToWorld(new Vector3(i<2?-4.5f:4.5f,0,new[]{-16f,-5f,5f,16f}[i]));var go=new GameObject("Health relic • delayed area heal");go.transform.SetParent(transform);go.transform.position=p;
                Art.Ring("Health relic rune",go.transform,.85f,.06f,new Color(.25f,.8f,.68f));var v=new GameObject("Floating health cross").transform;v.SetParent(go.transform,false);
                Art.Shape("Health cross upright",PrimitiveType.Cube,v,Vector3.up*.9f,new Vector3(.2f,.8f,.2f),new Color(.46f,1,.72f));Art.Shape("Health cross arms",PrimitiveType.Cube,v,Vector3.up*.9f,new Vector3(.68f,.2f,.2f),new Color(.75f,1,.88f));Relics.Add(new Relic{Position=p,ReadyAt=Time.time+40,Visual=v});
            }
            for(int team=0;team<2;team++)
            {
                var go=new GameObject("Base frostgate • interact");go.transform.SetParent(transform);go.transform.position=FlowerLaneMap.ToWorld(new Vector3(-5,0,team==0?-38:38));Gates[team]=go.transform;
                var ring=Art.Ring("Frostgate portal",go.transform,1.15f,.13f,team==0?new Color(.2f,.65f,1):Art.Coral);ring.transform.localRotation=Quaternion.Euler(90,0,0);ring.transform.localPosition=Vector3.up*1.2f;
                for(int s=-1;s<=1;s+=2)Art.Shape("Frostgate stone pillar",PrimitiveType.Cube,go.transform,new Vector3(s*1.2f,1.2f,0),new Vector3(.35f,2.6f,.5f),new Color(.32f,.43f,.54f));
            }
            BuildFog();TickOffers();
        }
        AugmentCard[] Draw(BananaPlayer hero,AugmentCard[] avoid=null)
        {
            var pool=new List<AugmentCard>();foreach(var card in MayhemAugments.Cards)
            {
                if(card.Hero>=0 && card.Hero!=hero.VisualStyle || hero.Augments.Chosen.Contains(card.Id))continue;
                bool old=false;if(avoid!=null)foreach(var item in avoid)if(item.Id==card.Id)old=true;if(!old)pool.Add(card);
            }
            var result=new AugmentCard[3];for(int i=0;i<3;i++){int index=augRandom.Next(pool.Count);result[i]=pool[index];pool.RemoveAt(index);}return result;
        }
        void TickOffers()
        {
            foreach(var hero in Heroes)
            {
                if(hero.Augments==null || hero.Augments.Chosen.Count>DueRound)continue;
                if(hero==World.Player && !AutomaticChoices)
                {if(Offer==null){Offer=Draw(hero);OfferSelection=0;OfferRerolls=1;World.SetPaused(true);}continue;}
                var cards=Draw(hero);hero.Augments.Apply(cards[augRandom.Next(3)].Id);
            }
        }
        public bool RerollOffer(){if(Offer==null || OfferRerolls==0)return false;Offer=Draw(World.Player,Offer);OfferSelection=0;OfferRerolls--;return true;}
        public bool ConfirmOffer()
        {if(Offer==null || !World.Player.Augments.Apply(Offer[Mathf.Clamp(OfferSelection,0,2)].Id))return false;Offer=null;World.SetPaused(false);return true;}
        public bool Transiting(BananaPlayer hero)=>trips.ContainsKey(hero);
        public bool TryGate(BananaPlayer hero)
        {
            float ready;gateReady.TryGetValue(hero,out ready);if(Finished || World.Paused || hero==null || !hero.Alive || hero.Controlled || Time.time<ready || trips.ContainsKey(hero) || Vector3.Distance(hero.transform.position,Gates[hero.Team].position)>2)return false;
            float z=(hero.Team==0?-1:1)*(Towers[hero.Team,0]!=null && Towers[hero.Team,0].Alive?9:18);
            trips.Add(hero,new Trip{At=Time.time+1,Start=hero.transform.position,End=FlowerLaneMap.ToWorld(new Vector3(-2,0,z))});World.Melee.Cancel(hero);gateReady[hero]=Time.time+8;hero.Reveal();World.Fx.Circle(hero.transform.position,1.2f,new Color(.2f,.6f,1),.6f);return true;
        }
        public Vector3? RelicGoal(BananaPlayer hero)
        {Relic best=null;float distance=18;foreach(var r in Relics)if(Time.time>=r.ReadyAt && r.PulseAt==0){float d=Vector3.Distance(r.Position,hero.transform.position);if(d<distance){distance=d;best=r;}}return best!=null?(Vector3?)best.Position:null;}
        public bool ProvokedTower(int team,Combatant actor)=>actor!=null && actor is BananaPlayer && actor.Team!=team && Time.time<actor.TowerAggroUntil;
        public static int Brush(Vector3 world)
        {
            Vector3 p=FlowerLaneMap.ToLane(world);float[] z={-17,-6,6,17};for(int i=0;i<4;i++)if(Mathf.Abs(p.z-z[i])<2 && (i<2?p.x<-3.8f:p.x>3.8f))return i;return -1;
        }
        bool PointVisible(int team,Vector3 point,int brush,bool revealed=false)
        {
            foreach(var actor in World.Actors)
            {
                if(actor==null || actor.Team!=team || !actor.Alive || !actor.gameObject.activeInHierarchy || !(actor is BananaPlayer || actor is LaneMinion || actor.IsStructure))continue;
                float range=actor is BananaPlayer?14:actor.IsStructure?12:8;if((actor.transform.position-point).sqrMagnitude>range*range)continue;
                if(brush<0 || revealed || Brush(actor.transform.position)==brush)return true;
            }
            return false;
        }
        public bool VisibleTo(int team,Combatant actor)=>actor!=null && (actor.Team==team || actor.IsStructure || PointVisible(team,actor.transform.position,Brush(actor.transform.position),Time.time<actor.RevealedUntil));
        void BuildFog()
        {
            var v=new List<Vector3>();var t=new List<int>();var centers=new List<Vector3>();
            for(int z=-44;z<44;z+=4)for(int x=-6;x<6;x+=2)
            {int i=v.Count;foreach(var p in new[]{new Vector3(x,.10f,z),new Vector3(x+2,.10f,z),new Vector3(x+2,.10f,z+4),new Vector3(x,.10f,z+4)})v.Add(FlowerLaneMap.ToWorld(p));t.AddRange(new[]{i,i+2,i+1,i,i+3,i+2});centers.Add(FlowerLaneMap.ToWorld(new Vector3(x+1,0,z+2)));}
            fog=new Mesh{name="Session shared-vision fog"};fog.SetVertices(v);fog.SetTriangles(t,0);fogColors=new Color[v.Count];fogCenters=centers.ToArray();fog.colors=fogColors;fog.RecalculateBounds();fogObject=new GameObject("Shared team fog of war");fogObject.transform.SetParent(transform,false);fogObject.AddComponent<MeshFilter>().sharedMesh=fog;var renderer=fogObject.AddComponent<MeshRenderer>();renderer.sharedMaterial=Art.FxMat("Abyss fog",Color.white);renderer.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;
        }
        void TickMayhem()
        {
            TickOffers();if(World.Paused)return;
            foreach(var entry in new Dictionary<BananaPlayer,Trip>(trips))
            {
                var hero=entry.Key;var trip=entry.Value;
                if(hero==null || !hero.Alive || hero.Controlled || Vector3.Distance(hero.transform.position,trip.Start)>.5f || hero.LastHitAt>trip.At-1){trips.Remove(hero);continue;}
                if(Time.time<trip.At)continue;trips.Remove(hero);hero.transform.position=World.Clamp(trip.End);if(hero==World.Player)World.CameraRig.Snap();World.Fx.Circle(hero.transform.position,1.2f,new Color(.2f,.6f,1),.4f);
            }
            foreach(var relic in Relics)
            {
                relic.Visual.gameObject.SetActive(Time.time>=relic.ReadyAt && relic.PulseAt==0);relic.Visual.localPosition=Vector3.up*Mathf.Sin(Time.time*3)*.1f;relic.Visual.localRotation=Quaternion.Euler(0,Time.time*35,0);
                if(relic.PulseAt>0 && Time.time>=relic.PulseAt)
                {relic.PulseAt=0;foreach(var hero in Heroes)if(hero.Alive && Vector3.Distance(hero.transform.position,relic.Position)<3)hero.Heal(20+(hero.MaxHealth-hero.Health)*.10f);World.Fx.Circle(relic.Position,3,Art.Mint,.5f);}
                if(Time.time<relic.ReadyAt || relic.PulseAt>0)continue;
                foreach(var hero in Heroes)if(hero.Alive && hero.Health<hero.MaxHealth-.1f && Vector3.Distance(hero.transform.position,relic.Position)<.85f)
                {hero.Heal(10+(hero.MaxHealth-hero.Health)*.08f);relic.ReadyAt=Time.time+40;relic.PulseAt=Time.time+2.5f;World.Fx.Circle(relic.Position,3,Art.Mint,2.5f);break;}
            }
            if(Time.time<visibilityAt)return;visibilityAt=Time.time+.15f;
            foreach(var actor in World.Actors)
            {
                if(actor==null)continue;Renderer[] renderers;if(!visibility.TryGetValue(actor,out renderers)){renderers=actor.GetComponentsInChildren<Renderer>(true);visibility[actor]=renderers;}
                bool show=World.CameraRig.MapOverview || VisibleTo(World.Player.Team,actor);foreach(var renderer in renderers)if(renderer!=null)renderer.enabled=show;
            }
            foreach(var actor in new List<Combatant>(visibility.Keys))if(actor==null)visibility.Remove(actor);
            fogObject.SetActive(!World.CameraRig.MapOverview);
            for(int i=0;i<fogCenters.Length;i++){Color color=PointVisible(World.Player.Team,fogCenters[i],-1)?Color.clear:new Color(.035f,.075f,.13f,.5f);for(int k=0;k<4;k++)fogColors[i*4+k]=color;}fog.colors=fogColors;
        }
        void ShutdownMayhem()
        {Offer=null;trips.Clear();gateReady.Clear();foreach(var pair in visibility)foreach(var r in pair.Value)if(r!=null)r.enabled=true;visibility.Clear();}
        void OnDestroy(){if(fog!=null)Destroy(fog);}
    }
}
