using System.Collections;
using UnityEngine;

namespace BigBattle
{
    public sealed partial class SmokeTest
    {
        IEnumerator LaneChecks()
        {
            Frame();world.StartLaneBattle();yield return Wait(.15f);var lane=world.Lane;lane.Autonomous=false;
            Check(world.IsLane && world.State==SessionState.Battle && world.Wave==0,"Flower lane starts a separate objective match without arena waves");
            int friendly=0,enemy=0;foreach(var hero in lane.Heroes)if(hero.Team==0)friendly++;else enemy++;
            Check(friendly==5 && enemy==5 && lane.Heroes.Count==10,"Single human plus nine autonomous heroes form five versus five teams");
            Check(lane.Towers[0,0]!=null && lane.Towers[1,1]!=null && lane.Cores[0]!=null && lane.Cores[1]!=null,"Each side owns two guardian towers and one flower core");
            Check(world.Player.transform.position.z<-25 && Mathf.Abs(world.Player.transform.position.z)>GardenArena.WalkRadius,"Lane hero spawns beyond the old circular arena boundary");
            Vector3 boundary=FlowerLaneMap.Clamp(new Vector3(99,10,99));Check(boundary.x==9 && boundary.z==34 && boundary.y==0,"Lane boundary clamps to its own expanded courtyard and length");
            Check(FlowerLaneMap.Clamp(new Vector3(99,0,0)).x==6,"Central bridge keeps actors inside the walkable strip");
            world.CameraRig.MapOverview=true;world.CameraRig.Snap();yield return Wait(.2f);yield return Shot("21-flower-lane-overview");world.CameraRig.MapOverview=false;world.CameraRig.Snap();
            Vector3 start=world.Player.transform.position;Frame(new CombatInput{Move=Vector2.up});yield return Wait(.3f);Frame();
            Check(world.Player.transform.position.z>start.z+1 && world.Player.transform.position.z<-20,"Player movement uses the long lane rather than circular clamping");
            Vector3 p=world.CameraRig.Camera.WorldToViewportPoint(world.Player.transform.position);Check(p.x>.1f && p.x<.9f && p.y>.1f && p.y<.9f,"Following camera keeps a lane player on screen near base");
            lane.SpawnMinionWave();int count0=0,count1=0;LaneMinion sprout0=null,sprout1=null;
            foreach(var actor in world.Actors)if(actor is LaneMinion){if(actor.Team==0){count0++;sprout0=(LaneMinion)actor;}else{count1++;sprout1=(LaneMinion)actor;}}
            Check(count0==3 && count1==3,"A flower wave creates three allied and three enemy sprouts");
            float sproutZ=sprout0.transform.position.z;var friendlyBot=lane.Heroes[1];float botZ=friendlyBot.transform.position.z;
            lane.Autonomous=true;yield return Wait(.65f);lane.Autonomous=false;
            Check(sprout0.transform.position.z>sproutZ+1.3f,"Friendly sprouts autonomously march toward the enemy objective");
            Check(friendlyBot.transform.position.z>botZ+2,"Hero AI advances along the lane toward the battle");
            float waveWait=lane.NextWaveIn;world.SetPaused(true);Vector3 paused=sprout0.transform.position;yield return Wait(.12f);
            Check(sprout0.transform.position==paused && Mathf.Abs(lane.NextWaveIn-waveWait)<.04f,"Pause freezes minion movement and reinforcement timer");world.SetPaused(false);
            var outer=lane.Towers[1,0];var inner=lane.Towers[1,1];var core=lane.Cores[1];float health=core.Health;
            Check(!inner.Damage(100,world.Player,Vector3.zero) && !core.Damage(100,world.Player,Vector3.zero) && core.Health==health,"Inner guardian and core reject damage while outer objectives stand");
            health=outer.Health;outer.Damage(100,world.Player,Vector3.zero,0);
            Check(outer.Health==health-20,"Unsupported structure attacks receive eighty-percent backdoor reduction");
            sprout0.transform.position=outer.transform.position-Vector3.forward*2;health=outer.Health;outer.Damage(100,world.Player,Vector3.zero,0);
            Check(outer.Health==health-100,"Nearby allied sprouts remove backdoor reduction");
            Check(!outer.Bind(2) && !outer.Controlled,"Guardian structures cannot be chained or stunned");
            // Real guardian projectile: the flower sprout is preferred over the
            // adjacent hero, rather than directly invoking the damage method.
            foreach(var hero in lane.Heroes){var bot=hero.GetComponent<LaneHeroBot>();if(bot!=null)bot.enabled=false;}
            var opponent=lane.Heroes.Find(h=>h.Team==1);opponent.InvulnerableUntil=0;opponent.transform.position=new Vector3(1.5f,0,-11.5f);
            sprout1.transform.position=new Vector3(.4f,0,-11.5f);sprout1.InvulnerableUntil=0;float minionHp=sprout1.Health,heroHp=opponent.Health;
            world.Player.transform.position=new Vector3(-2,0,-14);world.CameraRig.Snap();lane.Autonomous=true;yield return Wait(.45f);lane.Autonomous=false;
            Check(sprout1.Health<minionHp && opponent.Health==heroHp,"Guardian fires a real seed projectile and prioritizes enemy sprouts over heroes");yield return Shot("22-guardian-defense");
            opponent.InvulnerableUntil=0;opponent.Damage(999,world.Player,Vector3.zero,0);
            Check(!opponent.Alive && !opponent.gameObject.activeSelf && lane.RespawnIn(opponent)>7 && lane.HeroKills[0]==1,"Defeated hero awards team score and enters an eight-second respawn queue");
            var killer=lane.Heroes.Find(h=>h.Team==1 && h.Alive);world.Player.InvulnerableUntil=0;world.Player.Cast(4,Vector3.zero);world.Player.Damage(999,killer,Vector3.zero,0);
            Check(world.State==SessionState.Battle && lane.RespawnIn(world.Player)>7 && lane.HeroKills[1]==1,"Player defeat does not end a lane match while its core survives");yield return Shot("23-lane-respawn");
            float remaining=lane.RespawnIn(world.Player);world.SetPaused(true);yield return Wait(.12f);
            Check(Mathf.Abs(lane.RespawnIn(world.Player)-remaining)<.04f,"Pause freezes hero respawn countdown");world.SetPaused(false);yield return Wait(8.2f);
            Check(world.Player.Alive && opponent.Alive && world.Player.Health==180 && world.Player.transform.position.z==-29 && opponent.transform.position.z==29,"Both teams respawn healthy at their own bases");
            Check(world.Player.Remaining(4)>.5f && Time.time<world.Player.InvulnerableUntil,"Respawn preserves running skill cooldowns and grants brief spawn protection");
            outer.Damage(99999,world.Player,Vector3.zero,0);yield return Wait(.06f);
            Check(!inner.Protected && core.Protected && lane.Goal(0)==inner,"Destroying the outer guardian opens the inner one but keeps the core protected");
            inner.Damage(99999,world.Player,Vector3.zero,0);yield return Wait(.06f);
            Check(!core.Protected && lane.Goal(0)==core,"Destroying both guardians opens the enemy flower core");
            core.Damage(99999,world.Player,Vector3.zero,0);yield return Wait(.06f);
            Check(world.State==SessionState.Victory && lane.Finished,"Destroying the enemy core wins the objective match");yield return Shot("24-lane-victory");
            int waves=lane.WavesSpawned;yield return Wait(.15f);Check(lane.WavesSpawned==waves,"Match end stops reinforcement spawning");
            world.StartBattle();yield return Wait(.12f);lane=world.Lane;lane.Autonomous=false;
            Check(world.IsLane && lane.Cores[1].Health==1800 && lane.HeroKills[0]==0 && lane.Heroes.Count==10,"Retry recreates a clean objective match with both teams");
            killer=lane.Heroes.Find(h=>h.Team==1);lane.Towers[0,0].Damage(99999,killer,Vector3.zero,0);lane.Towers[0,1].Damage(99999,killer,Vector3.zero,0);lane.Cores[0].Damage(99999,killer,Vector3.zero,0);yield return Wait(.08f);
            Check(world.State==SessionState.Defeat && lane.Finished,"Losing the allied core ends the match in defeat");yield return Shot("25-lane-defeat");
            world.PreferredMode=MatchMode.Garden;world.EnterPractice();yield return Wait(.15f);
            Check(!world.IsLane && world.Lane==null && world.Player.Alive && world.Dummy!=null && world.Transients.childCount==0,"Returning to practice clears lane actors, respawns and projectiles");
            world.StartBattle();yield return Wait(.08f);Check(world.Wave==1 && world.EnemyCount==2 && !world.IsLane,"Original three-wave arena remains selectable and starts normally");world.EnterPractice();yield return Wait(.08f);
        }
    }
}
