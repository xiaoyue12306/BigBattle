using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace BigBattle
{
    // Real Timeline clips drive visual layers only. The manual director follows the combat clock.
    [DefaultExecutionOrder(25)]
    public sealed class SkillSequence : MonoBehaviour
    {
        sealed class Layer {public Transform root;public Renderer renderer;}
        readonly Dictionary<SkillVfxClip,Layer> layers=new Dictionary<SkillVfxClip,Layer>();
        MaterialPropertyBlock block;
        GameRoot world;SoulVfx soul;float born,age;bool initialized,counted;
        public PlayableDirector Director {get;private set;}
        public float Exposure {get;private set;}=1;
        public int VisibleLayers {get;private set;}
        public float SampleClock {get;private set;}
        public string SequenceName {get;private set;}
        public const int InstanceLimit=96;
        public static int LiveInstances {get;private set;}
        public static readonly string[] Names={"SoulCharge","SoulRelease","SoulCage","SoulSummon","SoulPhase","SoulImpact","SoulSkull","BananaSlash","BananaFinisher","SteelClash"};
        public static SkillSequence Attach(Transform parent,GameRoot world,string name)
        {
            if(LiveInstances>=InstanceLimit)return null;
            var asset=Resources.Load<TimelineAsset>("SkillTimelines/"+name);if(asset==null){Debug.LogError("Missing authored skill Timeline: "+name);return null;}
            var go=new GameObject("Timeline VFX • "+name);go.transform.SetParent(parent,false);var sequence=go.AddComponent<SkillSequence>();sequence.Setup(world,asset);return sequence;
        }
        public static SkillSequence Burst(GameRoot world,string name,Vector3 position,Vector3 facing)
        {
            if(world.Fx.ActiveEffects>=Feedback.EffectLimit)return null;
            var sequence=Attach(world.Fx.transform,world,name);if(sequence!=null){sequence.transform.position=position;if(facing.sqrMagnitude>.01f)sequence.transform.rotation=Quaternion.LookRotation(facing);}return sequence;
        }
        public void Setup(GameRoot game,TimelineAsset asset)
        {
            world=game;SequenceName=asset.name;soul=GetComponentInParent<SoulVfx>();born=Time.time;
            Director=gameObject.AddComponent<PlayableDirector>();Director.playOnAwake=false;Director.playableAsset=asset;Director.timeUpdateMode=DirectorUpdateMode.Manual;Director.extrapolationMode=DirectorWrapMode.Hold;
            foreach(var track in asset.GetOutputTracks())
            {
                Director.SetGenericBinding(track,this);
                foreach(var clip in track.GetClips())
                {
                    var data=clip.asset as SkillVfxClip;if(data==null || data.Shape==SequenceShape.Envelope)continue;
                    var go=Art.SequenceSurface(transform,data.Shape);go.name=clip.displayName;var renderer=go.GetComponent<Renderer>();renderer.enabled=false;layers.Add(data,new Layer{root=go.transform,renderer=renderer});
                }
            }
            initialized=true;counted=true;LiveInstances++;
            if(Application.isPlaying)Director.Play();else Director.RebuildGraph();Evaluate(0);
        }
        void OnEnable(){if(initialized){born=Time.time;Director.Play();Evaluate(0);}}
        void OnDisable(){if(Director!=null)Director.Stop();foreach(var layer in layers.Values)if(layer.renderer!=null)layer.renderer.enabled=false;}
        void OnDestroy(){if(counted){LiveInstances--;counted=false;}}
        void LateUpdate()
        {
            if(!Application.isPlaying || !initialized || world==null || world.Paused)return;
            age=Time.time-born;
            if(soul==null && age>Director.duration){Destroy(gameObject);return;}
            // Charge remains fully formed while the player holds a charged sword; release is gameplay-owned.
            double time=soul!=null && soul.Kind==SoulEffect.Charge?soul.Progress*4:age;
            Evaluate(System.Math.Min(time,System.Math.Max(0,Director.duration-.0001)));
        }
        void Evaluate(double time)
        {
            foreach(var layer in layers.Values)layer.renderer.enabled=false;
            VisibleLayers=0;Exposure=1;SampleClock=(float)time;Director.time=time;Director.Evaluate();
        }
        public void Sample(SkillVfxClip clip,float time,float weight)
        {
            float t=Mathf.Clamp01(time),alpha=Mathf.Max(0,clip.Envelope.Evaluate(t))*weight;
            if(clip.Shape==SequenceShape.Envelope){Exposure=alpha;return;}
            Layer node;if(!layers.TryGetValue(clip,out node))return;
            node.renderer.enabled=alpha>.001f;if(!node.renderer.enabled)return;VisibleLayers++;
            node.root.localPosition=clip.Offset+(clip.FollowSword && soul!=null?soul.Tip:Vector3.zero);
            node.root.localScale=Vector3.LerpUnclamped(clip.From,clip.To,clip.Expansion.Evaluate(t));
            float clock=Application.isPlaying?age:(float)Director.time,spin=clock*clip.Spin;
            if(clip.Billboard)node.root.rotation=(world!=null?world.CameraRig.Camera.transform.rotation:Quaternion.Euler(52,0,0))*Quaternion.Euler(0,0,spin);
            else node.root.localRotation=Quaternion.Euler(clip.Tilt+new Vector3(clip.Ground?90:0,spin,0));
            if(block==null)block=new MaterialPropertyBlock();
            block.Clear();
            Color color=clip.Tint;color.a*=alpha*clip.Opacity;
            block.SetColor("_Color",color);
            block.SetFloat("_Clock",clock);block.SetFloat("_Progress",t);
            block.SetFloat("_Style",clip.Shape==SequenceShape.Seal?1:clip.Shape==SequenceShape.Rays?2:clip.Shape==SequenceShape.Arc || clip.Shape==SequenceShape.Spiral?3:clip.Shape==SequenceShape.Shards?4:0);
            node.renderer.SetPropertyBlock(block);
        }
        public void Hide(SkillVfxClip clip){Layer node;if(layers.TryGetValue(clip,out node))node.renderer.enabled=false;}
    }
}
