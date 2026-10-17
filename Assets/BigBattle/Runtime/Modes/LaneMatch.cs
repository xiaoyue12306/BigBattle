using System.Collections.Generic;
using UnityEngine;

namespace BigBattle
{
    public sealed class LaneMatch : MonoBehaviour
    {
        public const int TeamSize=5;
        public const float RespawnDelay=8,WaveInterval=12;
        public readonly List<BananaPlayer> Heroes=new List<BananaPlayer>();
        public readonly LaneStructure[,] Towers=new LaneStructure[2,2];
        public readonly LaneStructure[] Cores=new LaneStructure[2];
        public readonly int[] HeroKills=new int[2];
        public bool Finished {get;private set;}
        public int WavesSpawned {get;private set;}
        public bool Autonomous=true;
        public float NextWaveIn=>Mathf.Max(0,nextWave-Time.time);
        public GameRoot World {get;private set;}
        readonly Dictionary<BananaPlayer,float> respawns=new Dictionary<BananaPlayer,float>();
        float nextWave;
        public void Setup(GameRoot world)
        {
            World=world;Heroes.Add(world.Player);world.Player.RespawnAt(FlowerLaneMap.Spawn(0)+Vector3.left*2.2f,Vector3.forward);
            for(int team=0;team<2;team++)
            {
                for(int i=0;i<2;i++)Towers[team,i]=Structure(team,false,i,new Vector3(0,0,(team==0?-1:1)*(i==0?12:22)));
                Cores[team]=Structure(team,true,2,new Vector3(0,0,team==0?-31:31));
                for(int i=team==0?1:0;i<TeamSize;i++)SpawnHero(team,(i+team)%2==0?HeroKind.Banana:HeroKind.Yueyue,i);
            }
            nextWave=Time.time+2;
        }
        LaneStructure Structure(int team,bool core,int index,Vector3 point)
        {
            var go=new GameObject(core?"Flower heart core":"Sprout guardian tower");go.transform.SetParent(transform);go.transform.position=point;
            var actor=go.AddComponent<LaneStructure>();actor.Setup(World,this,team,core,index);return actor;
        }
        void SpawnHero(int team,HeroKind kind,int slot)
        {
            var go=new GameObject((team==0?"Meadow ally • ":"Peach rival • ")+(kind==HeroKind.Banana?"Banana":"Yueyue")+" "+slot);go.transform.SetParent(transform);
            BananaPlayer hero=kind==HeroKind.Banana?go.AddComponent<BananaPlayer>():go.AddComponent<YueyuePlayer>();hero.SetupTeam(World,team);
            hero.RespawnAt(FlowerLaneMap.Spawn(team)+Vector3.right*((slot-2)*1.1f),team==0?Vector3.forward:Vector3.back);
            Heroes.Add(hero);go.AddComponent<LaneHeroBot>().Setup(this,hero,(slot-2)*.8f);
        }
        void Update()
        {
            if(World.Paused || Finished || World.State!=SessionState.Battle)return;
            foreach(var entry in new Dictionary<BananaPlayer,float>(respawns))
            {
                if(entry.Key==null){respawns.Remove(entry.Key);continue;}
                if(Time.time<entry.Value)continue;
                var hero=entry.Key;respawns.Remove(hero);hero.gameObject.SetActive(true);hero.RespawnAt(FlowerLaneMap.Spawn(hero.Team)+Vector3.right*((Heroes.IndexOf(hero)%TeamSize-2)*1.1f),hero.Team==0?Vector3.forward:Vector3.back);
                if(hero==World.Player){World.CameraRig.Snap();World.Fx.Notice("重新出发 · 与花灵一起推进");}
            }
            foreach(var hero in Heroes)
                if(hero!=null && hero.Alive && Vector3.Distance(hero.transform.position,FlowerLaneMap.Spawn(hero.Team))<4.5f)hero.Heal(12*Time.deltaTime);
            if(Autonomous && Time.time>=nextWave){SpawnMinionWave();nextWave=Time.time+WaveInterval;}
        }
        public void SpawnMinionWave()
        {
            WavesSpawned++;
            for(int team=0;team<2;team++)
            {
                int living=0;foreach(var actor in World.Actors)if(actor is LaneMinion && actor.Alive && actor.Team==team)living++;
                for(int i=0;i<3 && living+i<18;i++)
                {
                    var go=new GameObject(team==0?"Mint flower sprout":"Peach flower sprout");go.transform.SetParent(transform);go.transform.position=new Vector3((i-1)*.9f,0,team==0?-27:27);
                    go.AddComponent<LaneMinion>().Setup(World,this,team,(i-1)*.9f);
                }
            }
        }
        public Combatant Goal(int team)
        {
            int enemy=1-team;
            for(int i=0;i<2;i++)if(Towers[enemy,i]!=null && Towers[enemy,i].Alive)return Towers[enemy,i];
            return Cores[enemy];
        }
        public Combatant PickFight(Combatant source,float range)
        {
            Combatant best=null;float score=range;
            foreach(var actor in World.Actors)
            {
                if(actor==null || actor.Team==source.Team || !World.Targetable(actor))continue;
                float distance=Vector3.Distance(actor.transform.position,source.transform.position);
                float weighted=distance+(actor.IsStructure?1:actor is LaneMinion?1.5f:0);
                if(distance<=range && weighted<score){best=actor;score=weighted;}
            }
            return best;
        }
        public bool MinionSupport(int team,Vector3 point)
        {
            foreach(var actor in World.Actors)if(actor is LaneMinion && actor.Team==team && actor.Alive && Vector3.Distance(actor.transform.position,point)<8)return true;
            return false;
        }
        public void HeroDown(BananaPlayer hero)
        {
            if(Finished || respawns.ContainsKey(hero))return;
            World.Melee.Cancel(hero);var mage=hero as YueyuePlayer;if(mage!=null)mage.InterruptCharge();
            World.Fx.Ghost(hero.Visual,true);World.Sound.Play(SoundCue.Pop);
            HeroKills[1-hero.Team]++;if(hero.Team==1)World.Score++;
            respawns.Add(hero,Time.time+RespawnDelay);hero.gameObject.SetActive(false);
            if(hero==World.Player){World.Input.StopRumble();World.Fx.Notice("暂时休息 · 8 秒后在花心基地重生");}
        }
        public float RespawnIn(BananaPlayer hero){float at;return respawns.TryGetValue(hero,out at)?Mathf.Max(0,at-Time.time):0;}
        public void StructureDown(LaneStructure structure)
        {
            World.Fx.Burst(structure.transform.position+Vector3.up,structure.Team==0?Art.Mint:Art.Coral,24);World.Sound.Play(SoundCue.Win,.7f);
            if(structure.Core)World.Finish(structure.Team==1);
            else World.Fx.Notice((structure.Team==0?"我方":"敌方")+"花灵守卫已退场 · 继续守护花心！");
        }
        public void StopCombat(){Finished=true;}
        public void Shutdown(){Finished=true;respawns.Clear();gameObject.SetActive(false);}
    }
}
