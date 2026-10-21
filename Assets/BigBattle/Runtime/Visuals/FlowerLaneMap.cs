using UnityEngine;

namespace BigBattle
{
    // Original sunny single-lane garden. Decorations sit outside the walk strip;
    // movement remains an explicit flat-plane boundary, independent of colliders.
    public sealed class FlowerLaneMap : MonoBehaviour
    {
        public const float HalfLength=44;
        // Author geometry/rules in lane coordinates; the world camera stays fixed.
        // Positive Z therefore travels from the lower left to the upper right.
        public static readonly Quaternion Orientation=Quaternion.Euler(0,45,0);
        public static Vector3 ToWorld(Vector3 lane)=>Orientation*lane;
        public static Vector3 ToLane(Vector3 world)=>Quaternion.Inverse(Orientation)*world;
        public static Vector3 Across=>ToWorld(Vector3.right);
        public static Vector3 Forward(int team)=>ToWorld(team==0?Vector3.forward:Vector3.back);
        public static Vector3 Clamp(Vector3 p)
        {
            p=ToLane(p);
            p.y=0;p.z=Mathf.Clamp(p.z,-HalfLength,HalfLength);
            float width=Mathf.Lerp(6.0f,9.0f,Mathf.InverseLerp(30,39,Mathf.Abs(p.z)));
            p.x=Mathf.Clamp(p.x,-width,width);return ToWorld(p);
        }
        public static Vector3 Spawn(int team)=>ToWorld(new Vector3(0,0,team==0?-39:39));
        public void Build(){gameObject.AddComponent<AbyssMap>().Build();}
    }
}
