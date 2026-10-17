using UnityEngine;

namespace BigBattle
{
    public sealed class PeelTrap : MonoBehaviour
    {
        public bool Landed { get; private set; }
        GameRoot world;
        Combatant owner;
        Vector3 start, target;
        float born;
        Transform model;
        GameObject marker;
        public void Setup(GameRoot game, Combatant source, Vector3 origin, Vector3 direction)
        {
            world=game;owner=source;start=origin+Vector3.up;target=GardenArena.Clamp(origin+direction*3.4f);born=Time.time;
            model=new GameObject("Three peel petals").transform; model.SetParent(transform,false);
            for(int i=0;i<3;i++)
            {
                float a=i*120*Mathf.Deg2Rad;
                var petal=Art.Shape("Peel",PrimitiveType.Capsule,model,new Vector3(Mathf.Sin(a)*.25f,.09f,Mathf.Cos(a)*.25f),new Vector3(.19f,.09f,.65f),Art.Yellow);
                petal.transform.localRotation=Quaternion.Euler(0,i*120,55);
            }
            marker=Art.Ring("Peel landing zone",world.Transients,.85f,.055f,Art.Yellow); marker.transform.position=target;
        }
        void Update()
        {
            if(world.Paused) return;
            float age=Time.time-born;
            if(age<.38f) { float t=age/.38f;transform.position=Vector3.Lerp(start,target,t)+Vector3.up*Mathf.Sin(t*Mathf.PI)*1.35f;model.Rotate(0,500*Time.deltaTime,0); return; }
            if(!Landed)world.Fx.PeelBloom(target);
            Landed=true;transform.position=target;
            if(age>6.5f){Destroy(gameObject);return;}
            if(world.State!=SessionState.Battle && world.State!=SessionState.Practice) return;
            for(int i=0;i<world.Actors.Count;i++)
            {
                var enemy=world.Actors[i];
                if(enemy==null || enemy.Team==owner.Team || !enemy.Alive) continue;
                if((enemy.transform.position-target).sqrMagnitude<1.15f)
                {
                    enemy.Damage(20,owner,Vector3.zero,1.65f);
                    world.Fx.Burst(target+Vector3.up*.4f,Art.Yellow,6);world.Sound.Play(SoundCue.Slip);Destroy(gameObject);break;
                }
            }
        }
        void OnDestroy(){if(marker!=null)Destroy(marker);}
    }
}
