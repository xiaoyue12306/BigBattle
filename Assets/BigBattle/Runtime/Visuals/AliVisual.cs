using UnityEngine;

namespace BigBattle
{
    [DefaultExecutionOrder(20)]
    public sealed class AliVisual : MonoBehaviour
    {
        ActorVisual visual;Combatant actor;Animation sampler;AnimationState idle,walk,place,defeat;Transform model;Vector3 previous;float clock,stride,weight,placeAt=-100,deathAt=-1;
        public int ClipCount {get;private set;}
        public int BoneCount {get;private set;}
        public float SampleClock=>clock;
        public float WalkWeight=>weight;
        public static void Build(ActorVisual body)
        {
            var rig=body.gameObject.AddComponent<AliVisual>();rig.visual=body;rig.actor=body.GetComponentInParent<Combatant>();body.ExternalBodyPose=true;
            var asset=Resources.Load<GameObject>("Ali/Ali");if(asset==null){Debug.LogError("Missing prepared Ali FBX");return;}
            rig.model=Instantiate(asset,body.Body,false).transform;
            var material=Art.Mat("Ali original color",Color.white);material.mainTexture=Resources.Load<Texture2D>("Ali/AliColor");material.SetFloat("_Glossiness",.12f);
            foreach(var renderer in rig.model.GetComponentsInChildren<Renderer>())renderer.sharedMaterial=material;
            var skin=rig.model.GetComponentInChildren<SkinnedMeshRenderer>();rig.BoneCount=skin!=null?skin.bones.Length:0;
            rig.sampler=rig.model.GetComponent<Animation>();if(rig.sampler==null)rig.sampler=rig.model.gameObject.AddComponent<Animation>();rig.sampler.enabled=false;rig.sampler.playAutomatically=false;
            foreach(var clip in Resources.LoadAll<AnimationClip>("Ali/Ali"))
            {
                if(clip.name.StartsWith("__preview__"))continue;string name=clip.name.Contains("Idle")?"Idle":clip.name.Contains("Walk")?"Walk":clip.name.Contains("Place")?"Place":clip.name.Contains("Defeat")?"Defeat":null;
                if(name!=null){rig.sampler.AddClip(clip,name);rig.ClipCount++;}
            }
            rig.idle=rig.sampler["Idle"];rig.walk=rig.sampler["Walk"];rig.place=rig.sampler["Place"];rig.defeat=rig.sampler["Defeat"];
            foreach(AnimationState state in rig.sampler){state.enabled=true;state.wrapMode=WrapMode.ClampForever;state.weight=0;}
            if(rig.ClipCount!=4)Debug.LogError("Ali requires four authored clips");rig.ResetPose();
        }
        public void ResetPose(){clock=stride=weight=0;deathAt=-1;placeAt=-100;previous=actor.transform.position;Sample();}
        public void PlayPlace(){placeAt=clock;}
        void LateUpdate()
        {
            if(actor==null || actor.World==null || actor.World.Paused || idle==null)return;
            float distance=Vector3.Distance(actor.transform.position,previous);previous=actor.transform.position;if(distance>1.5f)distance=0;
            weight=Mathf.MoveTowards(weight,Mathf.Clamp01(distance/Mathf.Max(.0001f,Time.deltaTime)/4),Time.deltaTime*6);stride+=distance/1.05f;clock+=Time.deltaTime;
            if(!actor.Alive && deathAt<0)deathAt=clock;Sample();visual.Body.localPosition=visual.PresentationRecoil;
        }
        void Sample()
        {
            if(idle==null)return;float action=Mathf.Clamp01(1-(clock-placeAt)/.42f),dead=deathAt>=0?1:0;
            idle.time=Mathf.Repeat(clock,idle.length);idle.weight=(1-weight)*(1-action)*(1-dead);
            walk.time=Mathf.Repeat(stride,1)*walk.length;walk.weight=weight*(1-action)*(1-dead);
            place.time=Mathf.Clamp(clock-placeAt,0,place.length);place.weight=action*(1-dead);
            defeat.time=deathAt>=0?Mathf.Min(clock-deathAt,defeat.length):0;defeat.weight=dead;sampler.Sample();
        }
    }
}
