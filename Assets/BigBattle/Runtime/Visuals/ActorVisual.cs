using UnityEngine;

namespace BigBattle
{
    public sealed class ActorVisual : MonoBehaviour
    {
        public Transform Body, LeftHand, RightHand, Torso, LeftFoot, RightFoot;
        public Renderer[] Renderers;
        public bool UsesMixamo => library!=null && library.Tracks!=null && library.Tracks.Length>=13;
        public int MotionCount => library!=null && library.Tracks!=null?library.Tracks.Length:0;
        public MotionId CurrentMotion {get;private set;}
        MixamoMotionLibrary library;
        Transform leftArm,rightArm,leftShin,rightShin;
        TrailRenderer leftTrail,rightTrail;
        MotionPose rest;
        float smoothedMove,locomotionClock,actionStart=-100,actionDuration=.4f,recoilStart=-100;
        Vector3 recoil;
        bool defeated;
        readonly MaterialPropertyBlock block = new MaterialPropertyBlock();
        float flashUntil, swingUntil, moveAmount, slipUntil;
        bool spinning;
        float spinAngle;

        public static ActorVisual Create(Transform parent, int style)
        {
            var root = new GameObject("Animated visual"); root.transform.SetParent(parent, false);
            var visual = root.AddComponent<ActorVisual>();
            visual.Body = new GameObject("Body rig").transform; visual.Body.SetParent(root.transform, false);
            if (style == 0) {visual.BuildBanana();visual.library=Resources.Load<MixamoMotionLibrary>("LocalMixamo/Library");}
            else if (style == 1) visual.BuildMochi(); else visual.BuildDummy();
            visual.Renderers = root.GetComponentsInChildren<Renderer>(); return visual;
        }

        void BuildBanana()
        {
            Vector3[] curve = { new Vector3(-.30f,.47f,0), new Vector3(-.13f,.63f,0), new Vector3(.04f,.93f,0), new Vector3(.16f,1.27f,0), new Vector3(.13f,1.60f,0), new Vector3(.02f,1.89f,0), new Vector3(-.16f,2.08f,0) };
            Art.MeshObject("Banana golden skin", Body, Art.CurvedTube(curve, new[] {.075f,.22f,.31f,.33f,.29f,.18f,.065f}, 16), Art.Yellow);
            Art.Shape("Stem", PrimitiveType.Capsule, Body, new Vector3(-.18f,2.09f,0), new Vector3(.14f,.10f,.14f), new Color(.38f,.46f,.16f)).transform.localRotation = Quaternion.Euler(0,0,30);
            Art.Shape("Tip", PrimitiveType.Sphere, Body, new Vector3(-.31f,.46f,0), Vector3.one * .14f, new Color(.42f,.28f,.17f));
            Eye(Body, new Vector3(-.015f,1.46f,.286f), .095f);
            Eye(Body, new Vector3(.29f,1.46f,.25f), .095f);
            Art.Shape("Cheek L", PrimitiveType.Sphere, Body, new Vector3(-.11f,1.33f,.28f), new Vector3(.13f,.07f,.025f), new Color(1,.48f,.38f));
            Art.Shape("Cheek R", PrimitiveType.Sphere, Body, new Vector3(.35f,1.33f,.22f), new Vector3(.10f,.06f,.025f), new Color(1,.48f,.38f));
            Art.Shape("Smile", PrimitiveType.Capsule, Body, new Vector3(.14f,1.29f,.32f), new Vector3(.09f,.035f,.025f), Art.Ink).transform.localRotation = Quaternion.Euler(0,0,90);
            Art.Shape("Mint scarf", PrimitiveType.Capsule, Body, new Vector3(.11f,1.05f,0), new Vector3(.66f,.095f,.63f), Art.Mint);
            Art.Shape("Scarf tail", PrimitiveType.Cube, Body, new Vector3(.40f,1.01f,-.10f), new Vector3(.38f,.10f,.16f), Art.Mint).transform.localRotation = Quaternion.Euler(0,15,-15);
            LeftHand = Hand(-1); RightHand = Hand(1);
            for (int i = -1; i <= 1; i += 2)
            {
                var foot=new GameObject(i<0?"Left foot joint":"Right foot joint").transform;foot.SetParent(Body,false);
                foot.localPosition=new Vector3(i*.23f,.20f,.06f);
                Art.Shape("Little boot", PrimitiveType.Capsule, foot,Vector3.zero, new Vector3(.25f,.18f,.38f), Art.Ink);
                if(i<0)LeftFoot=foot;else RightFoot=foot;
            }
            Torso=new GameObject("Banana torso joint").transform;Torso.SetParent(Body,false);Torso.localPosition=Vector3.up*.95f;
            for(int i=Body.childCount-1;i>=0;i--){var child=Body.GetChild(i);if(child!=Torso && child!=LeftFoot && child!=RightFoot)child.SetParent(Torso,true);}
            leftArm=Art.Shape("Left rubber arm",PrimitiveType.Capsule,Torso,Vector3.zero,Vector3.one,Art.Yellow).transform;
            rightArm=Art.Shape("Right rubber arm",PrimitiveType.Capsule,Torso,Vector3.zero,Vector3.one,Art.Yellow).transform;
            leftShin=Art.Shape("Left short leg",PrimitiveType.Capsule,Body,Vector3.zero,Vector3.one,Art.Ink).transform;
            rightShin=Art.Shape("Right short leg",PrimitiveType.Capsule,Body,Vector3.zero,Vector3.one,Art.Ink).transform;
            rest=new MotionPose {LeftHand=new Vector3(-.49f,.91f,.03f),RightHand=new Vector3(.49f,.91f,.03f),LeftFoot=LeftFoot.localPosition,RightFoot=RightFoot.localPosition,LeftGrip=Quaternion.identity,RightGrip=Quaternion.identity};
            leftTrail=MakeTrail(LeftHand,-1);rightTrail=MakeTrail(RightHand,1);
        }

