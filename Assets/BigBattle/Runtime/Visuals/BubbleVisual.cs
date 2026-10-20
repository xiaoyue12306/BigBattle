using System.Collections.Generic;
using UnityEngine;

namespace BigBattle
{
    public sealed class BubbleVisual : MonoBehaviour
    {
        static Mesh cloud;
        readonly List<Transform> cells=new List<Transform>();readonly List<float> distances=new List<float>();readonly List<Renderer> renderers=new List<Renderer>();
        readonly MaterialPropertyBlock block=new MaterialPropertyBlock();Transform shell,halo;
        public float VisualClock {get;private set;}
        public Vector3 ShellScale=>shell!=null?shell.localScale:Vector3.zero;
        static Mesh Cloud()
        {
            if(cloud!=null)return cloud;var vertices=new List<Vector3>();var normals=new List<Vector3>();var uv=new List<Vector2>();var triangles=new List<int>();
            Vector3[] centers={new Vector3(0,.4f,0),new Vector3(.24f,.30f,.14f),new Vector3(-.23f,.29f,-.15f),new Vector3(.1f,.72f,-.04f),new Vector3(-.16f,.92f,.16f)};
            for(int n=0;n<centers.Length;n++)
            {
                int start=vertices.Count;float radius=n==0?.5f:n<3?.33f:n==3?.24f:.105f;
                for(int y=0;y<=8;y++)for(int x=0;x<=12;x++){float a=y*Mathf.PI/8,b=x*Mathf.PI*2/12;Vector3 normal=new Vector3(Mathf.Sin(a)*Mathf.Cos(b),Mathf.Cos(a),Mathf.Sin(a)*Mathf.Sin(b));vertices.Add(centers[n]+normal*radius);normals.Add(normal);uv.Add(new Vector2(x/12f,y/8f));}
                for(int y=0;y<8;y++)for(int x=0;x<12;x++){int i=start+y*13+x;triangles.Add(i);triangles.Add(i+1);triangles.Add(i+13);triangles.Add(i+1);triangles.Add(i+14);triangles.Add(i+13);}
            }
            cloud=new Mesh{name="Shared original golden explosion billows"};cloud.SetVertices(vertices);cloud.SetNormals(normals);cloud.SetUVs(0,uv);cloud.SetTriangles(triangles,0);cloud.RecalculateBounds();Art.Own(cloud);return cloud;
        }
        Renderer Cell(string name,Vector3 position,Vector3 scale,bool sphere)
        {
            GameObject go;
            if(sphere){go=GameObject.CreatePrimitive(PrimitiveType.Sphere);Destroy(go.GetComponent<Collider>());}
            else{go=new GameObject(name);go.AddComponent<MeshFilter>().sharedMesh=Cloud();go.AddComponent<MeshRenderer>();}
            go.name=name;go.transform.SetParent(transform,false);go.transform.localPosition=position;go.transform.localScale=scale;var renderer=go.GetComponent<Renderer>();renderer.sharedMaterial=Art.BubbleMaterial();renderer.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;renderer.receiveShadows=false;renderers.Add(renderer);return renderer;
        }
        public static BubbleVisual Bomb(Transform parent,Vector3 position)
        {
            var go=new GameObject("Breathing golden bubble");go.transform.SetParent(parent);go.transform.position=position;var v=go.AddComponent<BubbleVisual>();
            v.shell=v.Cell("Translucent bubble shell",Vector3.up*.58f,Vector3.one*1.1f,true).transform;
            v.halo=Art.Ring("Bubble contact halo",v.transform,.48f,.035f,Art.Yellow).transform;v.PaintBomb(0);return v;
        }
        public static BubbleVisual Blast(Transform parent,Vector3 position,Vector3 across,Vector3 forward,float[] limits)
        {
            var go=new GameObject("Near-to-far golden cross explosion");go.transform.SetParent(parent);go.transform.position=position;var v=go.AddComponent<BubbleVisual>();
            v.cells.Add(v.Cell("Cross origin",Vector3.zero,Vector3.one*1.45f,false).transform);v.distances.Add(0);
            Vector3[] directions={across,-across,forward,-forward};
            for(int d=0;d<4;d++)for(float start=0;start<limits[d]-.02f;start+=.8f)
            {
                float span=Mathf.Min(.8f,limits[d]-start),distance=start+span*.5f;var cell=v.Cell("Golden rolling blast "+d, directions[d]*distance,new Vector3(1.12f,1.05f,span*1.12f),false).transform;
                cell.localRotation=Quaternion.LookRotation(directions[d]);v.cells.Add(cell);v.distances.Add(distance);
            }
            v.PaintBlast(0,0);return v;
        }
        void Tint(Renderer r,float age,float alpha,float mode)
        {block.SetFloat("_FxClock",VisualClock);block.SetFloat("_Age",age);block.SetFloat("_Alpha",alpha);block.SetFloat("_Mode",mode);r.SetPropertyBlock(block);}
        public void PaintBomb(float age)
        {
            VisualClock=age;float pulse=Mathf.Sin(age*8+age*age*3);shell.localScale=new Vector3(1.08f+pulse*.07f,1.08f-pulse*.045f,1.08f+pulse*.07f);
            shell.localPosition=Vector3.up*(.58f+pulse*.025f);halo.localScale=Vector3.one*(1+pulse*.07f);Tint(renderers[0],age,.76f,0);
        }
        public void PaintBlast(float age,float reach)
        {
            VisualClock=age;
            for(int i=0;i<cells.Count;i++)
            {
                float elapsed=age-distances[i]/BubbleField.Length*BubbleField.TravelTime;bool visible=reach+.42f>=distances[i] && elapsed>=-.012f;
                cells[i].gameObject.SetActive(visible);if(!visible)continue;
                float fade=Mathf.Clamp01((.43f-elapsed)/.23f),rise=1+Mathf.Sin(Mathf.Clamp01(elapsed/.28f)*Mathf.PI)*.12f;
                var scale=cells[i].localScale;scale.y=rise*(i==0?1.4f:1.05f);cells[i].localScale=scale;Tint(renderers[i],elapsed,fade,1);
            }
        }
    }
}
