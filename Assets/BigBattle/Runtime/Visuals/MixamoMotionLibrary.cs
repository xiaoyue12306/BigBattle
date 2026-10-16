using System;
using UnityEngine;

namespace BigBattle
{
    public enum MotionId { Idle,Walk,Run,Attack1,Attack2,Attack3,Thrust,Throw,Dodge,Spin,Hurt,Death,Victory }

    [Serializable]
    public struct MotionPose
    {
        public float Height;
        public Vector3 Torso,LeftHand,RightHand,LeftFoot,RightFoot;
        public Quaternion LeftGrip,RightGrip;
        public static MotionPose Blend(MotionPose a,MotionPose b,float t)
        {
            return new MotionPose {Height=Mathf.Lerp(a.Height,b.Height,t),Torso=Vector3.Lerp(a.Torso,b.Torso,t),
                LeftHand=Vector3.Lerp(a.LeftHand,b.LeftHand,t),RightHand=Vector3.Lerp(a.RightHand,b.RightHand,t),
                LeftFoot=Vector3.Lerp(a.LeftFoot,b.LeftFoot,t),RightFoot=Vector3.Lerp(a.RightFoot,b.RightFoot,t),
                LeftGrip=Quaternion.Slerp(a.LeftGrip,b.LeftGrip,t),RightGrip=Quaternion.Slerp(a.RightGrip,b.RightGrip,t)};
        }
    }

    [Serializable]
    public sealed class MotionTrack
    {
        public MotionId Id;
        public string Source;
        public float Duration,Contact=.45f;
        public bool Loop;
        public MotionPose[] Frames;
        public MotionPose Sample(float normalized)
        {
            if(Frames==null || Frames.Length==0)return default(MotionPose);
            float frame=(Loop?Mathf.Repeat(normalized,1):Mathf.Clamp01(normalized))*(Frames.Length-1);
            int low=Mathf.FloorToInt(frame);return MotionPose.Blend(Frames[low],Frames[Mathf.Min(low+1,Frames.Length-1)],frame-low);
        }
    }

    // This asset is generated locally from the user's licensed Mixamo FBX files.
    // Neither the source files nor baked motion data are published to public Git.
    public sealed class MixamoMotionLibrary : ScriptableObject
    {
        public MotionTrack[] Tracks;
        public MotionTrack Find(MotionId id)
        {
            if(Tracks!=null)foreach(var track in Tracks)if(track.Id==id)return track;
            return null;
        }
    }
}