        Transform Hand(int side)
        {
            var pivot = new GameObject("Peel blade hand").transform; pivot.SetParent(Body, false); pivot.localPosition = new Vector3(side*.49f,.91f,.03f);
            Art.Shape("Mitten", PrimitiveType.Sphere, pivot, Vector3.zero, Vector3.one*.23f, Art.Cream);
            Vector3[] centers = { Vector3.zero, new Vector3(side*.12f,.22f,.04f), new Vector3(side*.16f,.48f,.13f), new Vector3(side*.12f,.71f,.26f) };
            Art.MeshObject("Curved peel blade", pivot, Art.CurvedTube(centers, new[] {.09f,.10f,.065f,.005f}, 6), Art.Cream);
            return pivot;
        }

        void BuildMochi()
        {
            Art.Shape("Mochi body", PrimitiveType.Sphere, Body, new Vector3(0,.65f,0), new Vector3(1.0f,1.05f,.9f), new Color(1,.58f,.62f));
            for (int side = -1; side <= 1; side += 2)
            {
                Art.Shape("Tiny foot", PrimitiveType.Sphere, Body, new Vector3(side*.29f,.15f,.12f), new Vector3(.28f,.22f,.35f), new Color(.67f,.26f,.35f));
                Art.Shape("Leaf", PrimitiveType.Capsule, Body, new Vector3(side*.18f,1.26f,0), new Vector3(.19f,.32f,.08f), Art.Mint).transform.localRotation = Quaternion.Euler(0,0,side*-40);
                Eye(Body, new Vector3(side*.18f,.77f,.42f), .09f);
            }
            Art.Shape("Mouth", PrimitiveType.Sphere, Body, new Vector3(0,.59f,.445f), new Vector3(.10f,.08f,.025f), Art.Ink);
        }

