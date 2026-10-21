using System.Collections.Generic;
using UnityEngine;

namespace BigBattle
{
    // Original geometry inspired by the frozen bridge: no extracted game assets.
    public sealed class AbyssMap : MonoBehaviour
    {
        static readonly Color Stone=new Color(.30f,.39f,.46f),Rim=new Color(.36f,.45f,.52f),Snow=new Color(.81f,.9f,.95f),Ice=new Color(.18f,.52f,.7f),Dark=new Color(.10f,.17f,.24f);
        GameObject Shape(string name,PrimitiveType type,Vector3 p,Vector3 scale,Color color)=>Art.Shape(name,type,transform,p,scale,color);
        public void Build()
        {
            transform.localRotation=FlowerLaneMap.Orientation;
            var sun=new GameObject("Cold northern daylight").AddComponent<Light>();sun.transform.SetParent(transform,false);sun.transform.rotation=Quaternion.Euler(52,-28,0);sun.type=LightType.Directional;sun.color=new Color(.77f,.88f,1);sun.intensity=1.15f;sun.shadows=LightShadows.Soft;sun.shadowStrength=.45f;
            Shape("Bottomless blue abyss",PrimitiveType.Cube,new Vector3(0,-20,0),new Vector3(220,.2f,220),new Color(.055f,.12f,.2f));
            Shape("Ancient bridge foundation",PrimitiveType.Cube,new Vector3(0,-1.1f,0),new Vector3(15,2,90),Dark);
            // Individually chipped tessellated slabs, combined into material batches below.
            var rng=new System.Random(13739);
            for(int z=-43;z<=43;z+=2)for(int x=-5;x<=5;x+=2)
            {
                var slab=Shape("Chipped basalt paving",PrimitiveType.Cube,new Vector3(x+(z%4==1?.3f:0),-.06f,z),new Vector3(1.92f,.14f,1.92f),((x+z)%3==0)?Rim:Stone);slab.transform.localRotation=Quaternion.Euler(0,(float)rng.NextDouble()*4-2,0);
                if(rng.NextDouble()<.25)Shape("Snow drift on stone seam",PrimitiveType.Cube,new Vector3(x,.025f,z+.75f),new Vector3(1.2f,.025f,.14f),Snow);
            }
            for(int side=-1;side<=1;side+=2)
            {
                Shape("Snow bank parapet footing",PrimitiveType.Cube,new Vector3(side*6.65f,.22f,0),new Vector3(.9f,.45f,88),Rim);
                Shape("Unbroken snow along bridge",PrimitiveType.Cube,new Vector3(side*6.65f,.48f,0),new Vector3(.95f,.1f,88),Snow);
                for(int z=-42;z<=42;z+=4)
                {
                    Shape("Battered bridge merlon",PrimitiveType.Cube,new Vector3(side*6.8f,1.1f,z),new Vector3(.9f,1.3f,1.15f),Stone);
                    Shape("Merlon snow cap",PrimitiveType.Cube,new Vector3(side*6.8f,1.8f,z),new Vector3(1,.16f,1.25f),Snow);
                    var buttress=Shape("Hanging angled buttress",PrimitiveType.Cube,new Vector3(side*7.1f,-1.5f,z),new Vector3(.85f,3.5f,1.2f),Dark);buttress.transform.localRotation=Quaternion.Euler(0,0,side*20);
                    if(z%8==2){Shape("Warm bridge brazier",PrimitiveType.Cylinder,new Vector3(side*6.35f,1.9f,z),new Vector3(.38f,.1f,.38f),Dark);Shape("Brazier ember",PrimitiveType.Sphere,new Vector3(side*6.35f,2.12f,z),new Vector3(.25f,.5f,.25f),new Color(1,.64f,.22f));}
                    for(int j=0;j<2;j++){var ice=Shape("Hanging ice spike",PrimitiveType.Cube,new Vector3(side*(7+j*.23f),-1.7f-j*.45f,z+.4f),new Vector3(.13f,1.5f+j*.7f,.14f),Ice);ice.transform.localRotation=Quaternion.Euler(0,0,side*9);}
                }
                for(int z=-30;z<=30;z+=20)
                {
                    Shape("Ancient watcher plinth",PrimitiveType.Cube,new Vector3(side*9.4f,-.2f,z),new Vector3(3,1.5f,3),Dark);
                    var statue=Shape("Carved northern watcher",PrimitiveType.Capsule,new Vector3(side*9.4f,2.2f,z),new Vector3(1.8f,2.7f,1.5f),Stone);statue.transform.localRotation=Quaternion.Euler(0,side*-25,side*5);
                    Shape("Watcher hood",PrimitiveType.Cube,new Vector3(side*9.5f,4.1f,z),new Vector3(2.1f,1,1.8f),Rim);
                    Shape("Snow on watcher hood",PrimitiveType.Cube,new Vector3(side*9.5f,4.65f,z),new Vector3(2.2f,.13f,1.85f),Snow);
                    Shape("Watcher cold rune",PrimitiveType.Cube,new Vector3(side*8.55f,3.65f,z-.3f),new Vector3(.05f,.13f,.5f),Ice);
                }
            }
            for(int team=0;team<2;team++)
            {
                int sign=team==0?-1:1;Color tint=team==0?new Color(.18f,.66f,1):new Color(.89f,.22f,.31f);
                Shape("Base platform",PrimitiveType.Cylinder,new Vector3(0,-.18f,sign*37),new Vector3(20,.16f,20),Stone);
                for(int i=0;i<3;i++)
                {
                    var ring=Art.Ring("Northern base engraved circle",transform,3+i*.7f,.06f,i==2?Snow:tint);ring.transform.localPosition=new Vector3(0,.02f,sign*39);
                }
                Shape("Fountain dais",PrimitiveType.Cylinder,new Vector3(0,.10f,sign*40.8f),new Vector3(7,.12f,4),Rim);
                for(int k=-2;k<=2;k++)
                {
                    Shape("Frozen base arch pillar",PrimitiveType.Cube,new Vector3(k*3,2.2f,sign*44),new Vector3(1.2f,4.5f,1.4f),Dark);
                    Shape("Pillar frozen crest",PrimitiveType.Cube,new Vector3(k*3,4.5f,sign*44),new Vector3(1.6f,.2f,1.7f),Snow);
                }
                for(int i=-1;i<=1;i+=2){var crystal=Shape("Base crystal spire",PrimitiveType.Cube,new Vector3(i*7.8f,2.4f,sign*35),new Vector3(.9f,4.5f,.9f),tint);crystal.transform.localRotation=Quaternion.Euler(0,45,i*9);}
            }
            foreach(float z in new[]{-17f,-6f,6f,17f})
            {
                int side=z<0?-1:1;
                for(int k=0;k<30;k++)
                {
                    float x=side*(4.15f+(float)rng.NextDouble()*1.45f),zz=z-1.7f+(float)rng.NextDouble()*3.4f;
                    var grass=Shape("Frosted concealment brush",PrimitiveType.Cube,new Vector3(x,.43f,zz),new Vector3(.075f,.7f+(float)rng.NextDouble()*.4f,.075f),k%4==0?Snow:new Color(.37f,.59f,.68f));grass.transform.localRotation=Quaternion.Euler(8,0,side*12);
                }
            }
            // Frosted mountain silhouettes beyond the railing.
            var cliffMesh=Art.CurvedTube(new[]{Vector3.zero,new Vector3(.08f,.55f,0),new Vector3(-.1f,1,.12f)},new[]{.65f,.9f,.02f},7);
            var snowMesh=Art.CurvedTube(new[]{Vector3.zero,new Vector3(.02f,.18f,0),new Vector3(-.08f,.48f,.08f)},new[]{.76f,.53f,.015f},7);
            for(int s=-1;s<=1;s+=2)for(int i=0;i<12;i++)
            {
                var rock=Art.MeshObject("Faceted abyss ice cliff",transform,cliffMesh,i%2==0?Dark:Stone);rock.transform.localPosition=new Vector3(s*(15+i%3*4),-8-i%2*3,(i-5.5f)*9);rock.transform.localScale=new Vector3(7,12+i%3*3,7);rock.transform.localRotation=Quaternion.Euler(0,i*33,s*9);
                var cap=Art.MeshObject("Irregular snow-covered peak",transform,snowMesh,Snow);cap.transform.localPosition=rock.transform.localPosition+Vector3.up*(8+i%3*2);cap.transform.localScale=new Vector3(6,8,6);cap.transform.localRotation=Quaternion.Euler(0,i*33,s*9);
            }
            CombineStatic();
        }
        void CombineStatic()
        {
            var groups=new Dictionary<Material,List<CombineInstance>>();var originals=new List<GameObject>();
            foreach(var filter in GetComponentsInChildren<MeshFilter>())
            {
                var renderer=filter.GetComponent<MeshRenderer>();if(renderer==null)continue;var material=renderer.sharedMaterial;
                if(!groups.ContainsKey(material))groups.Add(material,new List<CombineInstance>());
                groups[material].Add(new CombineInstance{mesh=filter.sharedMesh,transform=transform.worldToLocalMatrix*filter.transform.localToWorldMatrix});originals.Add(filter.gameObject);
            }
            foreach(var pair in groups)
            {
                var mesh=new Mesh{name="Batched frozen bridge masonry",indexFormat=UnityEngine.Rendering.IndexFormat.UInt32};mesh.CombineMeshes(pair.Value.ToArray());Art.Own(mesh);
                var go=new GameObject("Abyss static batch • "+pair.Key.name);go.transform.SetParent(transform,false);go.AddComponent<MeshFilter>().sharedMesh=mesh;go.AddComponent<MeshRenderer>().sharedMaterial=pair.Key;
            }
            foreach(var go in originals){go.SetActive(false);Destroy(go);}
        }
        public static void Structure(ActorVisual visual,int kind,int team)
        {
            Color stone=new Color(.32f,.42f,.50f),trim=new Color(.63f,.71f,.74f),tint=team==0?new Color(.13f,.65f,1):new Color(.92f,.19f,.32f);
            Transform root=visual.Body;
            Art.Shape("Northern octagonal footing",PrimitiveType.Cylinder,root,Vector3.up*.13f,new Vector3(kind==5?3.8f:2.2f,.13f,kind==5?3.8f:2.2f),stone);
            if(kind==9 || kind==5)
            {
                Art.Shape("Crystal socket",PrimitiveType.Cylinder,root,Vector3.up*.38f,new Vector3(kind==5?2.4f:1.5f,.2f,kind==5?2.4f:1.5f),trim);
                var gem=Art.Shape("Nexus crystal",PrimitiveType.Cube,root,Vector3.up*(kind==5?1.6f:1.1f),new Vector3(kind==5?1.4f:.8f,kind==5?2.7f:1.4f,kind==5?1.4f:.8f),tint);gem.transform.localRotation=Quaternion.Euler(0,45,kind==5?10:0);
                for(int i=0;i<4;i++){float a=i*Mathf.PI/2;Art.Shape("Crystal prong",PrimitiveType.Cube,root,new Vector3(Mathf.Sin(a)*(kind==5?1.4f:.85f),.62f,Mathf.Cos(a)*(kind==5?1.4f:.85f)),new Vector3(.2f,1,.2f),stone);}
            }
            else
            {
                Art.Shape("Tower armored column",PrimitiveType.Capsule,root,Vector3.up*1.55f,new Vector3(1.2f,1.4f,1.1f),stone);
                Art.Shape("Tower shoulder mantle",PrimitiveType.Cube,root,Vector3.up*2.5f,new Vector3(2,.4f,1.1f),trim);
                var head=Art.Shape("Tower carved crown",PrimitiveType.Cube,root,Vector3.up*2.9f,new Vector3(.86f,.85f,.86f),stone);head.transform.localRotation=Quaternion.Euler(0,45,0);
                Art.Shape("Tower frost crystal",PrimitiveType.Sphere,root,new Vector3(0,2.5f,.63f),Vector3.one*.55f,tint);
                for(int s=-1;s<=1;s+=2){var horn=Art.Shape("Tower crown horn",PrimitiveType.Cube,root,new Vector3(s*.65f,3.2f,0),new Vector3(.2f,1,.3f),trim);horn.transform.localRotation=Quaternion.Euler(0,0,s*-22);}
            }
        }
    }
}
