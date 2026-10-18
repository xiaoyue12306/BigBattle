using System.Collections;
using UnityEngine;

namespace BigBattle
{
    public sealed partial class SmokeTest
    {
        IEnumerator WitchChecks()
        {
            Frame();world.SelectHero(HeroKind.Banana);yield return Wait(.08f);world.OpenSelection();
            Frame(new CombatInput{MenuStep=-1});yield return null;Frame();yield return Wait(.05f);
            Check(GameRoot.HeroCount==3 && world.HeroSelection==2,"Three-character selection wraps backwards from banana to Witch");yield return Shot("26-witch-selection");
            world.SelectHero(HeroKind.Witch);yield return Wait(.12f);var witch=world.Player as WitchPlayer;var model=world.Player.Visual.GetComponent<WitchVisual>();var skin=world.Player.Visual.GetComponentInChildren<SkinnedMeshRenderer>();
            Check(witch!=null && world.PreviewOnly && world.State==SessionState.Practice && witch.VisualStyle==7,"Selecting Witch enters a distinct preview-only character in practice");
            Check(model!=null && model.HasModel && model.ClipCount>=3 && model.BoneCount>=11 && skin!=null && skin.sharedMaterial.mainTexture!=null,"User Witch FBX imports as a textured skin with authored Idle Walk Greeting clips");
            world.CameraRig.CaptureCloseup=true;world.CameraRig.CapturePortrait=true;world.CameraRig.CaptureSize=3.4f;world.CameraRig.Snap();yield return Wait(.5f);yield return Shot("27-witch-model");
            yield return Wait(1.7f);var thigh=System.Array.Find(witch.GetComponentsInChildren<Transform>(),t=>t.name=="LeftThigh");Quaternion rest=thigh.localRotation;Vector3 start=witch.transform.position;
            Frame(new CombatInput{Move=Vector2.right});yield return Wait(.19f);
            Check(Vector3.Distance(start,witch.transform.position)>.5f && model.WalkWeight>.5f && model.StridePhase>.5f && Quaternion.Angle(rest,thigh.localRotation)>.5f,"Witch actual movement drives the FBX walking clip and leg joints");
            world.SetPaused(true);float clock=model.SampleClock;Quaternion pose=thigh.localRotation;Vector3 point=witch.transform.position;yield return Wait(.15f);
            Check(model.SampleClock==clock && thigh.localRotation==pose && witch.transform.position==point,"Pause freezes manually sampled Witch FBX animation and position");world.SetPaused(false);Frame();yield return Wait(.35f);
            Check(model.WalkWeight<.1f,"Stopping blends the Witch back to her authored idle");
            float damage=world.Dummy.TotalDamageReceived;Frame(new CombatInput{Attack=true,Thrust=true,Peel=true,Dodge=true,Ultimate=true,UltimateHeld=true});yield return Wait(.2f);Frame();
            bool disabled=true;for(int i=0;i<5;i++)disabled&=!witch.Cast(i,Vector3.forward) && witch.Casts(i)==0;
            Check(disabled && world.Dummy.TotalDamageReceived==damage && world.Transients.childCount==0,"Undefined Witch attacks and abilities do not inherit banana skills or cause damage");
            var preferred=world.PreferredMode;world.StartBattle();world.StartLaneBattle();world.EnterSparring();
            Check(world.State==SessionState.Practice && !world.IsLane && world.Lane==null && world.PracticeFoe==null && world.PreferredMode==preferred,"Preview Witch cannot start arena lane or sparring and preserves preferred mode");
            witch.transform.position=Vector3.right*10.5f;Frame(new CombatInput{Move=Vector2.right});yield return Wait(.25f);Frame();
            Check(witch.transform.position.magnitude<=GardenArena.WalkRadius+.01f,"Witch movement obeys the shared active-map clamp");world.EnterPractice();yield return Wait(.05f);
            Check(model.SampleClock<.15f && model.WalkWeight<.1f,"Reset clears Witch animation clocks and movement blend");
            world.CameraRig.CapturePortrait=world.CameraRig.CaptureCloseup=false;world.CameraRig.CaptureSize=6.8f;
            world.SelectHero(HeroKind.Yueyue);yield return Wait(.1f);Check(world.Player is YueyuePlayer && !world.PreviewOnly,"Leaving Witch restores the real Yueyue character and combat policy");
            world.SelectHero(HeroKind.Banana);yield return Wait(.1f);Check(world.Player.HeroName=="香蕉刺客" && !world.PreviewOnly,"Leaving Witch restores the original banana character");
        }
    }
}
