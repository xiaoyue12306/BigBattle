using System.Collections.Generic;
using UnityEngine;

namespace BigBattle
{
    [DefaultExecutionOrder(20)]
    public sealed class WitchVisual : MonoBehaviour
    {
        ActorVisual visual;Combatant actor;Transform model;Animation player;AnimationState idle,walk,greeting;Vector3 previous;float age,stride,weight;
        public bool HasModel=>model!=null;
        public int BoneCount {get;private set;}
        public int ClipCount {get;private set;}
        public float SampleClock=>age;
        public float WalkWeight=>weight;
        public float StridePhase=>stride;
        public static void Build(ActorVisual visual)
        {
            var fx=visual.gameObject.AddComponent<WitchVisual>();fx.visual=visual;fx.actor=visual.GetComponentInParent<Combatant>();visual.ExternalBodyPose=true;
            var asset=Resources.Load<GameObject>("Witch/Witch");if(asset==null){Debug.LogError("Prepared Witch FBX is missing");return;}
            fx.model=Instantiate(asset,visual.Body,false).transform;fx.model.name="Witch • user model and authored FBX clips";
            var material=Art.Mat("Witch supplied original texture",Color.white);material.mainTexture=Resources.Load<Texture2D>("Witch/WitchColor");material.SetFloat("_Glossiness",.12f);
            foreach(var r in fx.model.GetComponentsInChildren<Renderer>())r.sharedMaterial=material;
            var skin=fx.model.GetComponentInChildren<SkinnedMeshRenderer>();fx.BoneCount=skin!=null?skin.bones.Length:0;
            fx.player=fx.model.GetComponent<Animation>();if(fx.player==null)fx.player=fx.model.gameObject.AddComponent<Animation>();fx.player.playAutomatically=false;fx.player.enabled=false;
            foreach(var clip in Resources.LoadAll<AnimationClip>("Witch/Witch"))
            {
                if(clip.name.StartsWith("__preview__"))continue;
                string name=clip.name.Contains("Greeting")?"Greeting":clip.name.Contains("Idle")?"Idle":clip.name.Contains("Walk")?"Walk":null;
                if(name==null)continue;fx.player.AddClip(clip,name);fx.ClipCount++;
            }
            fx.idle=fx.player["Idle"];fx.walk=fx.player["Walk"];fx.greeting=fx.player["Greeting"];
            if(fx.idle==null || fx.walk==null || fx.greeting==null){Debug.LogError("Witch authored Idle/Walk/Greeting clips are missing");return;}
            foreach(AnimationState state in fx.player){state.enabled=true;state.wrapMode=WrapMode.Loop;state.weight=0;}
            fx.ResetPose();
        }
        public void ResetPose(){age=stride=weight=0;if(actor!=null)previous=actor.transform.position;Sample();}
        void LateUpdate()
        {
            if(actor==null || actor.World==null || actor.World.Paused || !HasModel || idle==null)return;
            Vector3 delta=actor.transform.position-previous;previous=actor.transform.position;delta.y=0;float distance=delta.magnitude;if(distance>1.5f)distance=0;
            float speed=Time.deltaTime>.00001f?distance/Time.deltaTime:0;weight=Mathf.MoveTowards(weight,Mathf.Clamp01(speed/3.5f),Time.deltaTime*5);
            age+=Time.deltaTime;stride+=distance/.95f;Sample();
            visual.Body.localPosition=visual.PresentationRecoil;visual.Body.localRotation=Quaternion.Euler(weight*2,0,0);
        }
        void Sample()
        {
            if(idle==null)return;float hello=Mathf.Clamp01((2-age)/.4f)*(1-weight);
            idle.time=Mathf.Repeat(age,idle.length);idle.weight=(1-weight)*(1-hello);
            walk.time=Mathf.Repeat(stride,1)*walk.length;walk.weight=weight;
            greeting.time=Mathf.Min(age,greeting.length-.0001f);greeting.weight=hello;
            player.Sample();
        }
    }
}
