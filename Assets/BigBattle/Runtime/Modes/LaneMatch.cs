using System.Collections.Generic;
using UnityEngine;

namespace BigBattle
{
    public sealed partial class LaneMatch : MonoBehaviour
    {
        public const int TeamSize=5;public const float RespawnDelay=8,WaveInterval=25,InhibitorRespawn=300;
        public readonly List<BananaPlayer> Heroes=new List<BananaPlayer>();
        public readonly LaneStructure[,] Towers=new LaneStructure[2,2],NexusTowers=new LaneStructure[2,2];
        public readonly LaneStructure[] Cores=new LaneStructure[2],Inhibitors=new LaneStructure[2];
        public readonly int[] HeroKills=new int[2];
        public bool Finished {get;private set;}public int WavesSpawned {get;private set;}public bool Autonomous=true;
        public bool AutomaticChoices;public float Elapsed=>Mathf.Max(0,Time.time-started);public float NextWaveIn=>Mathf.Max(0,nextWave-Time.time);
        public float CurrentRespawnDelay=>Mathf.Min(32,RespawnDelay+Mathf.Floor(Elapsed/60)*2);
        public GameRoot World {get;private set;}
        readonly Dictionary<BananaPlayer,float> respawns=new Dictionary<BananaPlayer,float>();readonly float[] inhibitorAt=new float[2];
        float started,nextWave;
        public void Setup(GameRoot world,bool automatic=false)
        {
            World=world;AutomaticChoices=automatic;started=Time.time;Heroes.Add(world.Player);world.Player.RespawnAt(FlowerLaneMap.Spawn(0)-FlowerLaneMap.Across*2.2f,FlowerLaneMap.Forward(0));
            for(int team=0;team<2;team++)
            {
                int s=team==0?-1:1;
                for(int i=0;i<2;i++){Towers[team,i]=Structure(team,false,i,new Vector3(0,0,s*(i==0?11:20)));NexusTowers[team,i]=Structure(team,false,3+i,new Vector3(i==0?-2.2f:2.2f,0,s*30));}
                Inhibitors[team]=Structure(team,false,2,new Vector3(0,0,s*25));Cores[team]=Structure(team,true,5,new Vector3(0,0,s*34));
                for(int i=team==0?1:0;i<TeamSize;i++)SpawnHero(team,GameRoot.BattleHeroes[UnityEngine.Random.Range(0,3)],i);
            }
            nextWave=Time.time+15;SetupMayhem();
        }
        LaneStructure Structure(int team,bool core,int index,Vector3 point)
        {
            var go=new GameObject(core?"Nexus crystal":index==2?"Respawning inhibitor":"Frozen guardian turret");go.transform.SetParent(transform);go.transform.position=FlowerLaneMap.ToWorld(point);var actor=go.AddComponent<LaneStructure>();actor.Setup(World,this,team,core,index);return actor;
        }
        void SpawnHero(int team,HeroKind kind,int slot)
        {
            var go=new GameObject("Mayhem random hero • "+kind+" "+team+":"+slot);go.transform.SetParent(transform);
            BananaPlayer hero=kind==HeroKind.Ali?(BananaPlayer)go.AddComponent<AliPlayer>():kind==HeroKind.Banana?go.AddComponent<BananaPlayer>():go.AddComponent<YueyuePlayer>();hero.SetupTeam(World,team);
            hero.RespawnAt(FlowerLaneMap.Spawn(team)+FlowerLaneMap.Across*((slot-2)*1.1f),FlowerLaneMap.Forward(team));Heroes.Add(hero);go.AddComponent<LaneHeroBot>().Setup(this,hero,(slot-2)*.8f);
        }
        void Update()
        {
            if(World.Paused || Finished || World.State!=SessionState.Battle)return;
            foreach(var entry in new Dictionary<BananaPlayer,float>(respawns))
            {
                if(entry.Key==null){respawns.Remove(entry.Key);continue;}if(Time.time<entry.Value)continue;
                var hero=entry.Key;respawns.Remove(hero);hero.gameObject.SetActive(true);hero.RespawnAt(FlowerLaneMap.Spawn(hero.Team)+FlowerLaneMap.Across*((Heroes.IndexOf(hero)%TeamSize-2)*1.1f),FlowerLaneMap.Forward(hero.Team));
                if(hero.Augments!=null)hero.Augments.OnRespawn();if(hero==World.Player){World.CameraRig.Snap();World.Fx.Notice("已复活 · 基地传送门可快速返回战线");}
            }
            for(int team=0;team<2;team++)if(inhibitorAt[team]>0 && Time.time>=inhibitorAt[team])
            {inhibitorAt[team]=0;Inhibitors[team].Restore();Inhibitors[team].gameObject.SetActive(true);World.Fx.Notice("抑制水晶已经重生");}
            // Howling Abyss has no fountain healing / recall. Relics and augments heal.
            foreach(var hero in Heroes)if(hero!=null && hero.Alive && FlowerLaneMap.ToLane(hero.transform.position).z*(hero.Team==0?-1:1)<-39.5f)
            {var source=Cores[1-hero.Team];if(source!=null)hero.Damage(2000*Time.deltaTime,source,Vector3.zero,0);}
            TickMayhem();if(World.Paused)return;
            if(Autonomous && Time.time>=nextWave){SpawnMinionWave();nextWave=Time.time+WaveInterval;}
        }
        public void SpawnMinionWave()
        {
            WavesSpawned++;
            for(int team=0;team<2;team++)
            {
                int living=0;foreach(var actor in World.Actors)if(actor is LaneMinion && actor.Alive && actor.Team==team)living++;
                int count=WavesSpawned%3==0?7:6;
                for(int i=0;i<count && living+i<30;i++)
                {
                    var go=new GameObject(i==6?"Siege cannon minion":i<3?"Melee minion":"Caster minion");go.transform.SetParent(transform);go.transform.position=FlowerLaneMap.ToWorld(new Vector3((i%3-1)*.9f,0,(team==0?-1:1)*(36+i/3*1.2f)));
                    go.AddComponent<LaneMinion>().Setup(World,this,team,(i%3-1)*.9f,i==6?2:i<3?0:1);
                }
            }
        }
        public Combatant Goal(int team)
        {
            int enemy=1-team;for(int i=0;i<2;i++)if(Towers[enemy,i]!=null && Towers[enemy,i].Alive)return Towers[enemy,i];
            if(Inhibitors[enemy].Alive)return Inhibitors[enemy];for(int i=0;i<2;i++)if(NexusTowers[enemy,i]!=null && NexusTowers[enemy,i].Alive)return NexusTowers[enemy,i];return Cores[enemy];
        }
        public Combatant PickFight(Combatant source,float range)
        {
            Combatant best=null;float score=range;foreach(var actor in World.Actors)
            {if(actor==null || actor.Team==source.Team || !World.Targetable(actor,source))continue;float distance=Vector3.Distance(actor.transform.position,source.transform.position);float weighted=distance+(actor.IsStructure?1:actor is LaneMinion?1.5f:0);if(distance<=range && weighted<score){best=actor;score=weighted;}}return best;
        }
        public bool MinionSupport(int team,Vector3 point)
        {foreach(var actor in World.Actors)if(actor is LaneMinion && actor.Team==team && actor.Alive && Vector3.Distance(actor.transform.position,point)<8)return true;return false;}
        public void HeroDown(BananaPlayer hero)
        {
            if(Finished || respawns.ContainsKey(hero))return;World.Melee.Cancel(hero);var mage=hero as YueyuePlayer;if(mage!=null)mage.InterruptCharge();
            World.Fx.Ghost(hero.Visual,true);World.Sound.Play(SoundCue.Pop);HeroKills[1-hero.Team]++;if(hero.Team==1)World.Score++;
            float delay=CurrentRespawnDelay;respawns.Add(hero,Time.time+delay);hero.gameObject.SetActive(false);if(hero==World.Player){World.Input.StopRumble();World.Fx.Notice("暂时休息 · "+delay.ToString("0")+" 秒后在冰桥基地重生");}
        }
        public float RespawnIn(BananaPlayer hero){float at;return respawns.TryGetValue(hero,out at)?Mathf.Max(0,at-Time.time):0;}
        public float InhibitorIn(int team)=>Mathf.Max(0,inhibitorAt[team]-Time.time);
        public void StructureDown(LaneStructure structure)
        {
            World.Fx.Burst(structure.transform.position+Vector3.up,structure.Team==0?new Color(.2f,.7f,1):Art.Coral,24);World.Sound.Play(SoundCue.Win,.7f);
            if(structure.Core)World.Finish(structure.Team==1);else if(structure.Inhibitor){inhibitorAt[structure.Team]=Time.time+InhibitorRespawn;World.Fx.Notice("抑制水晶摧毁 · 可攻击基地双塔（本模式无超级兵）");}
            else World.Fx.Notice((structure.Team==0?"我方":"敌方")+"防御塔已摧毁");
        }
        public void StopCombat(){Finished=true;}
        public void Shutdown(){Finished=true;respawns.Clear();foreach(var hero in Heroes)if(hero!=null && hero.Augments!=null)hero.Augments.Clear();ShutdownMayhem();gameObject.SetActive(false);}
    }
}
