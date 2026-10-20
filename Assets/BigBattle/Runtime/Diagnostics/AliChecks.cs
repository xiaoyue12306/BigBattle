using System.Collections;
using UnityEngine;

namespace BigBattle
{
    public sealed partial class SmokeTest
    {
        Combatant BubbleTarget(Vector3 point,int team=1)
        {var go=new GameObject("Bubble acceptance target");go.transform.SetParent(world.transform);go.transform.position=point;var actor=go.AddComponent<Combatant>();actor.Initialize(world,team,2000,1);actor.enabled=false;return actor;}
        IEnumerator AliChecks()
        {
            Frame();world.SelectHero(HeroKind.Ali);yield return Wait(.1f);var ali=(AliPlayer)world.Player;var rig=ali.Visual.GetComponent<AliVisual>();
            Check(!world.PreviewOnly && ali.HeroName=="阿狸" && rig.ClipCount==4 && rig.BoneCount>=15 && Shader.Find("BigBattle/BubbleEnergy").isSupported,"Ali imports the supplied textured skin and four authored clips with a supported bubble shader");
            world.CameraRig.CaptureCloseup=world.CameraRig.CapturePortrait=true;world.CameraRig.CaptureSize=3.4f;world.CameraRig.Snap();yield return Shot("28-ali-model");
            world.CameraRig.CapturePortrait=false;world.CameraRig.CaptureSize=7;world.CameraRig.Snap();
            Frame(new CombatInput{Move=Vector2.right});yield return new WaitForSeconds(.18f);
            Frame(new CombatInput{Move=Vector2.right,Attack=true});yield return null;Frame(new CombatInput{Move=Vector2.right});
            Check(ali.Casts(0)==1 && ali.PendingBubbles==1 && !ali.Cast(0,Vector3.zero) && rig.WalkWeight>.2f,"One real attack-input frame places one bubble while a second cast is rejected by the placement interval");
            var field=world.Bubbles;var bomb=field.Bombs[0];Vector3 fixedPoint=bomb.Position;yield return new WaitForSeconds(.12f);Frame();yield return Wait(.12f);Vector3 scale=bomb.Visual.ShellScale;yield return Wait(.12f);
            Check(bomb.Position==fixedPoint && (ali.transform.position-fixedPoint).magnitude>.3f && bomb.Visual.ShellScale!=scale,"Placed bubble stays at its original point and breathes instead of following Ali");
            world.SetPaused(true);scale=bomb.Visual.ShellScale;float clock=bomb.Visual.VisualClock,animation=rig.SampleClock;yield return Wait(.15f);
            Check(bomb.Visual.VisualClock==clock && bomb.Visual.ShellScale==scale && rig.SampleClock==animation,"Pause freezes Ali animation, bubble fuse and breathing presentation");world.SetPaused(false);
            yield return Shot("29-ali-breathing-bubble");world.EnterPractice();yield return Wait(.06f);field=world.Bubbles;
            for(int i=0;i<5;i++)Check(field.Place(ali,new Vector3(i-2,0,-3),80),"Ali pending slot accepted "+(i+1));
            Check(!field.Place(ali,Vector3.zero,80) && ali.PendingBubbles==5,"A sixth unexploded bubble is rejected without consuming another slot");
            var other=BubbleTarget(Vector3.left*8,0);Check(field.Place(other,Vector3.left*8,80) && field.PendingFor(other)==1 && field.PendingFor(ali)==5,"Each bubble owner has an independent five-bubble allowance");
            world.EnterPractice();yield return Wait(.06f);field=world.Bubbles;field.Place(ali,Vector3.zero,80);yield return new WaitForSeconds(1.67f);
            Check(field.PendingCount==1 && field.Detonations==0,"Bubble cannot explode before its full 1.8-second scaled fuse");yield return new WaitForSeconds(.20f);
            Check(field.PendingCount==0 && field.Detonations==1 && field.WaveCount==1,"Fuse expiry releases its owner slot immediately while the cross wave is still visible");
            var wave=field.Waves[0];float reach=wave.Reach;clock=wave.Visual.VisualClock;world.SetPaused(true);yield return Wait(.12f);
            Check(wave.Reach==reach && wave.Visual.VisualClock==clock,"Pause freezes explosion expansion and the golden energy shader clock");yield return Shot("30-ali-cross-explosion");world.SetPaused(false);yield return new WaitForSeconds(.65f);
            Check(field.WaveCount==0 && field.GetComponentInChildren<BubbleVisual>()==null,"Bubble and explosion visuals expire completely");
            world.EnterPractice();yield return Wait(.06f);field=world.Bubbles;ali.transform.position=Vector3.zero;world.Dummy.transform.position=Vector3.right*9;
            Vector3[] points={Vector3.zero,Vector3.right*4.8f,Vector3.left*4.8f,Vector3.forward*4.8f,Vector3.back*4.8f};var targets=new Combatant[5];for(int i=0;i<5;i++)targets[i]=BubbleTarget(points[i]);
            var diagonal=BubbleTarget(new Vector3(2,0,2));var distant=BubbleTarget(Vector3.right*5.9f);var friend=BubbleTarget(Vector3.forward*2,0);var shield=BubbleTarget(Vector3.back*2);shield.InvulnerableUntil=Time.time+4;
            float health=ali.Health;field.Place(ali,Vector3.zero,80);Time.captureFramerate=5;for(int n=0;n<14;n++)yield return null;Time.captureFramerate=0;bool exact=true;foreach(var target in targets)exact&=target.TotalDamageReceived==80;
            Check(exact,"Even with 200ms simulation frames the center and four distant arms each receive exactly eighty once");
            Check(diagonal.TotalDamageReceived==0 && distant.TotalDamageReceived==0,"Cross blast does not damage diagonal or beyond-range enemies");
            Check(ali.Health==health && friend.TotalDamageReceived==0 && shield.TotalDamageReceived==0,"Bubble damage preserves self/friendly filtering and target invulnerability");
            yield return new WaitForSeconds(.7f);exact=true;foreach(var target in targets)exact&=target.TotalDamageReceived==80;Check(exact,"Explosion afterglow cannot repeat its damage");
            world.EnterPractice();yield return Wait(.06f);field=world.Bubbles;world.Dummy.transform.position=Vector3.forward*9;
            field.Place(ali,Vector3.left*4,80);yield return new WaitForSeconds(.65f);field.Place(ali,Vector3.zero,80);field.Place(ali,Vector3.right*4,80);yield return new WaitForSeconds(1.48f);
            Check(field.Detonations==3 && field.ChainDetonations==2 && field.PendingCount==0,"An expanding wave chains two younger bubbles early without recursive duplicate explosions");
            Check(field.Place(ali,Vector3.back*4,80),"Owner can place again immediately after chain detonation frees slots");world.EnterPractice();yield return Wait(.08f);
            Check(world.Transients.GetComponentInChildren<BubbleVisual>()==null && world.Bubbles.PendingCount==0 && world.Bubbles.WaveCount==0,"Session retry clears all armed bubbles, waves and owner accounting");
            field=world.Bubbles;var enemyOwner=BubbleTarget(new Vector3(4,0,3));var friendlyTarget=BubbleTarget(new Vector3(1,0,2),0);var enemyTarget=BubbleTarget(new Vector3(1,0,-2));
            field.Place(ali,Vector3.left*3,80);yield return new WaitForSeconds(.6f);field.Place(enemyOwner,Vector3.right,40);yield return new WaitForSeconds(1.6f);
            Check(field.ChainDetonations==1 && friendlyTarget.TotalDamageReceived==40 && enemyTarget.TotalDamageReceived==0,"Cross-owner chain detonation retains the triggered bubble's own damage and team attribution");world.EnterPractice();yield return Wait(.08f);
            field=world.Bubbles;int materials=Art.SharedMaterialCount;int sounds=world.Sound.BubblePlays;world.Sound.Muted=true;field.Place(ali,Vector3.zero,80);yield return new WaitForSeconds(2.5f);world.Sound.Muted=false;
            Check(world.Sound.BubblePlays==sounds && Art.SharedMaterialCount==materials && field.WaveCount==0,"Bubble placement and explosion obey mute and reuse warmed shared materials");
            world.EnterPractice();yield return Wait(.06f);field=world.Bubbles;
            for(int i=0;i<20;i++){var owner=BubbleTarget(Vector3.back*8,0);for(int n=0;n<5;n++)field.Place(owner,Vector3.back*8,80);}
            Check(field.PendingCount==BubbleField.GlobalLimit && !field.Place(ali,Vector3.zero,80),"Dense multi-owner bubbles respect the shared ninety-six reservation budget");world.EnterPractice();yield return Wait(.08f);
            world.StartBattle();Check(world.State==SessionState.Battle && world.Wave==1,"Ali can enter the original three-wave combat mode");foreach(var actor in world.Actors)if(actor!=ali)actor.enabled=false;
            field=world.Bubbles;var threat=BubbleTarget(Vector3.right*8);field.Place(threat,ali.transform.position,999);field.Place(ali,ali.transform.position+Vector3.right,80);yield return new WaitForSeconds(1.95f);
            Check(world.State==SessionState.Defeat && field.PendingCount==0 && field.WaveCount==0,"A lethal enemy bubble ends combat and immediately clears remaining waves and armed bubbles");world.EnterPractice();yield return Wait(.08f);
            world.StartLaneBattle();world.Lane.Autonomous=false;foreach(var hero in world.Lane.Heroes)hero.enabled=false;
            Check(world.IsLane && world.Lane.Heroes.Count==10 && world.Lane.Heroes.FindAll(h=>h is AliPlayer).Count==3,"Ali lane battle supports one human and nine AI with two explicit Ali bots");
            field=world.Bubbles;Vector3 lanePoint=FlowerLaneMap.ToWorld(new Vector3(5.5f,0,0));field.Place(ali,lanePoint,80);bomb=field.Bombs[0];
            Check(bomb.Across==FlowerLaneMap.Across && bomb.Forward==FlowerLaneMap.Forward(0),"Lane bubbles use the rotated map axes rather than world or facing axes");
            ali.Cast(0,Vector3.zero);float cooldown=ali.Remaining(0);ali.RespawnAt(FlowerLaneMap.Spawn(0),FlowerLaneMap.Forward(0));
            Check(ali.Remaining(0)==cooldown && field.PendingCount>=1,"Lane respawn preserves Ali placement cooldown and already armed bombs");yield return new WaitForSeconds(1.97f);bool floor=true;
            foreach(var w in field.Waves)for(int d=0;d<4;d++){Vector3 direction=d<2?w.Across*(d==0?1:-1):w.Forward*(d==2?1:-1);Vector3 end=w.Position+direction*w.Limits[d];floor&=(world.Clamp(end)-end).sqrMagnitude<.001f;}
            Check(floor && field.Detonations>=1,"Lane blast arms stop at the map boundary without bending outside the bridge");
            world.EnterPractice();world.SelectHero(HeroKind.Yueyue);yield return Wait(.08f);var mage=(YueyuePlayer)world.Player;
            var foe=new GameObject("Killable Ali snapshot fixture").AddComponent<AliPlayer>();foe.transform.SetParent(world.transform);foe.SetupTeam(world,1);foe.Damage(999,mage,Vector3.zero,0);
            Check(mage.Fallen.Style==8 && mage.Fallen.Name=="阿狸" && mage.Cast(2,Vector3.zero) && mage.Servant.MaxHealth==90 && mage.Servant.BaseAttackDamage==40,"A real defeated Ali returns with her own model and half health/damage");
            var spirit=mage.Servant;spirit.transform.position=world.Dummy.transform.position-Vector3.forward*2;float before=world.Dummy.TotalDamageReceived;yield return new WaitForSeconds(.65f);spirit.enabled=false;yield return new WaitForSeconds(2);
            Check(world.Dummy.TotalDamageReceived==before+40,"Returned Ali autonomously places a real forty-damage bubble");
            world.EnterPractice();yield return Wait(.08f);world.SetOpponent(HeroKind.Ali);world.EnterSparring();((AliDuelist)world.PracticeFoe).TrainingEnabled=false;
            world.PracticeFoe.Damage(999,mage,Vector3.zero,0);Check(world.PracticeFoe.IsDummy && mage.Souls==0,"Ali practice opponent restores safely without granting fake defeat souls");
            world.SetOpponent(HeroKind.Banana);world.CameraRig.CaptureCloseup=world.CameraRig.CapturePortrait=false;world.CameraRig.CaptureSize=6.8f;world.SelectHero(HeroKind.Banana);yield return Wait(.08f);
        }
    }
}
