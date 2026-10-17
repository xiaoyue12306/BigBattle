using UnityEngine;

namespace BigBattle
{
    // One cast owns a small fixed set of shared-mesh ribbons. It follows its
    // actor without changing combat range and freezes with the game clock.
    public sealed class BladeVortex : MonoBehaviour
    {
        Transform actor;Combatant combatant;float born,duration;
        readonly Transform[] bands=new Transform[3];
        readonly Transform[] leaves=new Transform[6];
        Renderer[] renderers;Color[] colors;
        readonly MaterialPropertyBlock block=new MaterialPropertyBlock();
        public void Setup(Transform owner,float life)
        {
            actor=owner;combatant=owner.GetComponent<Combatant>();born=Time.time;duration=life;
            for(int i=0;i<bands.Length;i++)
            {
                var go=Art.Crescent(transform,i==1?new Color(.05f,.86f,.65f,.8f):new Color(1,.71f,.08f,.88f),i==2?150:265,i==2?.10f:.18f);
                bands[i]=go.transform;bands[i].localPosition=Vector3.up*(.43f+i*.30f);bands[i].localScale=Vector3.one*(2.15f-i*.17f);
            }
            for(int i=0;i<leaves.Length;i++){leaves[i]=Art.Glint(transform,i%2==0?Art.Yellow:Art.Mint,true).transform;leaves[i].localScale=Vector3.one*.13f;}
            renderers=GetComponentsInChildren<Renderer>();colors=new Color[renderers.Length];
            for(int i=0;i<colors.Length;i++)colors[i]=renderers[i].sharedMaterial.color;
            Place();
        }
        void LateUpdate()
        {
            if(Time.timeScale==0)return;
            if(actor==null || !actor.gameObject.activeInHierarchy || (combatant!=null && !combatant.Alive) || Time.time-born>=duration){Destroy(gameObject);return;}
            Place();
        }
        void Place()
        {
            float age=Time.time-born,t=age/duration;
            transform.position=actor.position;
            for(int i=0;i<bands.Length;i++)bands[i].localRotation=Quaternion.Euler(i==1?-18:14,age*(i==1?-530:620)+i*110,0);
            for(int i=0;i<leaves.Length;i++)
            {
                float angle=(age*440+i*60)*Mathf.Deg2Rad;
                leaves[i].localPosition=new Vector3(Mathf.Sin(angle)*2.13f,.8f+Mathf.Sin(age*9+i)*.3f,Mathf.Cos(angle)*2.13f);
                leaves[i].rotation=Quaternion.LookRotation(new Vector3(0,18,-14))*Quaternion.Euler(0,0,age*150+i*60);
            }
            float fade=Mathf.Min(1,(1-t)*5)*Mathf.Min(1,age*20+.3f);
            for(int i=0;i<renderers.Length;i++){Color tint=colors[i];tint.a*=fade;block.SetColor("_Color",tint);block.SetFloat("_Progress",.5f);renderers[i].SetPropertyBlock(block);}
        }
    }
}
