using System.Collections;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace BigBattle
{
    public sealed partial class SmokeTest
    {
        IEnumerator TimelineChecks()
        {
            world.EnterPractice();yield return Wait(.1f);
            bool assets=true;int authoredClips=0;
            foreach(string name in SkillSequence.Names)
            {
                var asset=Resources.Load<TimelineAsset>("SkillTimelines/"+name);assets&=asset!=null;if(asset==null)continue;
                int tracks=0;foreach(var track in asset.GetOutputTracks()){tracks++;foreach(var clip in track.GetClips()){authoredClips++;assets&=clip.asset is SkillVfxClip;}}
                assets&=tracks>=3;
            }
            Check(assets && authoredClips>=40,"Ten real editable Timeline assets and their custom track clips survive the Windows build");
            var shader=Shader.Find("BigBattle/SequenceEnergy");Check(shader!=null && shader.isSupported,"Timeline layer shader is retained by the Built-in player");
            var release=SkillSequence.Burst(world,"SoulRelease",Vector3.zero,Vector3.forward);yield return Wait(.07f);
            Check(release.Director.playableAsset is TimelineAsset && release.Director.timeUpdateMode==DirectorUpdateMode.Manual && release.VisibleLayers>=3,"A real manual PlayableDirector evaluates multiple staged release layers");
            double time=release.Director.time;yield return Wait(.1f);Check(release.Director.time>time,"Timeline playback advances on the actual combat clock");
            world.SetPaused(true);time=release.Director.time;float sample=release.SampleClock;var renderer=release.GetComponentInChildren<Renderer>();var block=new MaterialPropertyBlock();renderer.GetPropertyBlock(block);float clock=block.GetFloat("_Clock");
            yield return Wait(.15f);renderer.GetPropertyBlock(block);Check(release.Director.time==time && release.SampleClock==sample && block.GetFloat("_Clock")==clock,"Pause freezes the director, authored sampling and Timeline material clock");world.SetPaused(false);
            yield return Wait(1.15f);Check(release==null,"The final authored afterglow ends and destroys its temporary director");
            int materials=Art.SharedMaterialCount;
            for(int i=0;i<32;i++)SkillSequence.Burst(world,"SteelClash",new Vector3(i%4,1,i/4),Vector3.forward);
            Check(SkillSequence.LiveInstances<=SkillSequence.InstanceLimit && world.Fx.ActiveEffects<=Feedback.EffectLimit,"Simultaneous Timeline contacts obey graph and feedback budgets");
            yield return Wait(.5f);Check(SkillSequence.LiveInstances==0 && Art.SharedMaterialCount==materials,"Repeated Timeline contacts fully retire and reuse warmed materials");
            SkillSequence.Burst(world,"SoulRelease",Vector3.zero,Vector3.forward);world.EnterPractice();yield return Wait(.1f);
            Check(SkillSequence.LiveInstances==0 && world.Fx.GetComponentInChildren<PlayableDirector>()==null,"Retry destroys live directors without stale visual tracks");
        }
    }
}