        void BuildDummy()
        {
            Art.Shape("Post", PrimitiveType.Cylinder, Body, new Vector3(0,.62f,0), new Vector3(.18f,.60f,.18f), new Color(.55f,.34f,.19f));
            Art.Shape("Straw body", PrimitiveType.Capsule, Body, new Vector3(0,1.02f,0), new Vector3(.80f,.55f,.70f), new Color(.88f,.65f,.32f));
            Art.Shape("Crossbar", PrimitiveType.Capsule, Body, new Vector3(0,.99f,0), new Vector3(.17f,.74f,.17f), new Color(.55f,.34f,.19f)).transform.localRotation = Quaternion.Euler(0,0,90);
            Art.Shape("Hat", PrimitiveType.Cylinder, Body, new Vector3(0,1.59f,0), new Vector3(1.10f,.06f,.90f), Art.Cream);
            Eye(Body, new Vector3(-.17f,1.24f,.32f), .07f); Eye(Body, new Vector3(.17f,1.24f,.32f), .07f);
            Art.Ring("Dummy target", transform, .67f, .065f, new Color(.89f,.67f,.33f));
        }

        static void Eye(Transform parent, Vector3 position, float size)
        {
            Art.Shape("Eye", PrimitiveType.Sphere, parent, position, new Vector3(size,size*1.35f,size*.50f), Art.Ink);
            Art.Shape("Eye glint", PrimitiveType.Sphere, parent, position + new Vector3(-size*.15f,size*.25f,size*.26f), Vector3.one*size*.25f, Color.white);
        }

