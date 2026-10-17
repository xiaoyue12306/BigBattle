using System.Collections.Generic;
using UnityEngine;

namespace BigBattle
{
    public sealed class YueyueVisual : MonoBehaviour
    {
        ActorVisual visual;Transform model,sword;SoulVfx chargeVfx,phaseVfx;
        readonly Dictionary<string,Transform> bones=new Dictionary<string,Transform>();readonly Dictionary<string,Quaternion> rest=new Dictionary<string,Quaternion>();
        static readonly string[] RaisedLimbs={"LeftUpperArm","RightUpperArm","LeftForearm","RightForearm"};
        bool charging,phase;float progress,releaseAt=-100;
        Material skinMaterial,phantomMaterial;Renderer[] skinRenderers;
        public bool HasModel=>model!=null;
        public int BoneCount=>bones.Count;
        public bool SwordVisible=>sword!=null && sword.gameObject.activeSelf;
        public static void Build(ActorVisual visual)
        {
            var rig=visual.gameObject.AddComponent<YueyueVisual>();rig.visual=visual;
            var asset=Resources.Load<GameObject>("Yueyue/Yueyue");
            if(asset!=null)
            {
                rig.model=Instantiate(asset,visual.Body,false).transform;rig.model.name="User supplied Yueyue • authored skeleton";
                foreach(var t in rig.model.GetComponentsInChildren<Transform>()){if(t.name.Contains("Arm") || t.name.Contains("Forearm") || t.name.Contains("Thigh") || t.name.Contains("Shin") || t.name=="Head" || t.name=="Spine"){rig.bones[t.name]=t;rig.rest[t.name]=t.localRotation;}}
                var texture=Resources.Load<Texture2D>("Yueyue/YueyueColor");var material=Art.Mat("Yueyue original texture",Color.white);material.mainTexture=texture;material.SetFloat("_Glossiness",.12f);
                rig.skinMaterial=material;rig.phantomMaterial=Art.FxMat("Yueyue intangible form",new Color(.02f,.8f,.65f,.43f));
                rig.skinRenderers=rig.model.GetComponentsInChildren<Renderer>();foreach(var r in rig.skinRenderers)r.sharedMaterial=material;
            }
            else Art.Shape("Missing Yueyue model",PrimitiveType.Capsule,visual.Body,Vector3.up,Vector3.one,GhostArt.Teal);
            rig.sword=GhostArt.Sword(visual.Body).transform;rig.sword.gameObject.SetActive(false);
            var actor=visual.GetComponentInParent<Combatant>();
            rig.chargeVfx=SoulVfx.Attach(visual.Body,actor.World,SoulEffect.Charge);rig.chargeVfx.Visible(false);
            rig.phaseVfx=SoulVfx.Attach(visual.Body,actor.World,SoulEffect.Phase);rig.phaseVfx.Visible(false);
        }
        public void Present(bool charge,float amount,bool ghost)
        {
            charging=charge;progress=amount;
            if(phase!=ghost && skinRenderers!=null)foreach(var r in skinRenderers)r.sharedMaterial=ghost?phantomMaterial:skinMaterial;
            phase=ghost;
        }
        public void Release(){releaseAt=Time.time;charging=false;}
        public void ResetPose(){releaseAt=-100;Present(false,0,false);sword.gameObject.SetActive(false);chargeVfx.Visible(false);phaseVfx.Visible(false);visual.Body.localScale=Vector3.one;}
        void Pose(string name,Vector3 euler)
        {
            Transform bone;if(!bones.TryGetValue(name,out bone))return;
            // Rotate in model-root axes, retaining each bone's FBX bind orientation.
            Quaternion delta=Quaternion.Inverse(bone.parent.rotation)*visual.Body.rotation*Quaternion.Euler(euler)*Quaternion.Inverse(visual.Body.rotation)*bone.parent.rotation;
            bone.localRotation=delta*rest[name];
        }
        void LateUpdate()
        {
            if(Time.timeScale==0)return;
            var actor=GetComponentInParent<Combatant>();if(actor==null)return;
            foreach(var p in bones){p.Value.localRotation=rest[p.Key];p.Value.localScale=Vector3.one;}
            if(!actor.Alive){visual.Body.localRotation=Quaternion.Euler(0,0,72);sword.gameObject.SetActive(false);chargeVfx.Visible(false);phaseVfx.Visible(false);return;}
            float gait=Mathf.Sin(Time.time*10)*(actor.Controlled?0:Mathf.Min(1,(actor.transform.position-lastPosition).magnitude/Mathf.Max(.001f,Time.deltaTime)*.22f));lastPosition=actor.transform.position;
            Pose("LeftThigh",new Vector3(gait*19,0,0));Pose("RightThigh",new Vector3(-gait*19,0,0));Pose("LeftShin",new Vector3(Mathf.Max(0,-gait)*15,0,0));Pose("RightShin",new Vector3(Mathf.Max(0,gait)*15,0,0));
            float action=visual.ActionFraction;
            if(action>=0 && action<1 && (visual.CurrentMotion==MotionId.Throw || visual.CurrentMotion==MotionId.Thrust))
            {float reach=Mathf.Sin(action*Mathf.PI);Pose("RightUpperArm",new Vector3(-65*reach,0,-12*reach));Pose("LeftUpperArm",new Vector3(-25*reach,0,15*reach));Pose("Spine",new Vector3(-4*reach,8*reach,0));}
            float slash=Mathf.Clamp01((Time.time-releaseAt)/.45f);bool releasing=Time.time-releaseAt<.55f;
            if(charging || releasing)
            {
                float raise=charging?Mathf.Clamp01(progress*5):1-slash;
                Pose("LeftUpperArm",new Vector3(-140*raise,0,-20*raise));Pose("RightUpperArm",new Vector3(-145*raise,0,15*raise));Pose("RightForearm",new Vector3(-20*raise,0,0));
                if(charging){foreach(string name in RaisedLimbs){Transform limb;if(bones.TryGetValue(name,out limb))limb.localScale=new Vector3(1,1.18f,1);}}
                sword.gameObject.SetActive(true);sword.localPosition=new Vector3(.18f,charging?2.28f:Mathf.Lerp(2.28f,.8f,slash),charging?-.12f:Mathf.Lerp(-.12f,1,slash));
                sword.localRotation=Quaternion.Euler(charging?-26+Mathf.Sin(Time.time*75)*progress*2:Mathf.Lerp(-26,125,slash),0,charging?Mathf.Sin(Time.time*83)*progress*2:0);
            }
            else sword.gameObject.SetActive(false);
            chargeVfx.Visible(charging);phaseVfx.Visible(phase);
            if(charging)
            {
                chargeVfx.Tip=sword.localPosition+sword.localRotation*Vector3.up*1.6f;chargeVfx.Progress=progress;
            }
            if(phase){visual.Body.localScale=new Vector3(.76f,1.06f,.76f);if(Time.time>=nextGhost){nextGhost=Time.time+.10f;actor.World.Fx.Ghost(visual);}}
        }
        Vector3 lastPosition;float nextGhost;
    }
}
