using System.Collections;
using UnityEngine;

namespace BigBattle
{
    public sealed partial class SmokeTest
    {
        void ClearLaneAugments(LaneMatch lane){foreach(var h in lane.Heroes)if(h.Augments!=null)h.Augments.Clear();}
        IEnumerator LaneChecks()
        {
            Frame();world.SelectHero(HeroKind.Banana);world.BeginMayhem(HeroKind.Banana,true);yield return Wait(.15f);var lane=world.Lane;lane.Autonomous=false;ClearLaneAugments(lane);
            Check(world.IsLane && world.State==SessionState.Battle && world.Wave==0,"Mayhem starts a separate objective match without arena waves");
            Check(lane.Heroes.Count==10 && lane.Heroes.FindAll(h=>h.Team==0).Count==5 && lane.Heroes.TrueForAll(h=>h.VisualStyle==0 || h.VisualStyle==3 || h.VisualStyle==8),"Five versus five uses one human and nine random playable heroes, never preview Witch");
            Check(lane.Towers[0,0]!=null && lane.Towers[1,1]!=null && lane.NexusTowers[0,1]!=null && lane.Inhibitors[1]!=null && lane.Cores[1]!=null,"Both bases contain two lane towers, an inhibitor, two nexus towers and a nexus");
            Vector3 boundary=FlowerLaneMap.ToLane(FlowerLaneMap.Clamp(FlowerLaneMap.ToWorld(new Vector3(99,10,99))));Check(Mathf.Abs(boundary.x-9)<.001f && Mathf.Abs(boundary.z-44)<.001f && boundary.y==0,"Frozen bridge boundary clamps to its diagonal eighty-eight-unit map");
            Check(Mathf.Abs(FlowerLaneMap.ToLane(FlowerLaneMap.Clamp(FlowerLaneMap.ToWorld(new Vector3(99,0,0)))).x-6)<.001f,"Bridge parapets constrain the twelve-unit walk strip");
            Check(Vector3.Distance(world.Clamp(lane.Towers[0,0].transform.position),lane.Towers[0,0].transform.position)>1,"Living structures have solid movement footprints");
            world.CameraRig.MapOverview=true;world.CameraRig.Snap();yield return Wait(.2f);yield return Shot("21-mayhem-ice-bridge");world.CameraRig.MapOverview=false;world.CameraRig.Snap();
            Vector3 base0=world.CameraRig.Camera.WorldToScreenPoint(FlowerLaneMap.Spawn(0)),base1=world.CameraRig.Camera.WorldToScreenPoint(FlowerLaneMap.Spawn(1));Check(base0.x<base1.x && base0.y<base1.y,"Frozen bridge retains the lower-left to upper-right route");
            Vector3 forward=FlowerLaneMap.Forward(0),start=world.Player.transform.position;Frame(new CombatInput{Move=new Vector2(forward.x,forward.z)});yield return Wait(.3f);Frame();Check(Vector3.Distance(start,world.Player.transform.position)>1,"Keyboard-equivalent movement advances on the frozen bridge");
            lane.SpawnMinionWave();int melee=0,casters=0;LaneMinion ally=null,enemy=null;
            foreach(var actor in world.Actors)if(actor is LaneMinion){var m=(LaneMinion)actor;if(m.Team==0){if(m.Kind==0)melee++;else casters++;ally=m;}else enemy=m;}
            Check(melee==3 && casters==3,"Ordinary waves contain three melee and three caster minions per side");
            float z=FlowerLaneMap.ToLane(ally.transform.position).z;lane.Autonomous=true;yield return Wait(.65f);lane.Autonomous=false;
            Check(FlowerLaneMap.ToLane(ally.transform.position).z>z+1.2f,"Minions autonomously advance and steer around allied structures");
            float next=lane.NextWaveIn;world.SetPaused(true);Vector3 frozen=ally.transform.position;yield return Wait(.12f);Check(ally.transform.position==frozen && lane.NextWaveIn==next,"Pause freezes minion movement and wave timing");world.SetPaused(false);
            lane.SpawnMinionWave();lane.SpawnMinionWave();int siege=0;foreach(var actor in world.Actors)if(actor is LaneMinion && ((LaneMinion)actor).Kind==2)siege++;
            Check(siege==2 && lane.WavesSpawned==3,"Third waves add one siege cannon per side and never super minions");
            foreach(var hero in lane.Heroes){var bot=hero.GetComponent<LaneHeroBot>();if(bot!=null)bot.enabled=false;}
            var outer=lane.Towers[1,0];var inner=lane.Towers[1,1];var inhibitor=lane.Inhibitors[1];var core=lane.Cores[1];float hp=outer.Health;
            Check(!inner.Damage(100,world.Player,Vector3.zero) && !inhibitor.Damage(100,world.Player,Vector3.zero) && !core.Damage(100,world.Player,Vector3.zero),"Locked inner tower, inhibitor and nexus reject damage");
            outer.Damage(100,world.Player,Vector3.zero,0);Check(Mathf.Abs(outer.Health-(hp-20))<.01f,"Unaugmented unsupported structure attacks retain eighty-percent reduction");
            ally.transform.position=outer.transform.position-forward*2;hp=outer.Health;outer.Damage(100,world.Player,Vector3.zero,0);Check(Mathf.Abs(outer.Health-(hp-100))<.01f,"Nearby minion support removes backdoor reduction");Check(!outer.Bind(2),"Crystal structures reject crowd control");
            var opponent=lane.Heroes.Find(h=>h.Team==1);opponent.InvulnerableUntil=0;opponent.transform.position=FlowerLaneMap.ToWorld(new Vector3(1.5f,0,-10.5f));enemy.transform.position=FlowerLaneMap.ToWorld(new Vector3(.5f,0,-10.5f));
            hp=enemy.Health;float heroHp=opponent.Health;world.Player.transform.position=FlowerLaneMap.ToWorld(new Vector3(-2,0,-14));lane.Autonomous=true;yield return Wait(.55f);lane.Autonomous=false;
            Check(enemy.Health<hp && opponent.Health==heroHp,"A real turret projectile prioritizes minions before unprovoked heroes");yield return Shot("22-ice-tower-defense");
            world.Player.InvulnerableUntil=0;world.Player.Damage(1,opponent,Vector3.zero,0);heroHp=opponent.Health;lane.Autonomous=true;yield return new WaitForSeconds(1.25f);lane.Autonomous=false;Check(opponent.Health<heroHp,"A hero attacking the allied hero draws actual tower fire ahead of nearby minions");
            opponent.InvulnerableUntil=0;opponent.Damage(9999,world.Player,Vector3.zero,0);Check(!opponent.Alive && lane.RespawnIn(opponent)>7,"Hero death enters the eight-second respawn queue");
            var killer=lane.Heroes.Find(h=>h.Team==1 && h.Alive);world.Player.InvulnerableUntil=0;world.Player.Cast(4,Vector3.zero);world.Player.Damage(9999,killer,Vector3.zero,0);Check(world.State==SessionState.Battle && lane.RespawnIn(world.Player)>7,"A human death does not end the match while the nexus lives");yield return Shot("23-mayhem-respawn");
            float wait=lane.RespawnIn(world.Player);world.SetPaused(true);yield return Wait(.12f);Check(lane.RespawnIn(world.Player)==wait,"Pause freezes respawn countdowns");world.SetPaused(false);yield return Wait(8.2f);
            Check(world.Player.Alive && opponent.Alive && world.Player.Health==180 && Mathf.Abs(FlowerLaneMap.ToLane(world.Player.transform.position).z+39)<.01f,"Both sides respawn healthy at their frozen bases");Check(world.Player.Remaining(4)>.4f && Time.time<world.Player.InvulnerableUntil,"Respawn keeps running skill cooldowns and spawn protection");
            outer.Damage(99999,world.Player,Vector3.zero,0);yield return Wait(.06f);Check(!inner.Protected && inhibitor.Protected && lane.Goal(0)==inner,"Outer turret loss opens the inner turret only");
            inner.Damage(99999,world.Player,Vector3.zero,0);yield return Wait(.06f);Check(!inhibitor.Protected && core.Protected && lane.Goal(0)==inhibitor,"Two lane towers open the inhibitor without exposing the nexus");
            inhibitor.Damage(99999,world.Player,Vector3.zero,0);yield return Wait(.06f);Check(lane.InhibitorIn(1)>299 && !lane.NexusTowers[1,0].Protected && core.Protected,"Destroyed inhibitor starts its five-minute respawn and exposes both nexus towers");
            lane.NexusTowers[1,0].Damage(99999,world.Player,Vector3.zero,0);yield return Wait(.06f);Check(core.Protected,"One surviving nexus turret still protects the nexus");lane.NexusTowers[1,1].Damage(99999,world.Player,Vector3.zero,0);yield return Wait(.06f);Check(!core.Protected,"Destroying both nexus turrets exposes the nexus");
            core.Damage(99999,world.Player,Vector3.zero,0);yield return Wait(.06f);Check(world.State==SessionState.Victory && lane.Finished,"Nexus destruction ends the match in victory");yield return Shot("24-mayhem-victory");
            int waves=lane.WavesSpawned;yield return Wait(.12f);Check(lane.WavesSpawned==waves,"Victory stops further reinforcement spawning");
            world.BeginMayhem(HeroKind.Banana,true);yield return Wait(.1f);lane=world.Lane;lane.Autonomous=false;ClearLaneAugments(lane);killer=lane.Heroes.Find(h=>h.Team==1);
            foreach(var b in new[]{lane.Towers[0,0],lane.Towers[0,1],lane.Inhibitors[0],lane.NexusTowers[0,0],lane.NexusTowers[0,1],lane.Cores[0]})b.Damage(99999,killer,Vector3.zero,0);
            Check(world.State==SessionState.Defeat && lane.Finished,"The mirrored allied objective chain ends in defeat");yield return Shot("25-mayhem-defeat");
            world.PreferredMode=MatchMode.Garden;world.EnterPractice();yield return Wait(.1f);Check(!world.IsLane && world.Lane==null && world.Player.Augments==null && world.Player.Health==180 && world.Transients.childCount==0,"Returning to practice removes match buffs, lane actors, pending offers and temporary objects");
            world.StartBattle();yield return Wait(.08f);Check(!world.IsLane && world.Wave==1 && world.EnemyCount==2,"Original three-wave garden challenge remains available");world.EnterPractice();yield return Wait(.08f);
            yield return MayhemChecks();
        }
    }
}
