using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace BigBattle
{
    // Standalone acceptance runner. Hardware reads are bypassed only when the
    // explicit --smoke-test argument is present; this never claims hardware QA.
    public sealed class SmokeTest : MonoBehaviour
    {
        [Serializable] sealed class Report
        {
            public string result="RUNNING";
            public string utc;
            public int passed;
            public int failed;
            public string graphics;
            public string hardware="Physical Xbox, dual keyboards and television: NOT TESTED";
            public List<string> checks=new List<string>();
            public List<string> errors=new List<string>();
        }
        GameRoot world;readonly Report report=new Report();string output;
        public void Setup(GameRoot game){world=game;}
        void Start()
        {
            output=Path.GetFullPath(Path.Combine(Application.dataPath,"../../../Artifacts/Smoke"));Directory.CreateDirectory(output);
            report.utc=DateTime.UtcNow.ToString("o");report.graphics=SystemInfo.graphicsDeviceName;
            Application.logMessageReceived+=Log;
            world.Input.Injected=true;
            StartCoroutine(Run());
        }
        void Log(string condition,string stack,LogType type)
        {
            if(type==LogType.Error || type==LogType.Exception || type==LogType.Assert)report.errors.Add(condition+"\n"+stack);
        }
        void Check(bool passed,string name)
        {
            report.checks.Add((passed?"PASS ":"FAIL ")+name);if(passed)report.passed++;else report.failed++;
            Debug.Log("BIGBATTLE_CHECK "+(passed?"PASS ":"FAIL ")+name);
        }
        void Frame(CombatInput input=default(CombatInput)){world.Input.InjectedFrame=input;}
        IEnumerator Wait(float duration){yield return new WaitForSecondsRealtime(duration);}
        IEnumerator Shot(string name)
        {
            yield return new WaitForEndOfFrame();
            Texture2D image=ScreenCapture.CaptureScreenshotAsTexture();
            var colors=new HashSet<Color32>();
            for(int y=0;y<image.height;y+=60)for(int x=0;x<image.width;x+=60)colors.Add(image.GetPixel(x,y));
            Check(colors.Count>12,"Rendered screenshot is nonblank: "+name);
            File.WriteAllBytes(Path.Combine(output,name+".png"),image.EncodeToPNG());Destroy(image);
        }
        IEnumerator Run()
        {
            while(!UnityEngine.Rendering.SplashScreen.isFinished)yield return null;
            yield return Wait(.7f);
            Check(world.State==SessionState.Practice && world.Dummy!=null,"Boot into practice with dummy");
            Check(world.CameraRig.Camera.orthographic,"Overhead orthographic camera");
            Check(world.Player.Visual.UsesMixamo && world.Player.Visual.MotionCount==13,"Thirteen real Mixamo motion tracks loaded");
            Check(world.Sound.GeneratedClipCount==60 && world.Sound.SamplesFinite && world.Sound.MaximumSample<=.761f,"Clean sound bank has 60 finite headroom-limited variants including steel clashes");
            world.Sound.ExportDemo(Path.Combine(output,"combat-audio-demo.wav"));
            yield return Shot("01-practice");
            Vector3 initial=world.Player.transform.position;
            Frame(new CombatInput{Move=Vector2.up});yield return Wait(.62f);Frame();yield return Wait(.04f);
            Check(world.Player.transform.position.z>initial.z+2,"Keyboard-equivalent movement");
            Check(Vector3.Dot(world.Player.Facing,Vector3.forward)>.99f,"Movement sets facing; stopping preserves it");
            float damage=world.Dummy.TotalDamageReceived;
            Frame(new CombatInput{Attack=true});yield return Wait(.10f);Frame();yield return Wait(.05f);
            Check(world.Dummy.TotalDamageReceived>damage,"Basic attack damages front target");
            Check(world.Fx.SlashCount>0 && world.Fx.ImpactCount>0 && world.Sound.ImpactPlays>0 && world.HitPauseCount>0,"Successful contact synchronizes ribbon, impact, sound and hit pause");
            damage=world.Dummy.TotalDamageReceived;
            Frame(new CombatInput{Thrust=true});yield return Wait(.03f);Frame();yield return Wait(.25f);
            Check(world.Player.Casts(1)==1 && world.Dummy.TotalDamageReceived>damage,"Thrust damages and lunges (casts="+world.Player.Casts(1)+", damage="+(world.Dummy.TotalDamageReceived-damage)+")");
            world.Player.transform.position=new Vector3(0,0,-2.4f);
            damage=world.Dummy.TotalDamageReceived;
            Frame(new CombatInput{Peel=true});yield return Wait(.03f);Frame();yield return Wait(.75f);
            Check(world.Player.Casts(2)==1 && world.Dummy.TotalDamageReceived>=damage+20,"Peel toss lands and triggers damage");
            var source=world.SpawnEnemy(new Vector3(8,0,0),60);
            Frame(new CombatInput{Dodge=true});yield return Wait(.03f);Frame();
            float hp=world.Player.Health;
            bool hit=world.Player.Damage(10,source,Vector3.zero);
            Check(!hit && world.Player.Health==hp,"Dodge grants temporary invulnerability");
            yield return Wait(.35f);
            Check(world.Player.Damage(10,source,Vector3.zero),"Invulnerability expires");
            world.Player.transform.position=new Vector3(0,0,-.5f);
            damage=world.Dummy.TotalDamageReceived;
            Frame(new CombatInput{Ultimate=true});yield return Wait(.03f);Frame();yield return Wait(.95f);
            Check(world.Player.Casts(4)==1 && world.Dummy.TotalDamageReceived>=damage+65,"Whirlwind deals repeated area damage");
            Check(world.Player.Remaining(4)>5,"Ultimate cooldown is active");
            Check(!world.Player.Cast(4,Vector3.zero),"Cooldown rejects repeated casts");
            Check(!world.Player.Damage(10,world.Player,Vector3.zero),"Team filtering prevents friendly fire");
            Vector3 behind=world.Player.transform.position-Vector3.forward*2;
            source.transform.position=behind;
            Check(Vector3.Dot(world.Assist(world.Player,Vector3.forward,4),Vector3.forward)>.99f,"Aim assist never turns toward rear target");
            Vector3 dummyPosition=world.Dummy.transform.position;
            world.Dummy.transform.position=Vector3.left*8;
            source.transform.position=world.Player.transform.position+new Vector3(.30f,0,2);
            Vector3 assisted=world.Assist(world.Player,Vector3.forward,4);
            Check(assisted.x>0 && Vector3.Angle(assisted,Vector3.forward)<=10.4f,"Front-cone aim correction stays small");
            world.Dummy.transform.position=dummyPosition;
            source.transform.position=new Vector3(7,0,0);
            float enemyHealth=source.Health;
            world.SpawnPeel(source.transform.position-Vector3.forward*3.4f,Vector3.forward);
            yield return Wait(.65f);
            Check(source.Health==enemyHealth-20 && source.StunnedUntil>Time.time,"Peel visibly slips and stuns an enemy");
            yield return Shot("02-combat-practice");
            world.SetPaused(true);Vector3 pausedPosition=world.Player.transform.position;
            Frame(new CombatInput{Move=Vector2.right});yield return Wait(.15f);Frame();
            Check(Time.timeScale==0 && Vector3.Distance(pausedPosition,world.Player.transform.position)<.001f,"Pause freezes movement");
            yield return Shot("03-pause");world.SetPaused(false);
            world.StartBattle();yield return Wait(.10f);
            Check(world.State==SessionState.Battle && world.EnemyCount==2 && world.Wave==1,"Battle starts first finite wave");
            MochiEnemy enemy=null;foreach(var actor in world.Actors)if(actor is MochiEnemy){enemy=(MochiEnemy)actor;break;}
            float distance=Vector3.Distance(world.Player.transform.position,enemy.transform.position);
            yield return Wait(.6f);
            Check(Vector3.Distance(world.Player.transform.position,enemy.transform.position)<distance-.4f,"Enemy AI pursues player");
            world.Player.transform.position=enemy.transform.position+Vector3.back*1.3f;
            yield return Wait(.12f);
            Check(enemy.WindingUp,"Enemy telegraphs attack before damage");
            yield return Shot("04-battle-telegraph");
            hp=world.Player.Health;yield return Wait(.75f);
            Check(world.Player.Health<hp,"Enemy strike damages player after warning");
            world.Player.Restore();world.Player.transform.position=new Vector3(0,0,-8);
            for(int wave=1;wave<=3;wave++)
            {
                Combatant[] targets=world.Actors.ToArray();
                foreach(var actor in targets)if(actor!=world.Player && actor.Alive)actor.Damage(999,world.Player,Vector3.zero);
                yield return Wait(wave<3?2.3f:.1f);
                Check(wave==3?world.State==SessionState.Victory:world.Wave==wave+1,"Wave progression "+wave);
            }
            Check(world.Score==9,"Three waves contain nine enemies in total");
            yield return Shot("05-victory");
            world.StartBattle();yield return Wait(.05f);
            Combatant attacker=null;foreach(var actor in world.Actors)if(actor.Team==1){attacker=actor;break;}
            world.Player.Damage(999,attacker,Vector3.zero);
            Check(world.State==SessionState.Defeat,"Player death ends battle");
            yield return Shot("06-defeat");
            world.EnterPractice();yield return Wait(.10f);
            Check(world.State==SessionState.Practice && world.Player.Alive && world.Player.Health==180 && world.EnemyCount==0,"Retry cleans enemies and restores player");
            Check(world.Player.Remaining(4)==0,"Retry resets cooldowns");
            Check(GardenArena.Clamp(new Vector3(100,10,100)).magnitude<=GardenArena.WalkRadius+.001f,"Arena bounds clamp movement");
            Check(LocalInput.Deadzone(new Vector2(.1f,.1f))==Vector2.zero,"Controller dead zone removes drift");
            Check(LocalInput.Deadzone(new Vector2(1,1)).magnitude<=1.001f,"Controller diagonal speed normalized");
            for(int count=1;count<=5;count++)
            {
                Rect[] rectangles=SplitScreenLayout.ForPlayers(count);float area=0;bool valid=rectangles.Length==count;
                foreach(Rect rect in rectangles){area+=rect.width*rect.height;valid&=rect.xMin>=0 && rect.yMin>=0 && rect.xMax<=1.001f && rect.yMax<=1.001f;}
                for(int i=0;i<count;i++)for(int j=i+1;j<count;j++)valid&=!rectangles[i].Overlaps(rectangles[j]);
                Check(valid && Mathf.Abs(area-1)<.001f,"Viewport geometry "+count+" players");
            }
            Check(SplitScreenLayout.MaxPlayers==5,"Five-player expansion contract");
            Frame();yield return Shot("07-reset-practice");
            world.Player.transform.position=new Vector3(0,0,-1);
            Frame(new CombatInput{Move=Vector2.up});yield return null;Frame();yield return Wait(.05f);
            var comboBefore=(int[])world.Player.ComboUsage.Clone();
            int impactsBefore=world.Fx.ImpactCount;
            Frame(new CombatInput{Attack=true});yield return Wait(.12f);yield return Shot("08-combo-contact");
            yield return Wait(1.15f);Frame();yield return Wait(.16f);
            Check(world.Player.ComboUsage[0]>comboBefore[0] && world.Player.ComboUsage[1]>comboBefore[1] && world.Player.ComboUsage[2]>comboBefore[2],"Holding attack advances all three combo motions");
            Check(world.Fx.ImpactCount>impactsBefore,"Combo hit events connect with dummy");
            world.SetPaused(true);yield return Wait(.08f);Check(Time.timeScale==0,"Pause takes precedence over hit pause");
            world.SetPaused(false);yield return Wait(.10f);Check(Mathf.Approximately(Time.timeScale,1),"Impact clock recovers after resume");
            // Exercise real practice-mode controls, then the shared resolver with
            // controlled geometry so AI scheduling cannot conceal ordering bugs.
            Frame(new CombatInput{Sparring=true});yield return null;Frame();yield return Wait(.05f);
            Check(world.Duelist!=null && world.Dummy==null && world.Duelist.Visual.UsesMixamo,"Sparring control creates a second animated banana with a weapon");
            float timeout=Time.unscaledTime+2;
            while(!world.Duelist.WindingUp && Time.unscaledTime<timeout)yield return null;
            Check(world.Duelist.WindingUp,"Armed practice AI gives a visible windup");
            hp=world.Player.Health;yield return Wait(.75f);
            Check(world.Player.Health<hp,"Armed practice AI uses the shared melee damage path");
            var duelist=world.Duelist;duelist.TrainingEnabled=false;
            PrepareDuel(duelist);
            Frame(new CombatInput{Move=Vector2.up});yield return null;Frame();
            int clashes=world.Melee.ClashCount,clang=world.Sound.ClashPlays,hitSounds=world.Sound.ImpactPlays;
            float playerDamage=world.Player.TotalDamageReceived,duelDamage=duelist.TotalDamageReceived;
            // Opponent registered first: near-simultaneous real player Cast.
            world.Melee.Slash(duelist,Vector3.back,0,.10f);
            Check(world.Player.Cast(0,Vector3.zero),"Player can start a real combo against a declared incoming swing");
            duelist.Visual.PlayMotion(MotionId.Attack1,.34f);
            yield return Wait(.17f);
            Check(world.Melee.ClashCount==clashes+1,"Near-simultaneous facing blades clash");
            Check(world.Player.TotalDamageReceived==playerDamage && duelist.TotalDamageReceived==duelDamage && world.Player.Health==180 && duelist.Health==360,"Clash cancels both outgoing and incoming damage");
            Check(world.Sound.ClashPlays==clang+1 && world.Sound.ImpactPlays==hitSounds && world.Fx.ClashCount>0,"Clash produces one steel sound and sparks without a damage impact sound");
            yield return Shot("09-blade-clash");
            PrepareDuel(duelist);clashes=world.Melee.ClashCount;
            world.Melee.Slash(world.Player,Vector3.forward,0,.06f);world.Melee.Slash(duelist,Vector3.back,0,.12f);
            yield return Wait(.23f);
            Check(world.Melee.ClashCount==clashes+1 && world.Player.Health==180 && duelist.Health==360,"Reverse registration and 60ms offset still cancel both strikes");
            PrepareDuel(duelist);clashes=world.Melee.ClashCount;duelDamage=duelist.TotalDamageReceived;
            world.Melee.Slash(world.Player,Vector3.forward,0,.04f);world.Melee.Slash(duelist,Vector3.back,0,.04f+MeleeResolver.ClashWindow+.02f);
            yield return Wait(.20f);
            Check(world.Melee.ClashCount==clashes && duelist.TotalDamageReceived>duelDamage,"Outside the 75ms timing window a normal hit remains damaging");
            PrepareDuel(duelist);clashes=world.Melee.ClashCount;duelDamage=duelist.TotalDamageReceived;
            world.Melee.Slash(world.Player,Vector3.forward,0,.05f);world.Melee.Slash(duelist,Vector3.forward,0,.05f);
            yield return Wait(.15f);
            Check(world.Melee.ClashCount==clashes && duelist.TotalDamageReceived>duelDamage,"A back-facing weapon cannot parry a frontal hit");
            PrepareDuel(duelist);clashes=world.Melee.ClashCount;duelist.transform.position=Vector3.forward*4;
            world.Melee.Slash(world.Player,Vector3.forward,0,.05f);world.Melee.Slash(duelist,Vector3.back,0,.05f);
            yield return Wait(.15f);
            Check(world.Melee.ClashCount==clashes && world.Player.Health==180 && duelist.Health==360,"Distant blades neither clash nor hit");
            PrepareDuel(duelist);
            world.Melee.Slash(world.Player,Vector3.forward,0,.1f);
            Check(world.Player.Cast(3,Vector3.right),"Dodge can cancel an announced swing");
            duelDamage=duelist.TotalDamageReceived;yield return Wait(.2f);
            Check(duelist.TotalDamageReceived==duelDamage && world.Melee.PendingCount==0,"Cancelled swing leaves no delayed damage");
            PrepareDuel(duelist);
            world.Melee.Slash(world.Player,Vector3.forward,0,.15f);
            world.SetPaused(true);yield return Wait(.20f);
            Check(world.Melee.PendingCount==1 && duelist.Health==360,"Pause freezes pending blade contacts");
            world.SetPaused(false);world.EnterPractice();yield return Wait(.22f);
            Check(world.Duelist==null && world.Melee.PendingCount==0 && world.Player.Health==180,"Session reset clears pending contacts and the sparring opponent");
            // A quick tap during recovery is buffered; release must not drop it.
            world.Player.transform.position=new Vector3(0,0,-1);
            Frame(new CombatInput{Move=Vector2.up});yield return null;Frame();
            Frame(new CombatInput{Attack=true});yield return Wait(.05f);Frame();yield return Wait(.08f);
            float castsBefore=world.Player.Casts(0);
            Frame(new CombatInput{Attack=true});yield return null;Frame();yield return Wait(.25f);
            Check(world.Player.Casts(0)==castsBefore+1 && world.Player.ComboStep==1,"Quick second tap buffers through recovery into the second combo strike");
            yield return Wait(1.15f);
            Frame(new CombatInput{Attack=true});yield return null;Frame();
            Check(world.Player.ComboStep==0,"Combo returns to its opening strike after idle timeout");
            int soundCount=world.Sound.ImpactPlays;world.Sound.Muted=true;world.Sound.Impact(true);
            Check(world.Sound.ImpactPlays==soundCount,"Mute suppresses impact playback");world.Sound.Muted=false;
            clang=world.Sound.ClashPlays;world.Sound.Muted=true;world.Sound.Play(SoundCue.Clash);
            Check(world.Sound.ClashPlays==clang,"Mute also suppresses steel collision playback");world.Sound.Muted=false;
            yield return Wait(.65f);Check(world.Fx.ActiveEffects<35,"Short-lived impact effects retire after combat");
            Check(report.errors.Count==0,"No runtime errors or exceptions");
            report.result=report.failed==0?"PASS":"FAIL";
            File.WriteAllText(Path.Combine(output,"report.json"),JsonUtility.ToJson(report,true));
            Debug.Log("BIGBATTLE_SMOKE_"+report.result+" passed="+report.passed+" failed="+report.failed);
            Application.Quit(report.failed==0?0:1);
        }
        void PrepareDuel(BananaDuelist duelist)
        {
            Frame();world.Melee.Clear();world.Fx.Clear();world.Player.ResetCombat();duelist.Restore();
            world.Player.transform.position=Vector3.back*.8f;duelist.transform.position=Vector3.forward*.8f;
            duelist.transform.rotation=Quaternion.LookRotation(Vector3.back);
        }
        void OnDestroy(){Application.logMessageReceived-=Log;}
    }
}
