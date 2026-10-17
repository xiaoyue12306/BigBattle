using UnityEngine;

namespace BigBattle
{
    public static class GhostArt
    {
        public static readonly Color Teal=new Color(.03f,.95f,.65f,.85f),Blue=new Color(.06f,.55f,1,.85f),Dark=new Color(.025f,.035f,.045f),Red=new Color(1,.16f,.24f,.88f);
        public static void Pointer(Transform parent,Color tint)
        {
            var go=Art.Shape("Facing rune",PrimitiveType.Cube,parent,new Vector3(0,.025f,.94f),new Vector3(.10f,.035f,.38f),tint);go.transform.localRotation=Quaternion.Euler(0,45,0);
        }
        public static LineRenderer Line(Transform parent,string name,Color color,float width,int count)
        {
            var go=new GameObject(name);go.transform.SetParent(parent,false);var line=go.AddComponent<LineRenderer>();line.sharedMaterial=Art.FxMat("Ghost line "+ColorUtility.ToHtmlStringRGBA(color),color);
            line.useWorldSpace=false;line.positionCount=count;line.widthMultiplier=width;line.numCornerVertices=2;line.numCapVertices=2;line.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;line.receiveShadows=false;return line;
        }
        public static void Skull(Transform root)
        {
            Art.Shape("Spectral cranium",PrimitiveType.Sphere,root,new Vector3(0,.06f,0),new Vector3(.50f,.45f,.42f),new Color(.68f,1,.84f));
            Art.Shape("Skull jaw",PrimitiveType.Cube,root,new Vector3(0,-.18f,.06f),new Vector3(.29f,.11f,.24f),Teal);
            for(int s=-1;s<=1;s+=2){Art.Shape("Black eye socket",PrimitiveType.Sphere,root,new Vector3(s*.12f,.06f,.175f),new Vector3(.15f,.17f,.07f),Dark);Art.Shape("Soul eye",PrimitiveType.Sphere,root,new Vector3(s*.12f,.06f,.212f),new Vector3(.065f,.08f,.025f),Teal);}
            for(int i=0;i<4;i++)Art.Shape("Little tooth",PrimitiveType.Cube,root,new Vector3((i-1.5f)*.065f,-.12f,.18f),new Vector3(.045f,.10f,.045f),Art.Cream);
            var trail=root.gameObject.AddComponent<TrailRenderer>();trail.sharedMaterial=Art.FxMat("Soul wake",Teal);trail.time=.3f;trail.widthMultiplier=.32f;trail.startColor=Teal;trail.endColor=new Color(.02f,.5f,1,0);trail.minVertexDistance=.06f;trail.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;
        }
        public static GameObject Sword(Transform parent)
        {
            var sword=new GameObject("Black soul sword");sword.transform.SetParent(parent,false);
            var edge=Art.Shape("Radiant green sword edge",PrimitiveType.Cube,sword.transform,new Vector3(0,.88f,0),new Vector3(.38f,1.78f,.14f),Teal);
            edge.GetComponent<Renderer>().sharedMaterial=Art.FxMat("Soul sword edge",Teal);
            // Dark faces on both sides: the overhead camera often sees the back
            // of the caster, which previously exposed a solid green rectangle.
            Art.Shape("Onyx blade",PrimitiveType.Cube,sword.transform,new Vector3(0,.88f,0),new Vector3(.29f,1.76f,.18f),Dark);
            Art.Shape("Green blade channel",PrimitiveType.Cube,sword.transform,new Vector3(0,.88f,.13f),new Vector3(.055f,1.60f,.025f),Teal);
            Art.Shape("Rear soul channel",PrimitiveType.Cube,sword.transform,new Vector3(0,.88f,-.105f),new Vector3(.045f,1.60f,.025f),Teal);
            for(int i=0;i<3;i++)for(int side=-1;side<=1;side+=2){var rune=Art.Shape("Etched blade soul rune",PrimitiveType.Cube,sword.transform,new Vector3(0,.44f+i*.38f,side*.113f),new Vector3(.095f,.095f,.018f),Blue);rune.transform.localRotation=Quaternion.Euler(0,0,45);}
            Art.Shape("Iron crossguard",PrimitiveType.Cube,sword.transform,Vector3.zero,new Vector3(.70f,.12f,.18f),Dark);
            Art.Shape("Hilt",PrimitiveType.Cylinder,sword.transform,Vector3.down*.19f,new Vector3(.10f,.17f,.10f),Dark);
            Art.Shape("Soul gem",PrimitiveType.Sphere,sword.transform,Vector3.zero,Vector3.one*.18f,Teal);
            return sword;
        }
    }
    public sealed class SpiritAura : MonoBehaviour
    {
        Combatant actor;Transform[] motes=new Transform[7];
        public void Setup(Combatant owner)
        {
            actor=owner;
            for(int i=0;i<motes.Length;i++){motes[i]=Art.Glint(transform,i%2==0?GhostArt.Teal:GhostArt.Blue,true).transform;motes[i].localScale=Vector3.one*.15f;}
            Art.Ring("Returned soul rune",transform,.63f,.06f,GhostArt.Teal);
            SoulVfx.Attach(transform,owner.World,SoulEffect.Aura);
        }
        void LateUpdate()
        {
            if(actor==null){Destroy(gameObject);return;}if(actor.World.Paused)return;
            for(int i=0;i<motes.Length;i++){float a=Time.time*2+i*6.28f/motes.Length;motes[i].localPosition=new Vector3(Mathf.Cos(a)*.58f,.3f+(Time.time*.6f+i*.21f)%1.5f,Mathf.Sin(a)*.58f);motes[i].rotation=Quaternion.LookRotation(new Vector3(0,18,-14));}
        }
    }
    public sealed class SoulCage : MonoBehaviour
    {
        Combatant victim;LineRenderer[] chains=new LineRenderer[6];Transform[] portals=new Transform[6];float end;
        public void Setup(Combatant target)
        {
            victim=target;end=Time.time+2;
            SoulVfx.Attach(transform,target.World,SoulEffect.Cage);
            for(int i=0;i<6;i++){chains[i]=GhostArt.Line(transform,"Binding iron and ghost light",i%2==0?GhostArt.Teal:new Color(.10f,.17f,.20f),i%2==0?.045f:.075f,17);portals[i]=Art.Ring("Void anchor",transform,.24f,.045f,GhostArt.Blue).transform;}
            Place();
        }
        void Place()
        {
            transform.position=victim.transform.position;
            for(int i=0;i<6;i++)
            {
                float a=i*Mathf.PI/3;Vector3 from=new Vector3(Mathf.Sin(a)*1.40f,.5f+i%3*.55f,Mathf.Cos(a)*1.40f),to=new Vector3(-Mathf.Sin(a)*.30f,.65f+i%2*.40f,-Mathf.Cos(a)*.30f);
                portals[i].localPosition=from;portals[i].localRotation=Quaternion.FromToRotation(Vector3.up,-from.normalized);
                for(int j=0;j<17;j++){float t=j/16f;Vector3 p=Vector3.Lerp(from,to,t)+Vector3.up*(j%2==0?.035f:-.035f);chains[i].SetPosition(j,p);}
            }
        }
        void LateUpdate(){if(victim==null || !victim.Alive || Time.time>=end || Time.time>=victim.BoundUntil){Destroy(gameObject);return;}if(victim.World.Paused)return;Place();}
    }
}
