using System.Collections.Generic;
using UnityEngine;

namespace BigBattle
{
    public sealed class YueyueVisual : MonoBehaviour
    {
        ActorVisual visual;Transform model,sword,power;Transform[] bands=new Transform[2];Transform[] streams=new Transform[42];LineRenderer[] rays=new LineRenderer[12];
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
            var collection=new GameObject("Soul streams");collection.transform.SetParent(visual.Body,false);
            for(int i=0;i<rig.streams.Length;i++){rig.streams[i]=Art.Glint(collection.transform,i%3==0?GhostArt.Red:i%3==1?GhostArt.Blue:GhostArt.Teal,true).transform;rig.streams[i].localScale=Vector3.one*(i%4==0?.12f:.07f);}
            for(int i=0;i<rig.rays.Length;i++)rig.rays[i]=GhostArt.Line(collection.transform,"Gathered soul current",i%3==0?GhostArt.Red:i%3==1?GhostArt.Blue:GhostArt.Teal,.045f,12);
            rig.power=Art.Glint(collection.transform,GhostArt.Teal).transform;
            for(int i=0;i<2;i++){rig.bands[i]=Art.Crescent(collection.transform,i==0?GhostArt.Blue:GhostArt.Teal,290,.10f).transform;rig.bands[i].GetComponent<Renderer>().sharedMaterial.SetFloat("_Progress",.5f);rig.bands[i].localPosition=Vector3.up*.12f;}
            collection.SetActive(false);rig.collection=collection;
        }
        GameObject collection;
        public void Present(bool charge,float amount,bool ghost)
        {
            charging=charge;progress=amount;
            if(phase!=ghost && skinRenderers!=null)foreach(var r in skinRenderers)r.sharedMaterial=ghost?phantomMaterial:skinMaterial;
            phase=ghost;
        }
        public void Release(){releaseAt=Time.time;charging=false;}
        public void ResetPose(){releaseAt=-100;Present(false,0,false);sword.gameObject.SetActive(false);collection.SetActive(false);}
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
            if(!actor.Alive){visual.Body.localRotation=Quaternion.Euler(0,0,72);sword.gameObject.SetActive(false);collection.SetActive(false);return;}
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
            collection.SetActive(charging);
            if(charging)
            {
                Vector3 end=sword.localPosition+sword.localRotation*Vector3.up*1.6f;
                power.localPosition=end;power.localScale=Vector3.one*(.20f+progress*.38f+Mathf.Sin(Time.time*35)*.04f);power.rotation=Quaternion.LookRotation(new Vector3(0,18,-14));
                for(int i=0;i<2;i++){bands[i].localScale=Vector3.one*(1+progress*.8f);bands[i].localRotation=Quaternion.Euler(i==0?0:20,Time.time*(i==0?150:-180),0);}
                for(int i=0;i<streams.Length;i++)
                {
                    float t=(Time.time*(.6f+progress*.7f)+i*.618f)%1,a=i*2.399f+Time.time*.4f;Vector3 start=new Vector3(Mathf.Sin(a)*4,Mathf.Cos(a*1.5f)*1.6f+1.5f,Mathf.Cos(a)*4);
                    streams[i].localPosition=Vector3.Lerp(start,end,t*t)+Vector3.up*Mathf.Sin(t*Mathf.PI)*.5f;streams[i].rotation=Quaternion.LookRotation(new Vector3(0,18,-14));
                }
                for(int i=0;i<rays.Length;i++)for(int j=0;j<12;j++){float t=j/11f,a=i*2.399f+Time.time*.4f;Vector3 start=new Vector3(Mathf.Sin(a)*3.5f,Mathf.Cos(a)*1.1f+1.2f,Mathf.Cos(a)*3.5f);rays[i].SetPosition(j,Vector3.Lerp(start,end,t)+Vector3.up*Mathf.Sin(t*Mathf.PI)*Mathf.Sin(Time.time*7+i)*.25f);}
            }
            if(phase){visual.Body.localScale=new Vector3(.76f,1.06f,.76f);if(Time.time>=nextGhost){nextGhost=Time.time+.10f;actor.World.Fx.Ghost(visual);}}
        }
        Vector3 lastPosition;float nextGhost;
    }
}
