using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace BigBattle
{
    [TrackColor(.04f,.75f,.63f),TrackClipType(typeof(SkillVfxClip)),TrackBindingType(typeof(SkillSequence))]
    public sealed class SkillVfxTrack : TrackAsset
    {
        public override Playable CreateTrackMixer(PlayableGraph graph,GameObject go,int inputCount)=>ScriptPlayable<SkillVfxMixer>.Create(graph,inputCount);
    }
    public sealed class SkillVfxMixer : PlayableBehaviour
    {
        public override void ProcessFrame(Playable playable,FrameData info,object playerData)
        {
            var target=playerData as SkillSequence;if(target==null)return;
            // Also clear inactive clips when scrubbing backwards in the Timeline editor.
            for(int i=0;i<playable.GetInputCount();i++)target.Hide(((ScriptPlayable<SkillVfxBehaviour>)playable.GetInput(i)).GetBehaviour().Clip);
            for(int i=0;i<playable.GetInputCount();i++)
            {
                float weight=playable.GetInputWeight(i);if(weight<=0)continue;
                var input=(ScriptPlayable<SkillVfxBehaviour>)playable.GetInput(i);
                target.Sample(input.GetBehaviour().Clip,(float)(input.GetTime()/System.Math.Max(.0001,input.GetDuration())),weight);
            }
        }
    }
}
