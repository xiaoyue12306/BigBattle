using System.Collections.Generic;
using UnityEngine;

namespace BigBattle
{
    public static class Art
    {
        static readonly Dictionary<string, Material> materials = new Dictionary<string, Material>();
        static readonly List<Object> owned = new List<Object>();
        static readonly Dictionary<string, Mesh> rings = new Dictionary<string, Mesh>();
        public static readonly Color Yellow = new Color(1f, .79f, .16f);
        public static readonly Color Cream = new Color(1f, .96f, .68f);
        public static readonly Color Ink = new Color(.13f, .20f, .24f);
        public static readonly Color Mint = new Color(.30f, .77f, .62f);
        public static readonly Color Coral = new Color(1f, .40f, .39f);

        public static Material Mat(string key, Color color, bool unlit = false)
        {
            Material material;
            if (materials.TryGetValue(key, out material)) return material;
            material = new Material(Shader.Find(unlit ? "Unlit/Color" : "Standard")) { name = key, color = color };
            if (!unlit) { material.SetFloat("_Glossiness", .18f); material.SetFloat("_Metallic", 0); }
            materials.Add(key, material); owned.Add(material);
            return material;
        }

        public static GameObject Shape(string name, PrimitiveType type, Transform parent, Vector3 position, Vector3 scale, Color color)
        {
            GameObject go = GameObject.CreatePrimitive(type);
            go.name = name; go.transform.SetParent(parent, false); go.transform.localPosition = position; go.transform.localScale = scale;
            go.GetComponent<Renderer>().sharedMaterial = Mat("c" + ColorUtility.ToHtmlStringRGB(color), color);
            Object.Destroy(go.GetComponent<Collider>());
            return go;
        }
        public static Material FxMat(string key,Color color)
        {
            Material material;
            if(materials.TryGetValue("fx"+key,out material))return material;
            material=new Material(Shader.Find("BigBattle/CombatFX")){name=key,color=color};materials.Add("fx"+key,material);owned.Add(material);return material;
        }
        public static int SharedMaterialCount=>materials.Count;
        public static Material SoulMaterial(string texture,bool glow,bool ribbon=false)
        {
            string key="soul:"+texture+":"+glow+":"+ribbon;Material material;
            if(materials.TryGetValue(key,out material))return material;
            material=new Material(Shader.Find("BigBattle/SoulParticle")){name=key,color=Color.white,mainTexture=Resources.Load<Texture2D>("SoulVfx/"+texture)};
            material.SetInt("_DstBlend",glow?(int)UnityEngine.Rendering.BlendMode.One:(int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            material.SetFloat("_Glow",glow?1:0);material.SetFloat("_UvRotate",ribbon?1:0);material.SetFloat("_Gray",texture.StartsWith("Flipbooks/")?1:0);materials.Add(key,material);owned.Add(material);return material;
        }
        public static Mesh SoulQuad()
        {
            Mesh mesh;if(rings.TryGetValue("soul quad",out mesh))return mesh;
            mesh=new Mesh{name="Shared soul texture card",vertices=new[]{new Vector3(-.5f,-.5f,0),new Vector3(.5f,-.5f,0),new Vector3(.5f,.5f,0),new Vector3(-.5f,.5f,0)},uv=new[]{Vector2.zero,Vector2.right,Vector2.one,Vector2.up},triangles=new[]{0,1,2,0,2,3},colors=new[]{Color.white,Color.white,Color.white,Color.white}};
            mesh.RecalculateBounds();rings.Add("soul quad",mesh);owned.Add(mesh);return mesh;
        }
        public static GameObject Crescent(Transform parent,Color color,float sweep,float width)
        {
            string key="energy"+sweep+":"+width;Mesh mesh;
            if(!rings.TryGetValue(key,out mesh))
            {
                const int segments=64,rows=5;
                var vertices=new Vector3[(segments+1)*rows];var uv=new Vector2[vertices.Length];var colors=new Color[vertices.Length];var triangles=new int[segments*(rows-1)*6];
                for(int s=0;s<=segments;s++)for(int r=0;r<rows;r++)
                {
                    float t=s/(float)segments,v=r/(float)(rows-1),a=(t-.5f)*sweep*Mathf.Deg2Rad;
                    // sin(PI) can be slightly negative in float precision;
                    // fractional powers must never receive that endpoint.
                    float taper=Mathf.Pow(Mathf.Max(0,Mathf.Sin(t*Mathf.PI)),.65f);
                    int n=s*rows+r;vertices[n]=new Vector3(Mathf.Sin(a),0,Mathf.Cos(a))*(1-width*taper*(1-v));
                    uv[n]=new Vector2(t,v);colors[n]=new Color(1,1,1,Mathf.Min(1,taper*3));
                    if(s<segments && r<rows-1)
                    {
                        int at=(s*(rows-1)+r)*6;
                        triangles[at]=n;triangles[at+1]=n+1;triangles[at+2]=n+rows;
                        triangles[at+3]=n+1;triangles[at+4]=n+rows+1;triangles[at+5]=n+rows;
                    }
                }
                mesh=new Mesh{name="Shared tapered energy crescent"};mesh.vertices=vertices;mesh.uv=uv;mesh.colors=colors;mesh.triangles=triangles;mesh.RecalculateBounds();
                rings.Add(key,mesh);owned.Add(mesh);
            }
            return EnergyObject("Banana moonblade",parent,mesh,color);
        }
        // Both meshes are authored once; every travelling blade and ground stamp shares them.
        public static GameObject SequenceSurface(Transform parent,SequenceShape shape)
        {
            const string materialKey="Timeline sequence energy";Material material;
            if(!materials.TryGetValue(materialKey,out material)){material=new Material(Shader.Find("BigBattle/SequenceEnergy")){name=materialKey};materials.Add(materialKey,material);owned.Add(material);}
            if(shape==SequenceShape.Arc){var arc=Crescent(parent,GhostArt.Teal,220,.25f);arc.GetComponent<Renderer>().sharedMaterial=material;return arc;}
            Mesh mesh=SoulQuad();
            if(shape==SequenceShape.Spiral || shape==SequenceShape.Shards)
            {
                string key="sequence "+shape;
                if(!rings.TryGetValue(key,out mesh))
                {
                    var vertices=new List<Vector3>();var uv=new List<Vector2>();var indices=new List<int>();
                    if(shape==SequenceShape.Spiral)for(int arm=0;arm<2;arm++)for(int j=0;j<=96;j++)
                    {
                        float t=j/96f,a=t*Mathf.PI*4+arm*Mathf.PI,radius=1-t*.82f;
                        Vector3 radial=new Vector3(Mathf.Sin(a),0,Mathf.Cos(a));Vector3 p=radial*radius+Vector3.up*t*3.2f;
                        int n=vertices.Count;vertices.Add(p-radial*.07f);vertices.Add(p+radial*.07f);uv.Add(new Vector2(t,0));uv.Add(new Vector2(t,1));
                        if(j<96)indices.AddRange(new[]{n,n+1,n+2,n+1,n+3,n+2});
                    }
                    else for(int j=0;j<14;j++)
                    {
                        float a=j*2.39996f,radius=.5f+j%4*.16f;Vector3 p=new Vector3(Mathf.Sin(a)*radius,.15f+j%5*.17f,Mathf.Cos(a)*radius);float size=.06f+j%3*.025f;int n=vertices.Count;
                        vertices.AddRange(new[]{p+Vector3.up*size*3,p+Vector3.right*size,p-Vector3.up*size*3,p-Vector3.right*size});uv.AddRange(new[]{Vector2.up,Vector2.one,Vector2.right,Vector2.zero});indices.AddRange(new[]{n,n+1,n+2,n,n+2,n+3});
                    }
                    mesh=new Mesh{name=key};mesh.SetVertices(vertices);mesh.SetUVs(0,uv);mesh.SetTriangles(indices,0);mesh.RecalculateBounds();rings.Add(key,mesh);owned.Add(mesh);
                }
            }
            var go=new GameObject("Timeline "+shape);go.transform.SetParent(parent,false);go.AddComponent<MeshFilter>().sharedMesh=mesh;var renderer=go.AddComponent<MeshRenderer>();renderer.sharedMaterial=material;renderer.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;renderer.receiveShadows=false;return go;
        }
        public static GameObject DoomSurface(Transform parent,bool ground,int variant=0)
        {
            string key=ground?"doom rift "+variant:"doom upright blade";Mesh mesh;
            if(!rings.TryGetValue(key,out mesh))
            {
                var vertices=new List<Vector3>();var uv=new List<Vector2>();var triangles=new List<int>();
                if(!ground)
                {
                    const int rows=40,sides=8;
                    for(int y=0;y<=rows;y++)
                    {
                        float t=y/(float)rows,bulge=Mathf.Pow(Mathf.Max(.0001f,Mathf.Sin(t*Mathf.PI)),.65f);
                        for(int s=0;s<=sides;s++)
                        {
                            float a=s*Mathf.PI*2/sides;
                            vertices.Add(new Vector3(Mathf.Sin(a)*.44f*bulge,t*8.8f,Mathf.Cos(a)*.5f*bulge+Mathf.Sin(t*Mathf.PI)*2.0f-t*.8f));
                            uv.Add(new Vector2(s/(float)sides,t));
                            if(y<rows && s<sides){int n=y*(sides+1)+s;triangles.AddRange(new[]{n,n+1,n+sides+1,n+1,n+sides+2,n+sides+1});}
                        }
                    }
                }
                else
                {
                    // Longitudinal overlapping strips, with jagged branches joining the main chasm.
                    for(int branch=0;branch<(variant>=4?1:3);branch++)for(int j=0;j<=8;j++)
                    {
                        float t=j/8f,sign=branch%2==0?1:-1;
                        float x=branch==0?Mathf.Sin(t*7+variant*2)*.20f:sign*t*(1.1f+variant%4*.20f)+Mathf.Sin(t*12+variant)*.13f*t;
                        float z=branch==0?t*1.5f: .2f+(branch-1)*.6f+t*.45f;
                        float w=branch==0?.65f+Mathf.Sin(t*8+variant)*.18f:Mathf.Lerp(.20f,.008f,t);
                        int n=vertices.Count;vertices.Add(new Vector3(x-w,0,z));vertices.Add(new Vector3(x+w,0,z));uv.Add(new Vector2(0,t));uv.Add(new Vector2(1,t));
                        if(j<8)triangles.AddRange(new[]{n,n+1,n+2,n+1,n+3,n+2});
                    }
                }
                mesh=new Mesh{name=key};mesh.SetVertices(vertices);mesh.SetUVs(0,uv);mesh.SetTriangles(triangles,0);mesh.RecalculateBounds();rings.Add(key,mesh);owned.Add(mesh);
            }
            string materialKey=ground?(variant>=4?"doom scorch":"doom ground"):"doom blade";Material material;
            if(!materials.TryGetValue(materialKey,out material)){material=new Material(Shader.Find("BigBattle/DoomEnergy")){name=materialKey};material.SetFloat("_Ground",ground?(variant>=4?2:1):0);materials.Add(materialKey,material);owned.Add(material);}
            var go=new GameObject(key);go.transform.SetParent(parent,false);go.AddComponent<MeshFilter>().sharedMesh=mesh;var renderer=go.AddComponent<MeshRenderer>();renderer.sharedMaterial=material;renderer.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;renderer.receiveShadows=false;return go;
        }
        public static GameObject Lance(Transform parent,Color color)
        {
            Mesh mesh;
            if(!rings.TryGetValue("lance",out mesh))
            {
                const int segments=20;var vertices=new Vector3[(segments+1)*2];var uv=new Vector2[vertices.Length];var colors=new Color[vertices.Length];var triangles=new int[segments*6];
                for(int i=0;i<=segments;i++)
                {
                    float t=i/(float)segments,w=Mathf.Sin(t*Mathf.PI)*.40f;
                    vertices[i*2]=new Vector3(-w,0,t);vertices[i*2+1]=new Vector3(w,0,t);
                    uv[i*2]=new Vector2(t,0);uv[i*2+1]=new Vector2(t,1);
                    colors[i*2]=colors[i*2+1]=Color.white;
                    if(i<segments){int n=i*2,at=i*6;triangles[at]=n;triangles[at+1]=n+1;triangles[at+2]=n+2;triangles[at+3]=n+1;triangles[at+4]=n+3;triangles[at+5]=n+2;}
                }
                mesh=new Mesh{name="Shared thrust lance"};mesh.vertices=vertices;mesh.uv=uv;mesh.colors=colors;mesh.triangles=triangles;mesh.RecalculateBounds();rings.Add("lance",mesh);owned.Add(mesh);
            }
            return EnergyObject("Banana thrust stream",parent,mesh,color);
        }
        static GameObject EnergyObject(string name,Transform parent,Mesh mesh,Color color)
        {
            string key="energy"+ColorUtility.ToHtmlStringRGBA(color);Material material;
            if(!materials.TryGetValue(key,out material)){material=new Material(Shader.Find("BigBattle/BladeEnergy")){name="Moonblade "+key,color=color};materials.Add(key,material);owned.Add(material);}
            var go=new GameObject(name);go.transform.SetParent(parent,false);go.AddComponent<MeshFilter>().sharedMesh=mesh;
            var renderer=go.AddComponent<MeshRenderer>();renderer.sharedMaterial=material;renderer.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;renderer.receiveShadows=false;return go;
        }
        public static GameObject Glint(Transform parent,Color color,bool leaf=false)
        {
            string key=leaf?"leaf glyph":"star glyph";Mesh mesh;
            if(!rings.TryGetValue(key,out mesh))
            {
                int count=leaf?4:8;var vertices=new Vector3[count+1];var colors=new Color[count+1];var triangles=new int[count*3];colors[0]=Color.white;
                for(int i=0;i<count;i++)
                {
                    float a=i*Mathf.PI*2/count;float radius=leaf?1:i%2==0?1:.22f;
                    vertices[i+1]=new Vector3(Mathf.Sin(a)*radius*(leaf?.45f:1),Mathf.Cos(a)*radius,0);
                    colors[i+1]=new Color(1,1,1,.65f);triangles[i*3]=0;triangles[i*3+1]=i+1;triangles[i*3+2]=(i+1)%count+1;
                }
                mesh=new Mesh{name="Shared "+key};mesh.vertices=vertices;mesh.colors=colors;mesh.triangles=triangles;mesh.RecalculateBounds();rings.Add(key,mesh);owned.Add(mesh);
            }
            var go=new GameObject(key);go.transform.SetParent(parent,false);go.AddComponent<MeshFilter>().sharedMesh=mesh;
            var renderer=go.AddComponent<MeshRenderer>();renderer.sharedMaterial=FxMat(key+ColorUtility.ToHtmlStringRGBA(color),color);renderer.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;renderer.receiveShadows=false;return go;
        }
        public static GameObject Arc(Transform parent,Color color,float sweep=145)
        {
            string key="arc"+sweep;Mesh mesh;
            if(!rings.TryGetValue(key,out mesh))
            {
                const int segments=32;var vertices=new Vector3[(segments+1)*2];var colors=new Color[vertices.Length];var indices=new int[segments*6];
                for(int i=0;i<=segments;i++)
                {
                    float t=i/(float)segments;float angle=(t-.5f)*sweep*Mathf.Deg2Rad;
                    Vector3 d=new Vector3(Mathf.Sin(angle),0,Mathf.Cos(angle));
                    vertices[i*2]=d*(1-.19f*Mathf.Sin(t*Mathf.PI));vertices[i*2+1]=d;
                    colors[i*2]=new Color(1,1,1,Mathf.Sin(t*Mathf.PI)*.12f);colors[i*2+1]=new Color(1,1,1,Mathf.Sin(t*Mathf.PI)*.95f);
                    if(i<segments){int v=i*2,j=i*6;indices[j]=v;indices[j+1]=v+1;indices[j+2]=v+2;indices[j+3]=v+1;indices[j+4]=v+3;indices[j+5]=v+2;}
                }
                mesh=new Mesh{name="Curved slash ribbon"};mesh.vertices=vertices;mesh.colors=colors;mesh.triangles=indices;mesh.RecalculateNormals();rings.Add(key,mesh);owned.Add(mesh);
            }
            var go=new GameObject("Slash ribbon");go.transform.SetParent(parent,false);go.AddComponent<MeshFilter>().sharedMesh=mesh;
            var renderer=go.AddComponent<MeshRenderer>();renderer.sharedMaterial=FxMat(ColorUtility.ToHtmlStringRGBA(color),color);renderer.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;return go;
        }

        public static GameObject MeshObject(string name, Transform parent, Mesh mesh, Color color)
        {
            var go = new GameObject(name); go.transform.SetParent(parent, false);
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            go.AddComponent<MeshRenderer>().sharedMaterial = Mat("c" + ColorUtility.ToHtmlStringRGB(color), color);
            owned.Add(mesh); return go;
        }

        public static Mesh CurvedTube(Vector3[] centers, float[] radii, int sides = 12)
        {
            var vertices = new List<Vector3>(); var triangles = new List<int>();
            for (int y = 0; y < centers.Length; y++)
            {
                Vector3 tangent = (centers[Mathf.Min(y + 1, centers.Length - 1)] - centers[Mathf.Max(0, y - 1)]).normalized;
                Vector3 normal = Vector3.Cross(tangent, Vector3.forward).normalized;
                if (normal.sqrMagnitude < .1f) normal = Vector3.right;
                Vector3 binormal = Vector3.Cross(tangent, normal).normalized;
                for (int x = 0; x < sides; x++)
                {
                    float angle = x * Mathf.PI * 2 / sides;
                    vertices.Add(centers[y] + (normal * Mathf.Cos(angle) + binormal * Mathf.Sin(angle)) * radii[y]);
                    if (y < centers.Length - 1)
                    {
                        int a = y * sides + x, b = y * sides + (x + 1) % sides, c = a + sides, d = b + sides;
                        triangles.Add(a); triangles.Add(b); triangles.Add(c); triangles.Add(b); triangles.Add(d); triangles.Add(c);
                    }
                }
            }
            int start = vertices.Count; vertices.Add(centers[0]); int end = vertices.Count; vertices.Add(centers[centers.Length - 1]);
            for (int x = 0; x < sides; x++)
            {
                triangles.Add(start); triangles.Add((x + 1) % sides); triangles.Add(x);
                int last = (centers.Length - 1) * sides;
                triangles.Add(end); triangles.Add(last + x); triangles.Add(last + (x + 1) % sides);
            }
            var mesh = new Mesh { name = "Original curved banana mesh" };
            mesh.SetVertices(vertices); mesh.SetTriangles(triangles, 0); mesh.RecalculateNormals(); mesh.RecalculateBounds(); return mesh;
        }

        public static GameObject Ring(string name, Transform parent, float radius, float width, Color color, float y = .03f, int segments = 64)
        {
            string key = radius + ":" + width + ":" + y + ":" + segments;
            Mesh cached;
            if (rings.TryGetValue(key, out cached))
            {
                var shared = new GameObject(name); shared.transform.SetParent(parent, false);
                shared.AddComponent<MeshFilter>().sharedMesh = cached;
                shared.AddComponent<MeshRenderer>().sharedMaterial = Mat("ring" + ColorUtility.ToHtmlStringRGB(color), color, true);
                return shared;
            }
            var vertices = new Vector3[(segments + 1) * 2]; var indices = new int[segments * 6];
            for (int i = 0; i <= segments; i++)
            {
                float a = i * Mathf.PI * 2 / segments;
                vertices[i * 2] = new Vector3(Mathf.Sin(a) * (radius - width), y, Mathf.Cos(a) * (radius - width));
                vertices[i * 2 + 1] = new Vector3(Mathf.Sin(a) * radius, y, Mathf.Cos(a) * radius);
                if (i == segments) continue;
                int t = i * 6, v = i * 2;
                indices[t] = v; indices[t + 1] = v + 1; indices[t + 2] = v + 2;
                indices[t + 3] = v + 1; indices[t + 4] = v + 3; indices[t + 5] = v + 2;
            }
            var mesh = new Mesh { name = name }; mesh.vertices = vertices; mesh.triangles = indices; mesh.RecalculateNormals();
            rings.Add(key, mesh);
            GameObject ring = MeshObject(name, parent, mesh, color);
            ring.GetComponent<Renderer>().sharedMaterial = Mat("ring" + ColorUtility.ToHtmlStringRGB(color), color, true);
            return ring;
        }

        public static void Cleanup()
        {
            foreach (Object item in owned) if (item != null) Object.Destroy(item);
            owned.Clear(); materials.Clear(); rings.Clear();
        }
    }
}
