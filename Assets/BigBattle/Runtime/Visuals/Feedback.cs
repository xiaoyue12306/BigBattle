using System.Collections.Generic;
using UnityEngine;

namespace BigBattle
{
    public sealed class FloatingNumber
    {
        public Vector3 Position; public string Text; public float Born; public Color Color;
    }

    public sealed class Feedback : MonoBehaviour
    {
        public readonly List<FloatingNumber> Numbers = new List<FloatingNumber>();
        public string NoticeText { get; private set; }
        public float NoticeUntil { get; private set; }
        public void Notice(string message) { NoticeText=message;NoticeUntil=Time.unscaledTime+2.5f; }
        public void Circle(Vector3 position,float radius,Color color,float life)
        {
            var ring=Art.Ring("Combat pulse",transform,1,.055f,color);
            ring.transform.position=position;ring.transform.localScale=Vector3.one*radius;
            ring.AddComponent<TransientVisual>().Setup(life,1.15f);
        }
        public void Slash(Vector3 position,Vector3 direction,Color color)
        {
            // A swept arc, not a full disk, communicates facing-based melee range.
            for(int i=-2;i<=2;i++)
            {
                Vector3 point=position+(Quaternion.Euler(0,i*18,0)*direction)*1.15f+Vector3.up*.65f;
                var spark=Art.Shape("Blade arc",PrimitiveType.Sphere,transform,point,new Vector3(.20f,.09f,.42f),color);
                spark.transform.rotation=Quaternion.LookRotation(direction);spark.AddComponent<TransientVisual>().Setup(.14f,.15f);
            }
        }
        public void Hit(Vector3 position,float damage,bool friendly)
        {
            Numbers.Add(new FloatingNumber {Position=position,Text=Mathf.RoundToInt(damage).ToString(),Born=Time.time,Color=friendly?new Color(.99f,.64f,.10f):Art.Coral});
            Burst(position,friendly?Art.Cream:Art.Coral,4);
        }
        public void Burst(Vector3 position,Color color,int count)
        {
            for(int i=0;i<count;i++)
            {
                Vector3 direction=Quaternion.Euler(0,i*360f/count,0)*Vector3.forward;
                var dot=Art.Shape("Impact sparkle",PrimitiveType.Sphere,transform,position+direction*.12f,Vector3.one*.12f,color);
                dot.AddComponent<TransientVisual>().Setup(.32f,.01f,direction*2+Vector3.up*1.5f);
            }
        }
        void Update(){Numbers.RemoveAll(item=>Time.time-item.Born>.75f);}
        public void Clear(){Numbers.Clear();foreach(Transform child in transform)Destroy(child.gameObject);}
    }

    public sealed class TransientVisual : MonoBehaviour
    {
        float born,life,endScale;Vector3 initial,velocity;
        public void Setup(float duration,float scale,Vector3 movement=default(Vector3)){born=Time.time;life=duration;endScale=scale;initial=transform.localScale;velocity=movement;}
        void Update(){float t=(Time.time-born)/life;if(t>=1){Destroy(gameObject);return;}transform.localScale=initial*Mathf.Lerp(1,endScale,t);transform.position+=velocity*Time.deltaTime;}
    }

    public enum SoundCue { Slash,Thrust,Toss,Dodge,Ultimate,Hurt,Pop,Slip,Win }
    public sealed class GameSound : MonoBehaviour
    {
        public bool Muted;
        AudioSource source; readonly List<AudioClip> clips=new List<AudioClip>();
        public void Setup()
        {
            source=gameObject.AddComponent<AudioSource>();source.spatialBlend=0;source.volume=.17f;
            float[] frequencies={510,290,740,620,220,150,900,390,660};
            for(int index=0;index<frequencies.Length;index++)
            {
                int length=index==8?13230:5292;var samples=new float[length];
                for(int i=0;i<length;i++)
                {
                    float t=i/44100f, envelope=Mathf.Sin(Mathf.PI*i/length)*Mathf.Exp(-t*16);
                    float frequency=frequencies[index]*(1+.8f*t*(index%2==0?1:-1));
                    samples[i]=(Mathf.Sin(t*frequency*Mathf.PI*2)+.18f*Mathf.Sin(t*frequency*Mathf.PI*4))*envelope*.7f;
                }
                var clip=AudioClip.Create("Original synth "+(SoundCue)index,length,1,44100,false);clip.SetData(samples,0);clips.Add(clip);
            }
        }
        public void Play(SoundCue cue){if(!Muted && source!=null && clips.Count>0)source.PlayOneShot(clips[(int)cue]);}
        void OnDestroy(){foreach(var clip in clips)if(clip!=null)Destroy(clip);}
    }

    public sealed class ArenaCamera : MonoBehaviour
    {
        public Camera Camera {get;private set;}
        GameRoot world;Vector3 focus;float shake;
        public void Setup(GameRoot game)
        {
            world=game;Camera=gameObject.AddComponent<Camera>();Camera.orthographic=true;Camera.orthographicSize=9.8f;
            Camera.backgroundColor=new Color(.63f,.84f,.89f);Camera.clearFlags=CameraClearFlags.SolidColor;Camera.nearClipPlane=.1f;Camera.farClipPlane=100;
            Camera.allowHDR=false;Camera.allowMSAA=true;gameObject.AddComponent<AudioListener>();
            Snap();
        }
        public void Snap(){focus=Vector3.zero;Place();}
        public void Shake(float amount){shake=Mathf.Max(shake,amount);}
        void LateUpdate()
        {
            if(world.Player==null)return;
            Vector3 target=world.Player.transform.position*.24f;
            focus=Vector3.Lerp(focus,target,1-Mathf.Exp(-Time.unscaledDeltaTime*3));
            // Wider floor coverage on narrower displays, including future viewports.
            Camera.orthographicSize=Mathf.Max(9.8f,15.4f/Mathf.Max(.7f,Camera.aspect));Place();
            shake=Mathf.MoveTowards(shake,0,Time.unscaledDeltaTime*.9f);
        }
        void Place()
        {
            Vector3 jitter=world!=null && !world.Paused?new Vector3(Mathf.Sin(Time.unscaledTime*95),Mathf.Sin(Time.unscaledTime*71),0)*shake:Vector3.zero;
            transform.position=focus+new Vector3(0,18,-14)+jitter;transform.rotation=Quaternion.LookRotation(focus-transform.position);
        }
    }
}