        public void Animate(float movement, bool spin) { moveAmount = movement; spinning = spin; }
        public void SetAccent(Color color)
        {
            foreach(var renderer in Renderers)
                if(renderer.name=="Mint scarf" || renderer.name=="Scarf tail")renderer.sharedMaterial=Art.Mat("Scarf"+ColorUtility.ToHtmlStringRGB(color),color);
        }
        public void Swing() { swingUntil = Time.time + .18f; }
        public void Flash() { flashUntil = Time.time + .10f; }
        public void Slip(float duration) { slipUntil = Time.time + duration; }
        public void PlayMotion(MotionId motion,float duration)
        {
            CurrentMotion=motion;actionStart=Time.time;actionDuration=Mathf.Max(.1f,duration);
        }
        public void React(Vector3 direction,float strength)
        {
            recoil=transform.InverseTransformDirection(direction)*Mathf.Min(.18f,strength*.12f);recoilStart=Time.time;
            if(!spinning)PlayMotion(MotionId.Hurt,.22f);
        }
        public void SetDefeated(){defeated=true;PlayMotion(MotionId.Death,.6f);}
        public void ResetState()
        {
            defeated=false;actionStart=recoilStart=flashUntil=slipUntil=swingUntil=-100;spinAngle=0;Body.localScale=Vector3.one;
            if(leftTrail!=null){leftTrail.Clear();rightTrail.Clear();}
        }
        TrailRenderer MakeTrail(Transform hand,int side)
        {
            var tip=new GameObject("Peel blade ribbon");tip.transform.SetParent(hand,false);tip.transform.localPosition=new Vector3(side*.12f,.70f,.26f);
            var trail=tip.AddComponent<TrailRenderer>();trail.sharedMaterial=Art.FxMat("Blade trail",Color.white);
            trail.time=.16f;trail.minVertexDistance=.025f;trail.widthMultiplier=.22f;
            trail.widthCurve=AnimationCurve.Linear(0,1,1,0);trail.startColor=new Color(1,.96f,.60f,.9f);trail.endColor=new Color(.35f,.90f,.75f,0);
            trail.numCornerVertices=3;trail.emitting=false;trail.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;trail.receiveShadows=false;return trail;
        }
        MotionPose LocomotionPose(float speed)
        {
            MotionPose pose=rest;
            float cycle=locomotionClock*Mathf.PI*2;
            pose.LeftFoot+=new Vector3(0,Mathf.Max(0,Mathf.Sin(cycle))*.16f,Mathf.Cos(cycle)*.27f)*speed;
            pose.RightFoot+=new Vector3(0,Mathf.Max(0,-Mathf.Sin(cycle))*.16f,-Mathf.Cos(cycle)*.27f)*speed;
            pose.LeftHand.z+=Mathf.Sin(cycle)*.13f*speed;pose.RightHand.z-=Mathf.Sin(cycle)*.13f*speed;
            if(!UsesMixamo)return pose;
            var idle=library.Find(MotionId.Idle);var walk=library.Find(MotionId.Walk);var run=library.Find(MotionId.Run);
            var moving=MotionPose.Blend(walk.Sample(locomotionClock),run.Sample(locomotionClock),Mathf.Clamp01((speed-.4f)/.6f));
            return MotionPose.Blend(idle.Sample(Time.time/idle.Duration),moving,speed);
        }
        MotionPose ActionPose(float t)
        {
            MotionPose pose=rest;
            if(UsesMixamo)
            {
                var track=library.Find(CurrentMotion);
                if(track!=null)
                {
                    float normalized=t<.27f?t/.27f*track.Contact:Mathf.Lerp(track.Contact,1,(t-.27f)/.73f);
                    return track.Sample(normalized);
                }
            }
            float swing=Mathf.Sin(Mathf.Clamp01((t-.10f)/.65f)*Mathf.PI);
            float sign=CurrentMotion==MotionId.Attack2?-1:1;
            pose.Torso=new Vector3(swing*14,sign*swing*35,-sign*swing*6);
            pose.RightHand+=new Vector3(-sign*swing*.20f,swing*.28f,swing*.5f);
            pose.LeftHand+=new Vector3(sign*swing*.15f,swing*.1f,swing*.25f);
            pose.RightGrip=Quaternion.Euler(swing*95,sign*swing*35,0);return pose;
        }
        static void Arm(Transform arm,Vector3 shoulder,Vector3 hand)
        {
            arm.localPosition=(shoulder+hand)*.5f;arm.localRotation=Quaternion.FromToRotation(Vector3.up,hand-shoulder);
            arm.localScale=new Vector3(.14f,Vector3.Distance(hand,shoulder)*.5f,.14f);
        }
        void LateUpdate()
        {
            if(Torso!=null){AnimateBanana();return;}
            float bounce = Mathf.Sin(Time.time * (moveAmount > .1f ? 14 : 3)) * (moveAmount > .1f ? .065f : .025f);
            Body.localPosition = new Vector3(0,bounce,0);
            float recoilWeight=Mathf.Clamp01(1-(Time.time-recoilStart)/.22f);
            Body.localPosition+=recoil*recoilWeight;
            Body.localScale=new Vector3(1+recoilWeight*.10f,1-recoilWeight*.14f,1+recoilWeight*.10f);
            float lean = moveAmount * 8;
            if (spinning) spinAngle += Time.deltaTime * 1100; else spinAngle = Mathf.LerpAngle(spinAngle,0,Time.deltaTime*18);
            Body.localRotation = Quaternion.Euler(lean,spinAngle,Mathf.Sin(Time.time*10)*moveAmount*4);
            Body.localRotation*=Quaternion.Euler(recoil.z*recoilWeight*60,0,-recoil.x*recoilWeight*60);
            if(Time.time<slipUntil)
            {
                Body.localRotation=Quaternion.Euler(0,0,72);
                Body.localPosition=new Vector3(0,-.20f+Mathf.Sin(Time.time*18)*.025f,0);
            }
            float swing = Mathf.Clamp01((swingUntil-Time.time)/.18f);
            if (LeftHand != null) LeftHand.localRotation = Quaternion.Euler(-swing*100,0,-swing*25);
            if (RightHand != null) RightHand.localRotation = Quaternion.Euler(-swing*125,0,swing*25);
            block.SetColor("_EmissionColor", Time.time < flashUntil ? new Color(.55f,.5f,.4f) : Color.black);
            // A color multiplier flash preserves each original material.
            foreach (Renderer item in Renderers)
            {
                if (item == null) continue;
                block.SetColor("_Color", Time.time < flashUntil ? Color.white : item.sharedMaterial.color);
                item.SetPropertyBlock(block);
            }
        }
        void AnimateBanana()
        {
            float dt=Time.deltaTime;
            smoothedMove=Mathf.MoveTowards(smoothedMove,moveAmount,dt*7);locomotionClock+=dt*Mathf.Lerp(.8f,2.4f,smoothedMove);
            var pose=LocomotionPose(smoothedMove);
            float t=(Time.time-actionStart)/actionDuration;
            bool action=t>=0 && t<1;
            if(action)
            {
                float weight=Mathf.SmoothStep(0,1,Mathf.Min(t/.12f,(1-t)/.22f));
                pose=MotionPose.Blend(pose,ActionPose(t),weight);
            }
            float hurt=Mathf.Clamp01(1-(Time.time-recoilStart)/.22f);
            Body.localPosition=Vector3.Lerp(Body.localPosition,Vector3.up*(pose.Height+Mathf.Sin(locomotionClock*Mathf.PI*4)*.025f*smoothedMove)+recoil*hurt,1-Mathf.Exp(-dt*25));
            Vector3 squash=new Vector3(1+hurt*.10f,1-hurt*.12f,1+hurt*.06f);
            if(defeated)squash*=Mathf.Lerp(1,.02f,Mathf.SmoothStep(0,1,t));
            Body.localScale=Vector3.Lerp(Body.localScale,squash,1-Mathf.Exp(-dt*28));
            if(spinning)spinAngle+=dt*1050;else spinAngle=Mathf.LerpAngle(spinAngle,0,1-Mathf.Exp(-dt*20));
            Body.localRotation=Quaternion.Euler(smoothedMove*7,spinAngle,-recoil.x*hurt*75);
            Torso.localRotation=Quaternion.Slerp(Torso.localRotation,Quaternion.Euler(pose.Torso),1-Mathf.Exp(-dt*26));
            LeftHand.localPosition=Vector3.Lerp(LeftHand.localPosition,pose.LeftHand-Vector3.up*.95f,1-Mathf.Exp(-dt*32));
            RightHand.localPosition=Vector3.Lerp(RightHand.localPosition,pose.RightHand-Vector3.up*.95f,1-Mathf.Exp(-dt*32));
            LeftHand.localRotation=Quaternion.Slerp(LeftHand.localRotation,pose.LeftGrip,1-Mathf.Exp(-dt*28));
            RightHand.localRotation=Quaternion.Slerp(RightHand.localRotation,pose.RightGrip,1-Mathf.Exp(-dt*28));
            LeftFoot.localPosition=Vector3.Lerp(LeftFoot.localPosition,pose.LeftFoot,1-Mathf.Exp(-dt*30));
            RightFoot.localPosition=Vector3.Lerp(RightFoot.localPosition,pose.RightFoot,1-Mathf.Exp(-dt*30));
            LeftFoot.localRotation=Quaternion.Euler(Mathf.Clamp((pose.LeftFoot.z-.06f)*-75,-25,25),0,0);
            RightFoot.localRotation=Quaternion.Euler(Mathf.Clamp((pose.RightFoot.z-.06f)*-75,-25,25),0,0);
            Arm(leftArm,new Vector3(-.28f,.15f,0),LeftHand.localPosition);Arm(rightArm,new Vector3(.34f,.15f,0),RightHand.localPosition);
            Arm(leftShin,new Vector3(-.17f,.60f,0),LeftFoot.localPosition+Vector3.up*.07f);
            Arm(rightShin,new Vector3(.20f,.60f,0),RightFoot.localPosition+Vector3.up*.07f);
            leftTrail.emitting=rightTrail.emitting=!defeated && (spinning || (action && t>.12f && t<.72f && CurrentMotion>=MotionId.Attack1 && CurrentMotion<=MotionId.Thrust));
            foreach(Renderer item in Renderers)
            {
                if(item==null || item is TrailRenderer)continue;
                block.SetColor("_Color",Time.time<flashUntil?Color.white:item.sharedMaterial.color);item.SetPropertyBlock(block);
            }
        }
    }
}
