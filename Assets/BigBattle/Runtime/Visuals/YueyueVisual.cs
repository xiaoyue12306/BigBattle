using System.Collections.Generic;
using UnityEngine;

namespace BigBattle
{
    // Presentation owns the body/skeleton only; never moves the combat actor.
    [DefaultExecutionOrder(20)]
    public sealed class YueyueVisual : MonoBehaviour
    {
        sealed class Joint {public Transform transform;public Quaternion rest,smoothed;public Vector3 position,angles;}
        static readonly string[] JointNames={"Pelvis","Spine","Head","LeftUpperArm","LeftForearm","LeftHand","RightUpperArm","RightForearm","RightHand","LeftThigh","LeftShin","RightThigh","RightShin"};
        readonly Dictionary<string,Joint> bones=new Dictionary<string,Joint>();
        ActorVisual visual;Combatant actor;Transform model,sword;SoulVfx chargeVfx,phaseVfx;
        Material skinMaterial,phantomMaterial;Renderer[] skinRenderers;
        bool charging,phase,initialized;float progress,releaseAt=-100,castAt=-100,castDuration;
        int skill=-1,variant,castSequence;float speedWeight,stride,turn,lastYaw,nextGhost;
        Vector3 lastPosition,bodyAngles,bodyOffset;
        public bool HasModel=>model!=null;
        public int BoneCount=>bones.Count;
        public bool SwordVisible=>sword!=null && sword.gameObject.activeSelf;
        public float WalkWeight=>speedWeight;
        public float StridePhase=>stride;
        public int CastingSkill=>Time.time-castAt<castDuration?skill:-1;
        public int BasicVariant=>variant;
        public Vector3 GripPosition=>HandMidpoint();
        public Vector3 SwordPosition=>sword.position;
        public Vector3 HandPosition(bool left){Joint b;return bones.TryGetValue(left?"LeftHand":"RightHand",out b)?b.transform.position:actor.transform.position;}
        public static void Build(ActorVisual visual)
        {
            var rig=visual.gameObject.AddComponent<YueyueVisual>();rig.visual=visual;rig.actor=visual.GetComponentInParent<Combatant>();visual.ExternalBodyPose=true;
            var asset=Resources.Load<GameObject>("Yueyue/Yueyue");
            if(asset!=null)
            {
                rig.model=Instantiate(asset,visual.Body,false).transform;rig.model.name="User Yueyue • refined asymmetric arm rig";
                foreach(var t in rig.model.GetComponentsInChildren<Transform>())if(System.Array.IndexOf(JointNames,t.name)>=0)rig.bones[t.name]=new Joint{transform=t,rest=t.localRotation,smoothed=t.localRotation,position=t.localPosition};
                var material=Art.Mat("Yueyue original texture",Color.white);material.mainTexture=Resources.Load<Texture2D>("Yueyue/YueyueColor");material.SetFloat("_Glossiness",.12f);
                rig.skinMaterial=material;rig.phantomMaterial=Art.FxMat("Yueyue intangible form",new Color(.02f,.8f,.65f,.43f));
                rig.skinRenderers=rig.model.GetComponentsInChildren<Renderer>();foreach(var r in rig.skinRenderers)r.sharedMaterial=material;
            }
            else Art.Shape("Missing Yueyue model",PrimitiveType.Capsule,visual.Body,Vector3.up,Vector3.one,GhostArt.Teal);
            rig.sword=GhostArt.Sword(visual.Body).transform;rig.sword.gameObject.SetActive(false);
            rig.chargeVfx=SoulVfx.Attach(visual.Body,rig.actor.World,SoulEffect.Charge);rig.chargeVfx.Visible(false);
            rig.phaseVfx=SoulVfx.Attach(visual.Body,rig.actor.World,SoulEffect.Phase);rig.phaseVfx.Visible(false);
            rig.initialized=true;rig.ResetPose();
        }
        public void PlayCast(int action)
        {
            skill=action;castAt=Time.time;castDuration=action==0?.58f:action==1?.62f:action==2?1.05f:.52f;
            if(action==0)variant=castSequence++%3;
        }
        public void Present(bool charge,float amount,bool ghost)
        {
            if(charge && !charging){skill=-1;castAt=-100;}charging=charge;progress=amount;
            if(phase!=ghost && skinRenderers!=null)foreach(var r in skinRenderers)r.sharedMaterial=ghost?phantomMaterial:skinMaterial;
            phase=ghost;
        }
        public void Release(){releaseAt=Time.time;charging=false;castAt=-100;skill=-1;}
        public void ResetPose()
        {
            releaseAt=castAt=-100;skill=-1;castSequence=0;speedWeight=stride=turn=0;Present(false,0,false);
            sword.gameObject.SetActive(false);chargeVfx.Visible(false);phaseVfx.Visible(false);
            visual.Body.localPosition=Vector3.zero;visual.Body.localRotation=Quaternion.identity;visual.Body.localScale=Vector3.one;
            foreach(var b in bones.Values){b.transform.localPosition=b.position;b.transform.localRotation=b.smoothed=b.rest;b.transform.localScale=Vector3.one;}
            lastPosition=actor.transform.position;lastYaw=actor.transform.eulerAngles.y;nextGhost=Time.time;
        }
        void Add(string name,Vector3 angles){Joint b;if(bones.TryGetValue(name,out b))b.angles+=angles;}
        static float Ease(float t)=>Mathf.SmoothStep(0,1,Mathf.Clamp01(t));
        static Vector3 Gesture(float t,Vector3 wind,Vector3 strike,float contact)
        {
            float prepare=contact*.48f;
            if(t<prepare)return Vector3.Lerp(Vector3.zero,wind,Ease(t/prepare));
            if(t<contact)return Vector3.Lerp(wind,strike,Ease((t-prepare)/(contact-prepare)));
            return Vector3.Lerp(strike,Vector3.zero,Ease((t-contact)/(1-contact)));
        }
        void Locomotion(float dt)
        {
            Vector3 delta=actor.transform.position-lastPosition;delta.y=0;lastPosition=actor.transform.position;
            // Teleports/respawns never become an enormous animation stride.
            float distance=delta.magnitude;if(distance>1.5f)distance=0;
            float speed=actor.Controlled?0:Mathf.Min(1,distance/Mathf.Max(.001f,dt)/4.8f);
            speedWeight=Mathf.MoveTowards(speedWeight,speed,dt*5.5f);stride=Mathf.Repeat(stride+distance*Mathf.PI*2/1.55f,Mathf.PI*2);
            float s=Mathf.Sin(stride),c=Mathf.Cos(stride),w=speedWeight*(charging?.22f:phase?.32f:1);
            float yaw=actor.transform.eulerAngles.y,yawSpeed=Mathf.DeltaAngle(lastYaw,yaw)/Mathf.Max(.001f,dt);lastYaw=yaw;
            turn=Mathf.Lerp(turn,Mathf.Clamp(yawSpeed/250,-1,1),1-Mathf.Exp(-dt*9));
            bodyOffset=new Vector3(s*.018f*w,(1-Mathf.Cos(stride*2))*.017f*w+Mathf.Sin(Time.time*2.5f)*.006f,0);
            bodyAngles=new Vector3(w*4,turn*2,-s*2.5f*w-turn*w*4);
            Add("Pelvis",new Vector3(0,s*4*w,-s*3*w));Add("Spine",new Vector3(-w*2,-s*6*w,-turn*w*3));
            Add("Head",new Vector3(Mathf.Sin(Time.time*1.8f)*1.3f,Mathf.Sin(Time.time*.7f)*2-s*2*w,s*1.8f*w));
            Add("LeftThigh",new Vector3(s*29*w,0,-2*w));Add("RightThigh",new Vector3(-s*29*w,0,2*w));
            Add("LeftShin",new Vector3(Mathf.Max(0,-s)*42*w,0,0));Add("RightShin",new Vector3(Mathf.Max(0,s)*42*w,0,0));
            Add("LeftUpperArm",new Vector3(-s*18*w,3*w,Mathf.Sin(Time.time*2)*1.2f-3*w));Add("RightUpperArm",new Vector3(s*23*w,-3*w,3*w));
            Add("LeftForearm",new Vector3(c*6*w,0,0));Add("RightForearm",new Vector3(-7*w-c*5*w,0,0));
            Add("LeftHand",new Vector3(0,Mathf.Sin(Time.time*2.5f)*2,-c*3*w));Add("RightHand",new Vector3(0,0,c*4*w));
        }
        void Casting()
        {
            float t=(Time.time-castAt)/Mathf.Max(.01f,castDuration);if(t<0 || t>=1 || actor.Controlled)return;
            float contact=skill==0?.60f:skill==1?.20f:skill==2?.5f:.35f;
            if(skill==0)
            {
                bool left=variant==1;string lead=left?"Left":"Right",other=left?"Right":"Left";float sign=left?-1:1;
                Add(lead+"UpperArm",Gesture(t,new Vector3(14,-sign*16,-sign*8),new Vector3(-72,sign*8,-sign*16),contact));
                Add(lead+"Forearm",Gesture(t,new Vector3(-28,0,0),new Vector3(-12,0,sign*12),contact));
                Add(lead+"Hand",Gesture(t,new Vector3(0,-sign*18,0),new Vector3(0,sign*26,sign*16),contact));
                Add(other+"UpperArm",Gesture(t,Vector3.zero,new Vector3(variant==2?-58:-16,0,sign*15),contact));
                Add("Spine",Gesture(t,new Vector3(-3,-sign*12,0),new Vector3(9,sign*13,sign*3),contact));Add("Head",Gesture(t,new Vector3(-4,sign*4,0),new Vector3(5,-sign*4,0),contact));
            }
            else if(skill==1)
            {
                Add("RightUpperArm",Gesture(t,new Vector3(18,-16,-15),new Vector3(-87,12,-8),contact));Add("RightForearm",Gesture(t,new Vector3(-36,0,0),new Vector3(-3,0,0),contact));
                Add("LeftUpperArm",Gesture(t,Vector3.zero,new Vector3(18,-15,22),contact));Add("Spine",Gesture(t,new Vector3(-6,-22,0),new Vector3(14,19,-5),contact));bodyAngles.x+=Mathf.Sin(t*Mathf.PI)*5;
            }
            else if(skill==2)
            {
                Add("LeftUpperArm",Gesture(t,new Vector3(-25,20,12),new Vector3(-60,-18,-48),contact));Add("RightUpperArm",Gesture(t,new Vector3(-25,-20,-12),new Vector3(-60,18,48),contact));
                Add("LeftForearm",Gesture(t,new Vector3(-25,0,15),new Vector3(-32,0,-22),contact));Add("RightForearm",Gesture(t,new Vector3(-25,0,-15),new Vector3(-32,0,22),contact));
                Add("Spine",Gesture(t,new Vector3(8,0,0),new Vector3(-9,0,0),contact));Add("Head",new Vector3(-6*Mathf.Sin(t*Mathf.PI),0,0));
            }
            else if(skill==3)
            {
                Add("LeftUpperArm",Gesture(t,Vector3.zero,new Vector3(-35,15,22),contact));Add("RightUpperArm",Gesture(t,Vector3.zero,new Vector3(-35,-15,-22),contact));Add("Spine",Gesture(t,Vector3.zero,new Vector3(12,0,0),contact));
            }
        }
        void SwordPose(bool releasing,float slash)
        {
            float raise=charging?Ease(progress*5):1-Ease(slash);
            Add("LeftUpperArm",new Vector3(-125*raise,0,-18*raise));Add("RightUpperArm",new Vector3(-130*raise,0,18*raise));
            Add("LeftForearm",new Vector3(-12*raise,0,0));Add("RightForearm",new Vector3(-15*raise,0,0));Add("Spine",new Vector3(-10*raise,Mathf.Sin(Time.time*16)*progress*1.4f,0));Add("Head",new Vector3(-6*raise,0,0));
            Add("LeftThigh",new Vector3(7*raise,0,-7*raise));Add("RightThigh",new Vector3(-8*raise,0,7*raise));Add("LeftShin",new Vector3(12*raise,0,0));Add("RightShin",new Vector3(8*raise,0,0));
            if(releasing){float hit=Mathf.Sin(Ease(slash)*Mathf.PI);Add("Spine",new Vector3(hit*27,hit*13,0));Add("RightUpperArm",new Vector3(-95*hit,0,-10*hit));Add("LeftUpperArm",new Vector3(-85*hit,0,16*hit));bodyAngles.x+=hit*7;}
        }
        void ApplyJoints(float dt)
        {
            // Parent-first evaluation is essential for world-axis pose deltas.
            foreach(string name in JointNames)
            {
                Joint b;if(!bones.TryGetValue(name,out b))continue;b.transform.localPosition=b.position;b.transform.localScale=Vector3.one;
                if(charging && name.EndsWith("UpperArm"))b.transform.position+=visual.Body.up*(Ease(progress*5)*.08f);
                Quaternion parent=b.transform.parent.rotation,basis=visual.Body.rotation;
                Quaternion target=Quaternion.Inverse(parent)*basis*Quaternion.Euler(b.angles)*Quaternion.Inverse(basis)*parent*b.rest;
                b.smoothed=Quaternion.Slerp(b.smoothed,target,1-Mathf.Exp(-dt*24));b.transform.localRotation=b.smoothed;
            }
        }
        void SolveArm(string side,Vector3 point,Vector3 pole,float weight)
        {
            Joint upper,lower,hand;if(!bones.TryGetValue(side+"UpperArm",out upper) || !bones.TryGetValue(side+"Forearm",out lower) || !bones.TryGetValue(side+"Hand",out hand))return;
            Vector3 origin=upper.transform.position,delta=point-origin;float a=Vector3.Distance(origin,lower.transform.position),b=Vector3.Distance(lower.transform.position,hand.transform.position);
            float d=Mathf.Clamp(delta.magnitude,.02f,a+b-.001f);Vector3 direction=delta.normalized;
            Vector3 perpendicular=Vector3.ProjectOnPlane(pole-origin,direction).normalized;if(perpendicular.sqrMagnitude<.01f)perpendicular=visual.Body.right;
            float x=(a*a-b*b+d*d)/(2*d),height=Mathf.Sqrt(Mathf.Max(0,a*a-x*x));Vector3 elbow=origin+direction*x+perpendicular*height;
            Quaternion q=Quaternion.FromToRotation(lower.transform.position-origin,elbow-origin)*upper.transform.rotation;upper.transform.rotation=Quaternion.Slerp(upper.transform.rotation,q,weight);
            Vector3 target=origin+direction*d;q=Quaternion.FromToRotation(hand.transform.position-lower.transform.position,target-lower.transform.position)*lower.transform.rotation;lower.transform.rotation=Quaternion.Slerp(lower.transform.rotation,q,weight);
        }
        Vector3 HandMidpoint()
        {
            Joint a,b;if(bones.TryGetValue("LeftHand",out a) && bones.TryGetValue("RightHand",out b))return (a.transform.position+b.transform.position)*.5f+visual.Body.up*.035f;
            return visual.Body.TransformPoint(new Vector3(0,2,0));
        }
        void LateUpdate()
        {
            if(!initialized || visual==null || actor==null || actor.World.Paused || Time.deltaTime<=0)return;
            float dt=Time.deltaTime;foreach(var b in bones.Values)b.angles=Vector3.zero;
            if(!actor.Alive){visual.Body.localRotation=Quaternion.Euler(0,0,72);sword.gameObject.SetActive(false);chargeVfx.Visible(false);phaseVfx.Visible(false);return;}
            Locomotion(dt);Casting();float slash=Mathf.Clamp01((Time.time-releaseAt)/.42f);bool releasing=Time.time-releaseAt<.62f;if(charging || releasing)SwordPose(releasing,slash);
            if(phase){bodyOffset.y+=.10f+Mathf.Sin(Time.time*5)*.035f;bodyAngles.x+=8;}
            if(actor.Controlled && !charging){Add("Spine",new Vector3(9,0,0));Add("Head",new Vector3(8,0,0));}
            visual.Body.localPosition=bodyOffset+visual.PresentationRecoil;visual.Body.localRotation=visual.IsSlipping?Quaternion.Euler(0,0,72):Quaternion.Euler(bodyAngles);visual.Body.localScale=phase?new Vector3(.84f,1.02f,.84f):Vector3.one;ApplyJoints(dt);
            sword.gameObject.SetActive(charging || releasing);
            if(charging)
            {
                float weight=Ease(progress*5);SolveArm("Left",visual.Body.TransformPoint(new Vector3(-.07f,2.05f,-.20f)),visual.Body.TransformPoint(new Vector3(-.8f,1.65f,.3f)),weight);
                SolveArm("Right",visual.Body.TransformPoint(new Vector3(.07f,2.05f,-.20f)),visual.Body.TransformPoint(new Vector3(.8f,1.65f,.3f)),weight);
            }
            if(charging || releasing){sword.position=HandMidpoint();sword.localRotation=Quaternion.Euler(charging?-26+Mathf.Sin(Time.time*75)*progress*1.6f:Mathf.Lerp(-26,115,Ease(slash)),0,charging?Mathf.Sin(Time.time*83)*progress*1.6f:0);}
            chargeVfx.Visible(charging);phaseVfx.Visible(phase);if(charging){chargeVfx.Tip=sword.localPosition+sword.localRotation*Vector3.up*1.6f;chargeVfx.Progress=progress;}
            if(phase && Time.time>=nextGhost){nextGhost=Time.time+.12f;actor.World.Fx.Ghost(visual);}
        }
    }
}
