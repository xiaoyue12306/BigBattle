using UnityEngine;

namespace BigBattle
{
    // Original sunny single-lane garden. Decorations sit outside the walk strip;
    // movement remains an explicit flat-plane boundary, independent of colliders.
    public sealed class FlowerLaneMap : MonoBehaviour
    {
        public const float HalfLength=34;
        public static Vector3 Clamp(Vector3 p)
        {
            p.y=0;p.z=Mathf.Clamp(p.z,-HalfLength,HalfLength);
            float width=Mathf.Lerp(6.0f,9.0f,Mathf.InverseLerp(24,30,Mathf.Abs(p.z)));
            p.x=Mathf.Clamp(p.x,-width,width);return p;
        }
        public static Vector3 Spawn(int team)=>new Vector3(0,0,team==0?-29:29);
        static readonly Color Grass=new Color(.56f,.79f,.47f),Cream=new Color(.98f,.90f,.70f),Water=new Color(.43f,.77f,.83f);
        public void Build()
        {
            var sun=new GameObject("Flower creek sunshine").AddComponent<Light>();sun.transform.SetParent(transform,false);sun.type=LightType.Directional;sun.transform.rotation=Quaternion.Euler(48,-32,0);sun.intensity=.85f;sun.color=new Color(1,.95f,.84f);sun.shadows=LightShadows.Soft;sun.shadowStrength=.30f;
            Shape("Clear pastel lake",PrimitiveType.Cube,new Vector3(0,-.8f,0),new Vector3(180,.20f,180),Water);
            Shape("Soft island foundation",PrimitiveType.Cube,new Vector3(0,-.55f,0),new Vector3(25,.65f,72),new Color(.68f,.74f,.51f));
            Shape("Cream riverbank",PrimitiveType.Cube,new Vector3(0,-.24f,0),new Vector3(25.5f,.22f,72.5f),Cream);
            Shape("Flower meadow",PrimitiveType.Cube,new Vector3(0,-.11f,0),new Vector3(24.9f,.13f,72),Grass);
            for(int sign=-1;sign<=1;sign+=2)
            {
                Shape("Round garden headland",PrimitiveType.Cylinder,new Vector3(0,-.24f,sign*31),new Vector3(25.5f,.11f,25.5f),Cream);
                Shape("Round base lawn",PrimitiveType.Cylinder,new Vector3(0,-.105f,sign*31),new Vector3(24.9f,.035f,24.9f),Grass);
                Shape("Base courtyard",PrimitiveType.Cylinder,new Vector3(0,-.035f,sign*29),new Vector3(19,.025f,18),new Color(.89f,.89f,.69f));
                var rune=Art.Ring("Team flower courtyard",transform,5.8f,.08f,sign<0?Art.Mint:new Color(1,.63f,.56f));rune.transform.localPosition=new Vector3(0,.012f,sign*29);
            }
            Shape("Single sunny lane",PrimitiveType.Cube,new Vector3(0,-.031f,0),new Vector3(12.5f,.040f,57),new Color(.94f,.87f,.67f));
            // A little stream crosses the meadow; the low wooden bridge is part
            // of the same walkable plane, so dashes and summoned AI cross safely.
            Shape("Flower creek crossing",PrimitiveType.Cube,new Vector3(0,-.035f,0),new Vector3(29,.03f,6.5f),Water);
            Shape("Bridge foundation",PrimitiveType.Cube,new Vector3(0,-.03f,0),new Vector3(12.6f,.06f,7.2f),new Color(.66f,.52f,.32f));
            for(int i=0;i<13;i++)Shape("Honey wood bridge plank",PrimitiveType.Cube,new Vector3(0,-.002f,-3.3f+i*.55f),new Vector3(12.5f,.035f,.49f),i%2==0?new Color(.88f,.74f,.47f):new Color(.95f,.83f,.57f));
            for(int side=-1;side<=1;side+=2)
            {
                for(int i=0;i<4;i++){float z=-3.3f+i*2.2f;Shape("Bridge white post",PrimitiveType.Cylinder,new Vector3(side*6.8f,.5f,z),new Vector3(.15f,.50f,.15f),Cream);Shape("Bridge golden cap",PrimitiveType.Sphere,new Vector3(side*6.8f,1.08f,z),Vector3.one*.28f,Art.Yellow);}
                Shape("Bridge handrail",PrimitiveType.Cube,new Vector3(side*6.8f,.82f,0),new Vector3(.14f,.15f,7.2f),Cream);
            }
            var random=new System.Random(6213);
            for(int i=0;i<30;i++)
            {
                float z=-26+i*1.8f;if(Mathf.Abs(z)<4)continue;
                for(int side=-1;side<=1;side+=2)
                {
                    Shape("Rounded lane curb",PrimitiveType.Sphere,new Vector3(side*6.7f,.025f,z),new Vector3(.25f,.12f,1.4f),Cream);
                    if(i%3==0)Flowers(new Vector3(side*7.8f,0,z),i);
                    if(i%6==0)Tree(new Vector3(side*10.3f,0,z+1),i);
                    else if(i%5==0)Shape("Mossy pebble",PrimitiveType.Sphere,new Vector3(side*10.3f,.2f,z),new Vector3(1.2f,.55f,.8f),new Color(.66f,.76f,.66f));
                }
                for(int j=-1;j<=1;j++)
                {
                    var stone=Shape("Soft cream stepping tile",PrimitiveType.Cylinder,new Vector3(j*3.2f+(i%2)*.5f,.005f,z),new Vector3(2.4f,.012f,1.0f),new Color(.97f,.91f,.75f));stone.transform.localRotation=Quaternion.Euler(0,(float)random.NextDouble()*15-7,0);
                }
            }
            for(int side=-1;side<=1;side+=2)for(int i=0;i<9;i++)
            {
                Vector3 p=new Vector3(side*(15+i%3*1.5f),-.64f,-28+i*7);
                Shape("Lily pad",PrimitiveType.Cylinder,p,new Vector3(.9f,.025f,.75f),new Color(.40f,.69f,.48f));
                if(i%3==0)Shape("Lake lotus",PrimitiveType.Sphere,p+Vector3.up*.11f,new Vector3(.32f,.18f,.32f),new Color(1,.73f,.77f));
            }
            for(int sign=-1;sign<=1;sign+=2)for(int side=-1;side<=1;side+=2){Tree(new Vector3(side*9.5f,0,sign*32),3);Flowers(new Vector3(side*6,0,sign*33),2);}
            var center=Art.Ring("Bridge flower medallion",transform,1.9f,.06f,new Color(.99f,.93f,.72f));center.transform.localPosition=Vector3.up*.032f;
        }
        GameObject Shape(string name,PrimitiveType primitive,Vector3 position,Vector3 scale,Color tint)=>Art.Shape(name,primitive,transform,position,scale,tint);
        void Tree(Vector3 point,int index)
        {
            Shape("Little apricot tree trunk",PrimitiveType.Cylinder,point+Vector3.up*.8f,new Vector3(.28f,.8f,.28f),new Color(.61f,.45f,.29f));
            Color leaf=index%2==0?new Color(.45f,.72f,.43f):new Color(.32f,.67f,.51f);
            Shape("Round friendly tree crown",PrimitiveType.Sphere,point+Vector3.up*2.1f,new Vector3(2.1f,2.3f,1.9f),leaf);
            Shape("Little side crown",PrimitiveType.Sphere,point+new Vector3(.6f,1.8f,0),Vector3.one*1.35f,leaf);
            Shape("Apricot fruit",PrimitiveType.Sphere,point+new Vector3(-.5f,1.7f,-.75f),Vector3.one*.28f,Art.Yellow);
        }
        void Flowers(Vector3 point,int index)
        {
            for(int n=0;n<3;n++)
            {
                Vector3 p=point+new Vector3((n-1)*.30f,.36f,n%2*.25f);
                Shape("Meadow flower stem",PrimitiveType.Capsule,p-Vector3.up*.15f,new Vector3(.04f,.2f,.04f),Art.Mint);
                for(int k=0;k<5;k++){float a=k*Mathf.PI*2/5;Shape("Soft flower petal",PrimitiveType.Sphere,p+new Vector3(Mathf.Sin(a)*.14f,0,Mathf.Cos(a)*.14f),new Vector3(.21f,.09f,.21f),(index+n)%2==0?new Color(1,.67f,.72f):Art.Cream);}
                Shape("Warm flower center",PrimitiveType.Sphere,p+Vector3.up*.02f,Vector3.one*.13f,Art.Yellow);
            }
        }
    }
}
