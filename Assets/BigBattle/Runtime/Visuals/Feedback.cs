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
        public int ActiveEffects=>transform.childCount;
        public void Notice(string message) { NoticeText=message;NoticeUntil=Time.unscaledTime+2.5f; }
        public void Circle(Vector3 position,float radius,Color color,float life)
        {
            if(transform.childCount>=220)return;
            var ring=Art.Ring("Combat pulse",transform,1,.055f,color);
            ring.GetComponent<Renderer>().sharedMaterial=Art.FxMat("Pulse"+ColorUtility.ToHtmlStringRGBA(color),color);
            ring.transform.position=position;ring.transform.localScale=Vector3.one*radius;
            ring.AddComponent<TransientVisual>().Setup(life,1.15f);
        }
        public void Slash(Vector3 position,Vector3 direction,Color color,int combo=0)
        {
            SlashCount++;
            Color accent=combo==2?Art.Yellow:combo==1?new Color(.44f,.83f,1,.82f):color;
            for(int layer=0;layer<(combo==2?4:3);layer++)
            {
                if(transform.childCount>=200)break;
                Color tint=layer==1?new Color(1,1,.91f,.96f):accent;
                if(layer==2)tint.a=.24f;
                var arc=Art.Arc(transform,tint,combo==2?215:combo==1?175:150);
                arc.transform.position=position+Vector3.up*(.66f+layer*.055f);
                arc.transform.rotation=Quaternion.LookRotation(direction)*Quaternion.Euler(combo==2?26:combo==1?-18:10,combo==1?22:-22,combo==1?-12:12);
                arc.transform.localScale=Vector3.one*(1.48f+combo*.16f+layer*.12f);
                arc.AddComponent<TransientVisual>().Setup(combo==2?.29f:.20f,combo==2?1.32f:1.16f,Vector3.zero,new Vector3(0,combo==1?-140:140,0));
            }
            int motes=combo==2?9:5;
            for(int i=0;i<motes;i++)
            {
                if(transform.childCount>=210)break;
                Vector3 radial=Quaternion.AngleAxis(-65+i*130f/(motes-1),Vector3.up)*direction;
                Vector3 tangent=Vector3.Cross(Vector3.up,radial)*(combo==1?-1:1);
                var spark=Art.Shape("Blade stardust",PrimitiveType.Cube,transform,position+radial*(1.3f+combo*.14f)+Vector3.up*.75f,new Vector3(.035f,.035f,.20f),accent);
                spark.GetComponent<Renderer>().sharedMaterial=Art.FxMat("Blade dust"+ColorUtility.ToHtmlStringRGBA(accent),accent);
                spark.transform.rotation=Quaternion.LookRotation(tangent);
                spark.AddComponent<TransientVisual>().Setup(.22f+i*.012f,.02f,tangent*(1.4f+i*.17f)+Vector3.up*.5f);
            }
            if(combo==2){Star(position+direction*1.45f+Vector3.up*.8f,Art.Cream,.65f);Circle(position,.9f,Art.Yellow,.28f);}
        }
        void Star(Vector3 position,Color color,float size)
        {
            for(int ray=0;ray<4;ray++)
            {
                if(transform.childCount>=220)break;
                var flash=Art.Shape("Sharp contact star",PrimitiveType.Cube,transform,position,new Vector3(size,.025f,.035f),color);
                flash.GetComponent<Renderer>().sharedMaterial=Art.FxMat("Star"+ColorUtility.ToHtmlStringRGBA(color),color);
                flash.transform.rotation=Quaternion.Euler(-38,ray*45,0);
                flash.AddComponent<TransientVisual>().Setup(.14f,.05f);
            }
        }
        public void Clash(Vector3 position)
        {
            ClashCount++;
            Numbers.Add(new FloatingNumber{Position=position+Vector3.up*.35f,Text="拼刀！",Born=Time.time,Color=Art.Cream});
            if(Numbers.Count>32)Numbers.RemoveAt(0);
            Star(position,Art.Cream,1.25f);Burst(position,Art.Yellow,16);Burst(position,Art.Cream,9);
            Circle(position, .45f,Art.Cream,.18f);
            Circle(new Vector3(position.x,.03f,position.z),1.1f,Art.Mint,.30f);
        }
        public void Hit(Vector3 position,float damage,bool friendly)
        {
            ImpactCount++;
            Numbers.Add(new FloatingNumber {Position=position,Text=Mathf.RoundToInt(damage).ToString(),Born=Time.time,Color=friendly?new Color(.99f,.64f,.10f):Art.Coral});
            if(Numbers.Count>32)Numbers.RemoveAt(0);
            bool heavy=damage>=30;Color tint=friendly?Art.Yellow:Art.Coral;
            Star(position,Art.Cream,heavy?.85f:.45f);
            Burst(position,tint,heavy?9:5);
            var star=Art.Shape("Contact flash",PrimitiveType.Sphere,transform,position,new Vector3(heavy?.40f:.26f,.07f,heavy?.40f:.26f),Art.Cream);
            star.GetComponent<Renderer>().sharedMaterial=Art.FxMat("White contact",new Color(1,1,.88f,.95f));
            star.AddComponent<TransientVisual>().Setup(.10f,1.4f);
            Circle(new Vector3(position.x,.015f,position.z),heavy?.8f:.48f,tint,.20f);
        }
        public void Burst(Vector3 position,Color color,int count)
        {
            for(int i=0;i<count;i++)
            {
                if(transform.childCount>220)break;
                Vector3 direction=Quaternion.Euler(0,i*360f/count,0)*Vector3.forward;
                var dot=Art.Shape("Impact sparkle",PrimitiveType.Cube,transform,position+direction*.12f,new Vector3(.07f,.07f,.20f),color);
                dot.GetComponent<Renderer>().sharedMaterial=Art.FxMat("Shard"+ColorUtility.ToHtmlStringRGBA(color),color);
                dot.transform.rotation=Quaternion.LookRotation(direction);
                dot.AddComponent<TransientVisual>().Setup(.26f,.01f,direction*(2.4f+i%3*.5f)+Vector3.up*1.5f,new Vector3(120,80,0),true);
            }
        }
        public void Ghost(ActorVisual source,bool death=false)
        {
            if(transform.childCount>180)return;
            var ghost=Instantiate(source.gameObject,transform);ghost.name=death?"Defeat tumble":"Dodge afterimage";
            ghost.transform.SetPositionAndRotation(source.transform.position,source.transform.rotation);
            ghost.GetComponent<ActorVisual>().enabled=false;
            foreach(var trail in ghost.GetComponentsInChildren<TrailRenderer>()){trail.Clear();trail.emitting=false;trail.enabled=false;}
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
        public void Clear(){Numbers.Clear();foreach(Transform child in transform)Destroy(child.gameObject);}
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
                if(renderers[i]==null)continue;Color color=colors[i];color.a*=1-t*t;block.SetColor("_Color",color);renderers[i].SetPropertyBlock(block);
            }
        }
    }

    public sealed class ArenaCamera : MonoBehaviour
    {
        public Camera Camera {get;private set;}
        GameRoot world;Vector3 focus,kick;float shake,punch;
        public void Setup(GameRoot game)
        {
            world=game;Camera=gameObject.AddComponent<Camera>();Camera.orthographic=true;Camera.orthographicSize=9.8f;
            Camera.backgroundColor=new Color(.63f,.84f,.89f);Camera.clearFlags=CameraClearFlags.SolidColor;Camera.nearClipPlane=.1f;Camera.farClipPlane=100;
            Camera.allowHDR=false;Camera.allowMSAA=true;gameObject.AddComponent<AudioListener>();
            Snap();
        }
        public void Snap(){focus=Vector3.zero;kick=Vector3.zero;shake=punch=0;Place();}
        public void Shake(float amount){shake=Mathf.Max(shake,amount);}
        public void Impact(Vector3 direction,float strength)
        {
            kick=Vector3.ClampMagnitude(kick-direction.normalized*strength*.06f,.14f);punch=Mathf.Max(punch,strength*.11f);Shake(strength*.055f);
        }
        void LateUpdate()
        {
            if(world.Player==null)return;
            Vector3 target=world.Player.transform.position*.24f;
            focus=Vector3.Lerp(focus,target,1-Mathf.Exp(-Time.unscaledDeltaTime*3));
            // Wider floor coverage on narrower displays, including future viewports.
            Camera.orthographicSize=Mathf.Max(9.8f,15.4f/Mathf.Max(.7f,Camera.aspect))-punch;Place();
            shake=Mathf.MoveTowards(shake,0,Time.unscaledDeltaTime*.9f);
            kick=Vector3.Lerp(kick,Vector3.zero,1-Mathf.Exp(-Time.unscaledDeltaTime*22));punch=Mathf.MoveTowards(punch,0,Time.unscaledDeltaTime*1.6f);
        }
        void Place()
        {
            Vector3 jitter=world!=null && !world.Paused?new Vector3(Mathf.Sin(Time.unscaledTime*67),Mathf.Sin(Time.unscaledTime*47),0)*shake+kick:Vector3.zero;
            transform.position=focus+new Vector3(0,18,-14)+jitter;transform.rotation=Quaternion.LookRotation(focus-transform.position);
        }
    }
}
