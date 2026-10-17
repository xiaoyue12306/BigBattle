using System.Collections.Generic;
using UnityEngine;

namespace BigBattle
{
    // Presentation only. Fixed world-up volume, independent of aim, camera and damage sweep.
    public sealed class DoomWaveVisual : MonoBehaviour
    {
        GameRoot world;DoomRift rift;float born;Renderer blade;Transform halo;
        readonly MaterialPropertyBlock block=new MaterialPropertyBlock();
        readonly List<LineRenderer> lightning=new List<LineRenderer>();
        public void Setup(GameRoot game,Vector3 facing)
        {
            world=game;born=Time.time;
            var volume=Art.DoomSurface(transform,false);volume.transform.localPosition=Vector3.down*.92f;blade=volume.GetComponent<Renderer>();
            halo=Art.DoomSurface(transform,false).transform;halo.localPosition=Vector3.down*.92f;halo.localScale=new Vector3(1.22f,1.03f,1.16f);
            for(int i=0;i<4;i++){var line=GhostArt.Line(transform,"Forked apocalypse lightning",i==0?new Color(.4f,1,.86f):i==1?GhostArt.Red:i==2?GhostArt.Blue:GhostArt.Teal,i==0?.065f:.035f,32);lightning.Add(line);}
            SoulVfx.Attach(transform,world,SoulEffect.Doom);
            if(world.Fx.ActiveEffects<Feedback.EffectLimit){var go=new GameObject("Doom fractured ground");go.transform.SetParent(world.Fx.transform);go.transform.position=new Vector3(transform.position.x,.035f,transform.position.z);go.transform.rotation=Quaternion.LookRotation(facing);rift=go.AddComponent<DoomRift>();rift.Setup(world);}
        }
        public void Advance(Vector3 point){if(rift!=null)rift.Advance(point);}
        void LateUpdate()
        {
            if(world==null || world.Paused)return;float age=Time.time-born;
            block.Clear();block.SetFloat("_Clock",age);block.SetFloat("_Opacity",1);blade.SetPropertyBlock(block);
            block.SetFloat("_Opacity",.18f+.05f*Mathf.Sin(age*23));halo.GetComponent<Renderer>().SetPropertyBlock(block);
            for(int i=0;i<lightning.Count;i++)for(int j=0;j<32;j++)
            {
                float t=j/31f,a=i*Mathf.PI*.5f+Mathf.Sin(t*25+age*21)*.17f;
                float radius=Mathf.Sin(t*Mathf.PI)*(.58f+Mathf.Sin(t*71+age*33)*.18f);
                lightning[i].SetPosition(j,new Vector3(Mathf.Sin(a)*radius,t*9-.9f,Mathf.Cos(a)*radius+Mathf.Sin(t*Mathf.PI)*2-t*.8f));
            }
        }
    }

    public sealed class DoomRift : MonoBehaviour
    {
        sealed class Segment {public Transform root;public Renderer surface;public float born;}
        public const int SegmentLimit=192;
        public static int LiveSegments {get;private set;}
        public int SegmentCount=>segments.Count;
        readonly List<Segment> segments=new List<Segment>();readonly MaterialPropertyBlock block=new MaterialPropertyBlock(),flameBlock=new MaterialPropertyBlock();
        GameRoot world;float distance,last;int stamped;
        public void Setup(GameRoot game){world=game;last=Time.time;}
        public void Advance(Vector3 point)
        {
            float forward=transform.InverseTransformPoint(point).z;
            // Distance stamps overlap at every frame rate, even on a long swept movement step.
            while(distance<=forward && stamped<62)
            {
                Vector3 floorPoint=transform.TransformPoint(Vector3.forward*(distance+.75f));floorPoint.y=0;
                if(LiveSegments<SegmentLimit && (world.Clamp(floorPoint)-floorPoint).sqrMagnitude<.01f)
                {
                    var mark=Art.DoomSurface(transform,true,stamped%4);mark.transform.localPosition=Vector3.forward*distance;
                    var root=mark.transform;segments.Add(new Segment{root=root,surface=mark.GetComponent<Renderer>(),born=Time.time});LiveSegments++;
                    var scorch=Art.DoomSurface(root,true,4+stamped%4);scorch.name="Scorched split earth";scorch.transform.localPosition=Vector3.down*.012f;scorch.transform.localScale=new Vector3(1.9f,1,1);
                    if(stamped%4==0)for(int side=-1;side<=1;side+=2)
                    {
                        var rock=Art.Shape("Rift uprooted stone",PrimitiveType.Cube,root,new Vector3(side*.95f,.12f,.55f),new Vector3(.48f,.65f,.57f),new Color(.08f,.15f,.17f));rock.transform.localRotation=Quaternion.Euler(side*23,stamped*37,side*-27);
                        var jet=new GameObject("Ground soul flame");jet.transform.SetParent(root,false);jet.transform.localPosition=new Vector3(side*.85f,.65f,.6f);jet.AddComponent<MeshFilter>().sharedMesh=Art.SoulQuad();var renderer=jet.AddComponent<MeshRenderer>();renderer.sharedMaterial=Art.SoulMaterial("Flipbooks/ParaFire",true);renderer.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;renderer.receiveShadows=false;
                    }
                }
                stamped++;distance+=1.2f;last=Time.time;
            }
        }
        void LateUpdate()
        {
            if(world==null || world.Paused)return;
            for(int i=segments.Count-1;i>=0;i--)
            {
                Segment s=segments[i];float age=Time.time-s.born;
                if(age>=4.6f){Destroy(s.root.gameObject);segments.RemoveAt(i);LiveSegments--;continue;}
                block.Clear();block.SetFloat("_Clock",age);block.SetFloat("_Opacity",1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(2.6f,4.6f,age)));s.surface.SetPropertyBlock(block);
                // All child energy surfaces share the fade and paused scaled clock.
                for(int j=0;j<s.root.childCount;j++)
                {
                    var child=s.root.GetChild(j);var r=child.GetComponent<Renderer>();
                    if(r!=null && r.sharedMaterial.shader.name=="BigBattle/DoomEnergy")r.SetPropertyBlock(block);
                    else if(child.name=="Ground soul flame")
                    {
                        float frame=Mathf.Repeat(age*30+child.localPosition.x*11+64,64),fade=block.GetFloat("_Opacity");
                        flameBlock.Clear();flameBlock.SetColor("_Color",new Color(.025f,.86f,.65f,fade*.65f));flameBlock.SetFloat("_Clock",age);flameBlock.SetVector("_Sheet",new Vector4(8,8,Mathf.Floor(frame),(Mathf.Floor(frame)+1)%64));flameBlock.SetFloat("_FrameBlend",frame-Mathf.Floor(frame));r.SetPropertyBlock(flameBlock);
                        child.rotation=world.CameraRig.Camera.transform.rotation;child.localScale=new Vector3(1.1f,2.3f*(.85f+.15f*Mathf.Sin(age*13+child.localPosition.x)),1);
                    }
                    else if(child.name=="Rift uprooted stone"){Vector3 p=child.localPosition;p.y=.12f-Mathf.SmoothStep(0,.9f,Mathf.InverseLerp(2.6f,4.6f,age));child.localPosition=p;}
                }
            }
            if(Time.time-last>4.7f)Destroy(gameObject);
        }
        void OnDestroy(){LiveSegments-=segments.Count;segments.Clear();}
    }
}
