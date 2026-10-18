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
        public int ImpactCount {get;private set;}
        public int SlashCount {get;private set;}
        public int ClashCount {get;private set;}
        public float DoomFlashUntil;
        public int ActiveEffects=>transform.childCount;
        public void Notice(string message) { NoticeText=message;NoticeUntil=Time.unscaledTime+2.5f; }
        public void Circle(Vector3 position,float radius,Color color,float life)
        {
            if(transform.childCount>=EffectLimit)return;
            var ring=Art.Ring("Combat pulse",transform,1,.055f,color);
            ring.GetComponent<Renderer>().sharedMaterial=Art.FxMat("Pulse"+ColorUtility.ToHtmlStringRGBA(color),color);
            ring.transform.position=position;ring.transform.localScale=Vector3.one*radius;
            ring.AddComponent<TransientVisual>().Setup(life,1.15f);
        }
        public int FinisherCount {get;private set;}
        public int ThrustCount {get;private set;}
        public int WhirlwindCount {get;private set;}
        public int PeelBloomCount {get;private set;}
        public const int EffectLimit=220;
        static readonly Color Jade=new Color(.06f,.86f,.64f,.96f);
        static readonly Color Azure=new Color(.14f,.65f,1,.94f);
        static readonly Color Gold=new Color(1,.66f,.045f,.97f);
        static readonly Color Hot=new Color(1,.99f,.83f,.98f);
        static readonly Quaternion Billboard=Quaternion.LookRotation(new Vector3(0,18,-14));
        GameObject Moon(Vector3 position,Quaternion rotation,float radius,float sweep,float width,Color tint,float life,float expansion=1.12f,float turn=0)
        {
            if(transform.childCount>=EffectLimit)return null;
            var go=Art.Crescent(transform,tint,sweep,width);go.transform.SetPositionAndRotation(position,rotation);go.transform.localScale=Vector3.one*radius;
            go.AddComponent<TransientVisual>().Setup(life,expansion,Vector3.zero,new Vector3(0,turn,0));return go;
        }
        void Spark(Vector3 position,Color tint,float size,Vector3 velocity,float life,bool leaf=false)
        {
            if(transform.childCount>=EffectLimit)return;
            var go=Art.Glint(transform,tint,leaf);go.transform.SetPositionAndRotation(position,Billboard);go.transform.localScale=Vector3.one*size;
            go.AddComponent<TransientVisual>().Setup(life,.04f,velocity,new Vector3(0,0,leaf?220:100));
        }
        public void Slash(Vector3 position,Vector3 direction,Color color,int combo=0)
        {
            SlashCount++;bool hostile=color==Art.Coral;
            // Cosmetic playback starts at the same resolved weapon contact as the blade/audio.
            SkillSequence.Burst(GetComponentInParent<GameRoot>(),combo==2?"BananaFinisher":"BananaSlash",position,direction);
            Color accent=hostile?Art.Coral:combo==2?Gold:combo==1?Azure:Jade;
            Quaternion facing=Quaternion.LookRotation(direction);
            Quaternion plane=facing*Quaternion.Euler(combo==2?14:combo==1?-12:8,combo==1?24:-24,0);
            float radius=combo==2?2.13f:1.88f,life=combo==2?.34f:.26f,sign=combo==1?-1:1;
            // A dark fringe keeps the luminous blade readable against the pale garden.
            Moon(position+Vector3.up*.69f,plane,radius*1.035f,combo==2?215:170,.37f,new Color(.06f,.36f,.27f,.30f),life,1.08f,sign*70);
            Moon(position+Vector3.up*.73f,plane,radius,combo==2?215:170,.30f,accent,life,1.12f,sign*70);
            Moon(position+Vector3.up*.79f,plane,radius*.97f,combo==2?205:155,.07f,Hot,life*.76f,1.13f,sign*70);
            Moon(position+Vector3.up*.48f,facing*Quaternion.Euler(-12,combo==1?-48:48,0),radius*.80f,125,.20f,hostile?Art.Cream:combo==2?Jade:Gold,life*.9f,1.08f,-sign*110);
            for(int i=0;i<(combo==2?14:9);i++)
            {
                Vector3 radial=Quaternion.AngleAxis(-75+i*150f/(combo==2?13:8),Vector3.up)*direction;
                Vector3 tangent=Vector3.Cross(Vector3.up,radial)*sign;
                Spark(position+radial*(radius*.88f)+Vector3.up*(.65f+i%3*.12f),i%3==0?Hot:accent,i%3==0?.105f:.065f,tangent*2+radial*.65f+Vector3.up*.35f,.30f+i%3*.04f,i%3!=0);
            }
            if(combo==2)
            {
                FinisherCount++;
                Moon(position+Vector3.up*.94f,facing*Quaternion.Euler(55,-28,0),2.02f,180,.25f,Gold,.30f,1.15f,-80);
                Star(position+direction*1.65f+Vector3.up*.85f,Hot,.62f);
                Moon(position+Vector3.up*.05f,Quaternion.identity,.75f,360,.10f,new Color(1,.75f,.12f,.55f),.32f,2.5f);
                for(int i=0;i<5;i++)
                {
                    Vector3 fan=Quaternion.AngleAxis(-55+i*27.5f,Vector3.up)*direction;
                    Spark(position+fan*1.35f+Vector3.up*.9f,Gold,.13f,fan*2.5f+Vector3.up*.7f,.40f,true);
                }
            }
        }
        void Star(Vector3 position,Color color,float size)
        {
            Spark(position,new Color(.05f,.55f,.4f,.45f),size*1.2f,Vector3.zero,.16f);
            Spark(position,color,size,Vector3.zero,.14f);
            Spark(position,Color.white,size*.45f,Vector3.zero,.09f);
        }
        public void Thrust(Vector3 from,Vector3 to,Vector3 direction)
        {
            ThrustCount++;Vector3 delta=to-from;float distance=Mathf.Max(.5f,delta.magnitude);Quaternion rotation=Quaternion.LookRotation(direction);
            for(int layer=0;layer<2;layer++)
            {
                if(transform.childCount>=EffectLimit)break;
                var stream=Art.Lance(transform,layer==0?Jade:Hot);stream.transform.SetPositionAndRotation(from+Vector3.up*(.76f+layer*.03f),rotation);
                stream.transform.localScale=new Vector3(layer==0?1.7f:.4f,1,distance);stream.AddComponent<TransientVisual>().Setup(.25f,1.08f);
            }
            for(int i=0;i<7;i++)Spark(Vector3.Lerp(from,to,i/6f)+Vector3.up*.8f,i%2==0?Gold:Jade,.09f,-direction*1.2f+Vector3.up*.2f,.30f,true);
            Moon(to+Vector3.up*.72f,rotation*Quaternion.Euler(0,90,70),.68f,280,.21f,Gold,.24f,1.6f,80);
            Star(to+Vector3.up*.8f,Hot,.45f);
        }
        public void Whirlwind(Transform actor,float duration)
        {
            WhirlwindCount++;if(transform.childCount>=EffectLimit-12)return;
            var root=new GameObject("Banana golden leaf vortex");root.transform.SetParent(transform,false);root.transform.position=actor.position;
            root.AddComponent<BladeVortex>().Setup(actor,duration);
            Moon(actor.position+Vector3.up*.045f,Quaternion.identity,1.2f,360,.08f,Jade,.35f,1.8f);
        }
        public void WhirlBeat(Vector3 position)
        {
            float a=Time.time*430;
            Moon(position+Vector3.up*.7f,Quaternion.Euler(12,a,0),2.2f,130,.19f,Gold,.20f,1.06f,300);
            for(int i=0;i<3;i++)
            {
                Vector3 radial=Quaternion.Euler(0,a+i*120,0)*Vector3.forward;
                Spark(position+radial*2.1f+Vector3.up*.6f,i==0?Hot:Jade,.11f,radial*.6f+Vector3.up,.28f,true);
            }
        }
        public void PeelBloom(Vector3 position)
        {
            PeelBloomCount++;Circle(position,.82f,Jade,.28f);
            for(int i=0;i<6;i++)
            {
                Vector3 d=Quaternion.Euler(0,i*60,0)*Vector3.forward;
                Spark(position+d*.4f+Vector3.up*.25f,i%2==0?Gold:Jade,.12f,d*.8f+Vector3.up*.6f,.38f,true);
            }
        }
        public void Clash(Vector3 position)
        {
            ClashCount++;
            SkillSequence.Burst(GetComponentInParent<GameRoot>(),"SteelClash",position,Vector3.forward);
            Numbers.Add(new FloatingNumber{Position=position+Vector3.up*.35f,Text="拼刀！",Born=Time.time,Color=Art.Cream});
            if(Numbers.Count>32)Numbers.RemoveAt(0);
            Star(position,Hot,.95f);Burst(position,Gold,14);Burst(position,Hot,8);
            Moon(position,Quaternion.Euler(48,0,0),.65f,360,.10f,Gold,.25f,2.3f);
            Circle(new Vector3(position.x,.03f,position.z),1.1f,Jade,.30f);
        }
        public void Hit(Vector3 position,float damage,bool friendly)
        {
            ImpactCount++;
            Numbers.Add(new FloatingNumber {Position=position,Text=Mathf.RoundToInt(damage).ToString(),Born=Time.time,Color=friendly?new Color(.99f,.64f,.10f):Art.Coral});
            if(Numbers.Count>32)Numbers.RemoveAt(0);
            bool heavy=damage>=30;Color tint=friendly?Art.Yellow:Art.Coral;
            Star(position,Art.Cream,heavy?.85f:.45f);
            Burst(position,tint,heavy?9:5);
            if(transform.childCount>=EffectLimit)return;
            var star=Art.Shape("Contact flash",PrimitiveType.Sphere,transform,position,new Vector3(heavy?.40f:.26f,.07f,heavy?.40f:.26f),Art.Cream);
            star.GetComponent<Renderer>().sharedMaterial=Art.FxMat("White contact",new Color(1,1,.88f,.95f));
            star.AddComponent<TransientVisual>().Setup(.10f,1.4f);
            Circle(new Vector3(position.x,.015f,position.z),heavy?.8f:.48f,tint,.20f);
        }
        public void Burst(Vector3 position,Color color,int count)
        {
            for(int i=0;i<count;i++)
            {
                Vector3 direction=Quaternion.Euler(0,i*360f/count,0)*Vector3.forward;
                Spark(position+direction*.10f,i%4==0?Hot:color,i%4==0?.13f:.075f,direction*(2.6f+i%3*.55f)+Vector3.up*(.5f+i%4*.3f),.26f+i%3*.025f,i%4!=0);
            }
        }
        public void Ghost(ActorVisual source,bool death=false)
        {
            if(transform.childCount>180)return;
            var ghost=Instantiate(source.gameObject,transform);ghost.name=death?"Defeat tumble":"Dodge afterimage";
            ghost.transform.SetPositionAndRotation(source.transform.position,source.transform.rotation);
            ghost.GetComponent<ActorVisual>().enabled=false;
            foreach(var trail in ghost.GetComponentsInChildren<TrailRenderer>()){trail.Clear();trail.emitting=false;trail.enabled=false;}
            // An afterimage is a frozen silhouette, never a second live emitter.
            foreach(var fx in ghost.GetComponentsInChildren<SoulVfx>())fx.gameObject.SetActive(false);
            foreach(var ps in ghost.GetComponentsInChildren<ParticleSystem>())ps.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
            if(!death)foreach(var renderer in ghost.GetComponentsInChildren<Renderer>())renderer.sharedMaterial=Art.FxMat("Mint afterimage",new Color(.43f,.95f,.80f,.30f));
            ghost.AddComponent<TransientVisual>().Setup(death?.36f:.18f,death?.05f:.9f,death?new Vector3(.2f,2,0):Vector3.zero,death?new Vector3(0,200,180):Vector3.zero,death);
        }
        public void Dust(Vector3 position)
        {
            if(transform.childCount>180)return;
            var dust=Art.Shape("Foot dust",PrimitiveType.Sphere,transform,position+Vector3.up*.08f,new Vector3(.20f,.07f,.20f),Art.Cream);
            dust.GetComponent<Renderer>().sharedMaterial=Art.FxMat("Soft dust",new Color(1,.96f,.73f,.22f));
            dust.AddComponent<TransientVisual>().Setup(.24f,1.8f,Vector3.up*.12f);
        }
        void Update(){Numbers.RemoveAll(item=>Time.time-item.Born>.75f);}
        public void Clear(){Numbers.Clear();foreach(Transform child in transform){child.gameObject.SetActive(false);Destroy(child.gameObject);}}
    }

    public sealed class TransientVisual : MonoBehaviour
    {
        float born,life,endScale;Vector3 initial,velocity,spin;bool gravity;
        Renderer[] renderers;Color[] colors;readonly MaterialPropertyBlock block=new MaterialPropertyBlock();
        public void Setup(float duration,float scale,Vector3 movement=default(Vector3),Vector3 angular=default(Vector3),bool falling=false)
        {
            born=Time.time;life=duration;endScale=scale;initial=transform.localScale;velocity=movement;spin=angular;gravity=falling;
            renderers=GetComponentsInChildren<Renderer>();colors=new Color[renderers.Length];
            for(int i=0;i<colors.Length;i++)colors[i]=renderers[i].sharedMaterial.color;
        }
        void Update()
        {
            float t=(Time.time-born)/life;if(t>=1){Destroy(gameObject);return;}
            transform.localScale=initial*Mathf.Lerp(1,endScale,t);transform.position+=velocity*Time.deltaTime;transform.Rotate(spin*Time.deltaTime);
            if(gravity)velocity+=Vector3.down*9*Time.deltaTime;
            for(int i=0;i<renderers.Length;i++)
            {
                if(renderers[i]==null)continue;Color color=colors[i];color.a*=1-t*t;block.SetColor("_Color",color);block.SetFloat("_Progress",Mathf.Clamp01(t));renderers[i].SetPropertyBlock(block);
            }
        }
    }

    public sealed class ArenaCamera : MonoBehaviour
    {
        public Camera Camera {get;private set;}
        GameRoot world;Vector3 focus,kick;float shake,punch,doomAt=-100;
        public void Setup(GameRoot game)
        {
            world=game;Camera=gameObject.AddComponent<Camera>();Camera.orthographic=true;Camera.orthographicSize=9.8f;
            Camera.backgroundColor=new Color(.63f,.84f,.89f);Camera.clearFlags=CameraClearFlags.SolidColor;Camera.nearClipPlane=.1f;Camera.farClipPlane=100;
            Camera.allowHDR=true;Camera.allowMSAA=true;gameObject.AddComponent<SoulBloom>();gameObject.AddComponent<AudioListener>();
            Snap();
        }
        public bool MapOverview;
        public bool CaptureCloseup;
        public bool CapturePortrait;
        public float CaptureSize=6.8f;
        public bool CaptureDoom;
        Vector3 Target()
        {
            if(CapturePortrait)return world.Player.transform.position+Vector3.up*1.15f;
            if(CaptureDoom)return world.Player.transform.position+world.Player.Facing*5;
            if(CaptureCloseup)return world.Player.transform.position;
            if(world!=null && world.IsLane){Vector3 lane=FlowerLaneMap.ToLane(world.Player.transform.position);return FlowerLaneMap.ToWorld(new Vector3(lane.x*.45f,0,Mathf.Clamp(lane.z+3,-28,28)));}
            return world.Player.transform.position*.24f;
        }
        public void Snap(){focus=MapOverview?Vector3.zero:world!=null && (world.IsLane || CaptureCloseup || CaptureDoom)?Target():Vector3.zero;kick=Vector3.zero;shake=punch=0;doomAt=-100;Place();}
        public void DoomImpact(){doomAt=Time.time;Shake(1.05f);}
        public void Shake(float amount){shake=Mathf.Max(shake,amount);}
        public void Impact(Vector3 direction,float strength)
        {
            kick=Vector3.ClampMagnitude(kick-direction.normalized*strength*.06f,.14f);punch=Mathf.Max(punch,strength*.11f);Shake(strength*.055f);
        }
        void LateUpdate()
        {
            if(world.Player==null)return;
            Vector3 target=MapOverview?Vector3.zero:Target();
            focus=Vector3.Lerp(focus,target,1-Mathf.Exp(-Time.unscaledDeltaTime*3));
            // Wider floor coverage on narrower displays, including future viewports.
            float doomAge=Time.time-doomAt,expansion=doomAge<1.4f?Mathf.Sin(Mathf.Clamp01(doomAge/1.4f)*Mathf.PI)*1.8f:0;
            Camera.orthographicSize=(MapOverview?38:CaptureDoom?11.8f:CaptureCloseup?CaptureSize:Mathf.Max(9.8f,15.4f/Mathf.Max(.7f,Camera.aspect))-punch)+expansion;Place();
            shake=Mathf.MoveTowards(shake,0,Time.unscaledDeltaTime*.9f);
            kick=Vector3.Lerp(kick,Vector3.zero,1-Mathf.Exp(-Time.unscaledDeltaTime*22));punch=Mathf.MoveTowards(punch,0,Time.unscaledDeltaTime*1.6f);
        }
        void Place()
        {
            Vector3 jitter=world!=null && !world.Paused?new Vector3(Mathf.Sin(Time.unscaledTime*67),Mathf.Sin(Time.unscaledTime*47),0)*shake+kick:Vector3.zero;
            Camera.farClipPlane=MapOverview?220:100;RenderSettings.fog=!MapOverview;
            transform.position=focus+(MapOverview?new Vector3(0,65,-50.55f):CapturePortrait?new Vector3(0,8,-16):new Vector3(0,18,-14))+jitter;transform.rotation=Quaternion.LookRotation(focus-transform.position);
        }
    }
}
