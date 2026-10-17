using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace BigBattle
{
    public enum SoulEffect { Skull,Chain,Cage,Summon,Aura,Phase,Charge,Doom,Impact,Release }

    // Kenney's CC0 artwork, layered for the native Built-in renderer. These are
    // visual emitters only: no damage, sounds, control, hit-pause or skill timers.
    [DefaultExecutionOrder(30)]
    public sealed class SoulVfx : MonoBehaviour
    {
        sealed class Card
        {
            public Transform transform;public Renderer renderer;public Color tint;
            public Vector3 position,scale;public float spin;public bool floor,billboard;
            public int frames=1,columns=1,rows=1,role,ordinal;public float fps=30;public bool loop=true;
        }
        readonly List<Card> cards=new List<Card>();readonly List<ParticleSystem> emitters=new List<ParticleSystem>();
        readonly MaterialPropertyBlock block=new MaterialPropertyBlock();
        GameRoot world;SoulEffect kind;float born,life;bool initialized;
        ParticleSystem gathering;readonly ParticleSystem.Particle[] gatheringParticles=new ParticleSystem.Particle[120];
        readonly List<LineRenderer> currents=new List<LineRenderer>(),currentEdges=new List<LineRenderer>();
        public float Progress;public Vector3 Tip=new Vector3(.18f,3.7f,-.85f);
        public SoulEffect Kind=>kind;
        public int EmitterCount=>emitters.Count;
        public int AnimatedLayers {get;private set;}
        public float AnimationFrame {get;private set;}
        public static readonly string[] SheetNames={"AuraRays","AuraFlames","ArcaneOrb","ArcaneVortex","ArcaneImpact","ArcaneLightning","ParaFire"};
        public static readonly string[] TextureNames={"circle_02","fire_01","flame_01","light_02","magic_01","magic_03","smoke_04","spark_01","spark_06","star_04","twirl_02","slash_02","trace_05"};
        static readonly Color Mint=new Color(.08f,1,.65f,.8f),Cyan=new Color(.05f,.58f,1,.8f),Ink=new Color(.015f,.075f,.10f,.65f),Hot=new Color(.65f,1,.91f,.9f),Red=new Color(1,.08f,.23f,.75f);
        public static SoulVfx Attach(Transform parent,GameRoot world,SoulEffect kind)
        {
            var go=new GameObject("Soul VFX • "+kind);go.transform.SetParent(parent,false);var fx=go.AddComponent<SoulVfx>();fx.Setup(world,kind);return fx;
        }
        public static SoulVfx Burst(GameRoot world,SoulEffect kind,Vector3 position,Vector3 facing=default(Vector3))
        {
            if(world.Fx.ActiveEffects>=Feedback.EffectLimit)return null;
            var fx=Attach(world.Fx.transform,world,kind);fx.transform.position=position;
            if(facing.sqrMagnitude>.01f)fx.transform.rotation=Quaternion.LookRotation(facing);return fx;
        }
        Card Layer(string texture,Color tint,Vector3 position,Vector3 scale,bool floor=false,float spin=0,bool glow=true,bool billboard=true)
        {
            var go=new GameObject(texture);go.transform.SetParent(transform,false);go.AddComponent<MeshFilter>().sharedMesh=Art.SoulQuad();var renderer=go.AddComponent<MeshRenderer>();renderer.sharedMaterial=Art.SoulMaterial(texture,glow);renderer.shadowCastingMode=ShadowCastingMode.Off;renderer.receiveShadows=false;
            var card=new Card{transform=go.transform,renderer=renderer,tint=tint,position=position,scale=scale,floor=floor,spin=spin,billboard=billboard};cards.Add(card);return card;
        }
        ParticleSystem Emitter(string texture,Color tint,float rate,float size,float lifetime,float speed,float radius,bool glow=true,bool worldSpace=true)
        {
            var go=new GameObject("Kenney particles • "+texture);go.transform.SetParent(transform,false);var ps=go.AddComponent<ParticleSystem>();ps.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
            var main=ps.main;main.playOnAwake=false;main.loop=true;main.duration=2;main.startLifetime=new ParticleSystem.MinMaxCurve(lifetime*.65f,lifetime);main.startSize=new ParticleSystem.MinMaxCurve(size*.5f,size);main.startColor=tint;main.startSpeed=new ParticleSystem.MinMaxCurve(speed*.4f,speed);main.maxParticles=kind==SoulEffect.Charge?120:64;main.simulationSpace=worldSpace?ParticleSystemSimulationSpace.World:ParticleSystemSimulationSpace.Local;main.cullingMode=ParticleSystemCullingMode.AlwaysSimulate;main.startRotation=new ParticleSystem.MinMaxCurve(-Mathf.PI,Mathf.PI);
            var emission=ps.emission;emission.rateOverTime=rate;
            var shape=ps.shape;shape.shapeType=ParticleSystemShapeType.Sphere;shape.radius=radius;shape.radiusThickness=1;
            var color=ps.colorOverLifetime;color.enabled=true;var gradient=new Gradient();gradient.SetKeys(new[]{new GradientColorKey(Color.white,0),new GradientColorKey(Color.white,1)},new[]{new GradientAlphaKey(0,0),new GradientAlphaKey(1,.12f),new GradientAlphaKey(.6f,.45f),new GradientAlphaKey(0,1)});color.color=gradient;
            var scale=ps.sizeOverLifetime;scale.enabled=true;scale.size=new ParticleSystem.MinMaxCurve(1,AnimationCurve.EaseInOut(0,.35f,1,1.35f));
            var rotation=ps.rotationOverLifetime;rotation.enabled=true;rotation.z=new ParticleSystem.MinMaxCurve(-.9f,.9f);
            var renderer=ps.GetComponent<ParticleSystemRenderer>();renderer.sharedMaterial=Art.SoulMaterial(texture,glow);renderer.renderMode=ParticleSystemRenderMode.Billboard;renderer.shadowCastingMode=ShadowCastingMode.Off;renderer.receiveShadows=false;renderer.maxParticleSize=1;
            emitters.Add(ps);ps.Play();return ps;
        }
        Card Sheet(string name,Color tint,Vector3 position,Vector3 scale,bool floor=false,float spin=0,bool once=false)
        {
            var c=Layer("Flipbooks/"+name,tint,position,scale,floor,spin);
            c.frames=name.StartsWith("Aura")?60:7;c.columns=c.frames==60?8:4;c.rows=c.frames==60?8:2;c.fps=c.frames==60?30:18;c.loop=!once;AnimatedLayers++;return c;
        }
        ParticleSystem FireFrames(Color tint,float rate,float size,float lifetime,float speed,float radius)
        {
            var ps=Emitter("Flipbooks/ParaFire",tint,rate,size,lifetime,speed,radius);var sheet=ps.textureSheetAnimation;
            sheet.enabled=true;sheet.numTilesX=8;sheet.numTilesY=8;sheet.frameOverTime=new ParticleSystem.MinMaxCurve(1,AnimationCurve.Linear(0,0,1,1));sheet.cycleCount=1;return ps;
        }
        void RingSet(float size)
        {
            Layer("light_02",new Color(.01f,.20f,.22f,.24f),Vector3.up*.035f,Vector3.one*size*1.18f,true,-34,false);
            Layer("magic_01",Mint,Vector3.up*.05f,Vector3.one*size,true,28,false);
            Layer("twirl_02",new Color(.02f,.53f,.74f,.55f),Vector3.up*.07f,Vector3.one*size*.9f,true,-95,false);
            Layer("circle_02",new Color(.04f,.8f,.58f,.75f),Vector3.up*.08f,Vector3.one*size*.76f,true,0,false);
        }
        void Setup(GameRoot game,SoulEffect effect)
        {
            world=game;kind=effect;born=Time.time;
            if(kind==SoulEffect.Skull)
            {
                Layer("light_02",Ink,Vector3.zero,Vector3.one*1.35f,false,-75,false);
                Sheet("ArcaneOrb",Mint,Vector3.zero,Vector3.one*1.25f,false,85);
                FireFrames(Mint,26,.85f,.55f,.45f,.15f);
                Emitter("smoke_04",new Color(.01f,.20f,.22f,.32f),13,.85f,.55f,.35f,.12f,false);
                Emitter("star_04",Hot,12,.17f,.4f,1.2f,.3f);
            }
            if(kind==SoulEffect.Chain)
            {
                Layer("magic_03",Hot,Vector3.zero,Vector3.one*.9f,false,160);
                Emitter("spark_01",Cyan,22,.65f,.24f,.35f,.15f);
                Emitter("fire_01",Mint,24,.6f,.28f,.7f,.15f);
                Sheet("ArcaneLightning",Cyan,Vector3.zero,Vector3.one*1.35f,false,130);
            }
            if(kind==SoulEffect.Cage)
            {
                RingSet(3.3f);Emitter("flame_01",new Color(.05f,.7f,.6f,.4f),24,.65f,.65f,.7f,1.05f);
                Sheet("AuraRays",Mint,Vector3.up*.09f,Vector3.one*3.5f,true,-30);
                for(int i=0;i<6;i++){float a=i*Mathf.PI/3;Sheet("ArcaneVortex",i%2==0?Cyan:Mint,new Vector3(Mathf.Sin(a)*1.4f,.5f+i%3*.55f,Mathf.Cos(a)*1.4f),Vector3.one*1.1f,false,i%2==0?60:-60);}
            }
            if(kind==SoulEffect.Summon || kind==SoulEffect.Aura || kind==SoulEffect.Phase)
            {
                bool burst=kind==SoulEffect.Summon;bool phase=kind==SoulEffect.Phase;
                RingSet(burst?3.2f:phase?1.9f:1.5f);
                Sheet(burst?"AuraFlames":"AuraRays",phase?Cyan:Mint,Vector3.up*.09f,Vector3.one*(burst?3.6f:phase?2.3f:1.65f),true,phase?-30:20);
                Emitter("flame_01",Mint,burst?44:phase?32:12,burst?1.1f:.6f,burst?.8f:.65f,burst?1.2f:.45f,burst?.8f:.4f);
                Emitter("smoke_04",new Color(.02f,.14f,.20f,phase?.22f:.13f),phase?15:8,1.1f,.75f,.3f,.5f,false);
                if(burst){life=1.1f;Layer("magic_03",Hot,Vector3.up*.8f,new Vector3(2,3,1));}
                if(phase)Layer("twirl_02",Cyan,Vector3.up*.85f,Vector3.one*1.8f,false,-150);
            }
            if(kind==SoulEffect.Charge)
            {
                RingSet(4.6f);
                Layer("light_02",Ink,Tip,Vector3.one*1.5f,false,-120,false).role=1;
                Layer("magic_03",Hot,Tip,Vector3.one*1.25f,false,35).role=1;
                Sheet("ArcaneOrb",Cyan,Tip,Vector3.one*1.9f,false,-80).role=1;
                for(int i=0;i<9;i++){var stream=Layer(i%3==0?"spark_06":"trace_05",i%3==0?Red:i%3==1?Cyan:Mint,Vector3.zero,new Vector3(.35f,2,1));stream.role=2;stream.ordinal=i;}
                Sheet("AuraRays",new Color(.10f,.7f,1,.55f),Vector3.up*.10f,Vector3.one*4.6f,true,-28);
                Sheet("AuraFlames",new Color(.02f,.8f,.5f,.75f),Vector3.up*.12f,Vector3.one*3.4f,true,42);
                for(int i=0;i<3;i++)
                {
                    currentEdges.Add(GhostArt.Line(transform,"Dark border of gathered soul",new Color(.01f,.05f,.09f,.55f),.13f,24));
                    currents.Add(GhostArt.Line(transform,"Red blue green soul current",i==0?Red:i==1?Cyan:Mint,.065f,24));
                    Color tint=i==0?Red:i==1?Cyan:Mint;var gradient=new Gradient();gradient.SetKeys(new[]{new GradientColorKey(tint,0),new GradientColorKey(tint,.8f),new GradientColorKey(Hot,1)},new[]{new GradientAlphaKey(0,0),new GradientAlphaKey(.9f,.22f),new GradientAlphaKey(1,.8f),new GradientAlphaKey(.3f,1)});
                    currents[i].sharedMaterial=Art.SoulMaterial("spark_06",true,true);currents[i].colorGradient=gradient;
                    currentEdges[i].sharedMaterial=Art.SoulMaterial("trace_05",false,true);currentEdges[i].startColor=currentEdges[i].endColor=new Color(.01f,.07f,.10f,.35f);currentEdges[i].widthMultiplier=1.2f;
                }
                gathering=Emitter("star_04",Hot,38,.19f,1.3f,0,3.5f);gathering.transform.localPosition=Tip;
                Emitter("flame_01",new Color(.03f,.65f,.52f,.28f),26,.95f,.8f,.7f,1.1f);
            }
            if(kind==SoulEffect.Doom)
            {
                Layer("slash_02",Ink,Vector3.zero,new Vector3(5.8f,3.2f,1),false,0,false,false);
                Layer("slash_02",Mint,Vector3.forward*.04f,new Vector3(5.5f,2.8f,1),false,0,true,false);
                Layer("slash_02",Cyan,Vector3.forward*.08f,new Vector3(5.1f,2.5f,1),false,0,true,false);
                Layer("slash_02",Red,Vector3.forward*.12f+Vector3.up*.22f,new Vector3(4.7f,2.3f,1),false,0,true,false);
                Layer("magic_03",Hot,Vector3.zero,Vector3.one*2.2f,false,90);
                FireFrames(Mint,48,1.8f,.65f,1.4f,1.15f);
                Emitter("smoke_04",Ink,35,1.7f,.65f,1.2f,.9f,false);
                Emitter("spark_01",Cyan,45,.7f,.28f,2.2f,1.2f);
                Sheet("ArcaneVortex",new Color(.03f,.55f,.8f,.6f),Vector3.zero,Vector3.one*3.6f,false,70);
            }
            if(kind==SoulEffect.Impact || kind==SoulEffect.Release)
            {
                bool release=kind==SoulEffect.Release;life=release?.8f:.45f;
                Layer("light_02",Ink,Vector3.zero,Vector3.one*(release?4:1.6f),false,0,false);
                Layer("magic_03",Hot,Vector3.zero,Vector3.one*(release?3.5f:1.25f),false,25);
                Layer("spark_01",Mint,Vector3.zero,Vector3.one*(release?4:1.9f),false,-60);
                Sheet(release?"ArcaneLightning":"ArcaneImpact",release?Cyan:Mint,Vector3.zero,Vector3.one*(release?4.1f:2.1f),false,0,true);
                if(release)RingSet(5.5f);
                var ps=Emitter("star_04",Hot,0,release?.26f:.17f,.5f,release?6:3,.15f);ps.Emit(release?42:18);
                ps=Emitter("fire_01",Mint,0,release?1.3f:.75f,.6f,release?3:1,.25f);ps.Emit(release?16:7);
            }
            initialized=true;Animate();
        }
        public void Visible(bool value)
        {
            if(gameObject.activeSelf==value)return;
            if(value)born=Time.time;gameObject.SetActive(value);
        }
        void OnEnable(){if(initialized)foreach(var ps in emitters)ps.Play();}
        public void SetPaused(bool value)
        {
            if(!initialized || !gameObject.activeInHierarchy)return;
            foreach(var ps in emitters)
            {
                // timeScale changes leave the current frame's native particle
                // step queued. Explicit pause also freezes that pending step.
                if(value && ps.isPlaying)ps.Pause();
                else if(!value && ps.isPaused)ps.Play();
            }
        }
        void OnDisable(){foreach(var ps in emitters)if(ps!=null)ps.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);}
        void LateUpdate()
        {
            if(!initialized || world==null || world.Paused)return;
            float age=Time.time-born;if(life>0 && age>=life){Destroy(gameObject);return;}Animate();
            if(gathering!=null)
            {
                gathering.transform.localPosition=Tip;Vector3 target=transform.TransformPoint(Tip);int count=gathering.GetParticles(gatheringParticles);
                for(int i=0;i<count;i++){Vector3 delta=target-gatheringParticles[i].position;gatheringParticles[i].velocity=delta.normalized*(4+Progress*7);if(delta.sqrMagnitude<.06f)gatheringParticles[i].remainingLifetime=0;}
                gathering.SetParticles(gatheringParticles,count);
            }
        }
        void Animate()
        {
            float age=Time.time-born,fade=life>0?1-Mathf.Clamp01(age/life):1;
            Quaternion view=world.CameraRig.Camera.transform.rotation;
            for(int i=0;i<currents.Count;i++)
            {
                float a=i*Mathf.PI*2/3+age*.9f;Vector3 radial=new Vector3(Mathf.Sin(a),0,Mathf.Cos(a));
                Vector3 from=radial*(3.4f+Progress*.5f)+Vector3.up*.2f,control=radial*3+Vector3.up*(2+Progress),bend=Tip+Vector3.Cross(Vector3.up,radial)*1.3f;
                for(int j=0;j<24;j++)
                {
                    float t=j/23f,u=1-t;Vector3 p=u*u*u*from+3*u*u*t*control+3*u*t*t*bend+t*t*t*Tip;
                    p+=Vector3.Cross(radial,Vector3.up)*Mathf.Sin(t*19-age*17)*.05f*Mathf.Sin(t*Mathf.PI);
                    currentEdges[i].SetPosition(j,p);currents[i].SetPosition(j,p+Vector3.up*.012f);
                }
                currents[i].widthMultiplier=.65f+Progress*.75f;
            }
            for(int i=0;i<cards.Count;i++)
            {
                Card c=cards[i];Vector3 p=c.position,scale=c.scale;float alpha=fade;
                if(kind==SoulEffect.Charge)
                {
                    if(c.role==0){scale*=.9f+Progress*.35f;alpha*=.5f+Progress*.5f;}
                    else if(c.role==1){p=Tip;scale*=.6f+Progress*.9f;}
                    else
                    {
                        int n=c.ordinal;float t=Mathf.Repeat(age*(.65f+Progress*.8f)+n*.618f,1),a=n*2.399f+age*.25f;
                        Vector3 from=new Vector3(Mathf.Sin(a)*4.1f,1.4f+Mathf.Cos(a)*1.2f,Mathf.Cos(a)*4.1f);
                        p=Vector3.Lerp(from,Tip,t*t);scale*=.65f+Progress*.5f;alpha*=Mathf.Sin(t*Mathf.PI);
                    }
                }
                if(kind==SoulEffect.Summon || kind==SoulEffect.Release || kind==SoulEffect.Impact)scale*=Mathf.Lerp(.45f,1.55f,1-fade);
                c.transform.localPosition=p;c.transform.localScale=scale;
                if(c.floor)c.transform.localRotation=Quaternion.Euler(90,age*c.spin,0);
                else if(c.billboard)c.transform.rotation=view*Quaternion.Euler(0,0,age*c.spin);
                else c.transform.localRotation=Quaternion.Euler(0,0,0);
                Color color=c.tint;color.a*=alpha;block.Clear();block.SetColor("_Color",color);block.SetFloat("_Clock",age);block.SetFloat("_Flow",kind==SoulEffect.Doom?.018f:kind==SoulEffect.Skull?.015f:0);c.renderer.SetPropertyBlock(block);
                if(c.frames>1)
                {
                    float frame=c.loop?Mathf.Repeat(age*c.fps,c.frames):Mathf.Min(age*c.fps,c.frames-1);float current=Mathf.Floor(frame),next=c.loop?(current+1)%c.frames:Mathf.Min(current+1,c.frames-1);
                    block.SetVector("_Sheet",new Vector4(c.columns,c.rows,current,next));block.SetFloat("_FrameBlend",frame-current);c.renderer.SetPropertyBlock(block);AnimationFrame=frame;
                }
            }
        }
    }
}
