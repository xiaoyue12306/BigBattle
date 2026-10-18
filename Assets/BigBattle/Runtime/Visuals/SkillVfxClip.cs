using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace BigBattle
{
    public enum SequenceShape { Ring,Seal,Rays,Arc,Spiral,Shards,Envelope }
    [Serializable]
    public sealed class SkillVfxClip : PlayableAsset,ITimelineClipAsset
    {
        public SequenceShape Shape;
        [ColorUsage(true,true)]public Color Tint=new Color(.05f,1,.7f,1);
        public Vector3 Offset,From=Vector3.one,To=Vector3.one,Tilt;
        public bool Ground=true,Billboard,FollowSword;
        public float Spin,Opacity=1;
        public AnimationCurve Envelope=new AnimationCurve(new Keyframe(0,0),new Keyframe(.12f,1),new Keyframe(.55f,.6f),new Keyframe(1,0));
        public AnimationCurve Expansion=AnimationCurve.EaseInOut(0,0,1,1);
        public ClipCaps clipCaps=>ClipCaps.Blending;
        public override Playable CreatePlayable(PlayableGraph graph,GameObject owner)
        {
            var playable=ScriptPlayable<SkillVfxBehaviour>.Create(graph);playable.GetBehaviour().Clip=this;return playable;
        }
    }
    public sealed class SkillVfxBehaviour : PlayableBehaviour {public SkillVfxClip Clip;}
}
