using System.Collections.Generic;
using UnityEngine;

namespace BigBattle
{
    public enum GhostShot { Skull,Chain,Doom }
    public sealed class GhostProjectile : MonoBehaviour
    {
        public GhostShot Kind {get;private set;}
        public bool Flying {get;private set;}
        public int HitCount {get;private set;}
        GameRoot world;Combatant source,target;Vector3 aim,origin;float born,speed,nextCrack;LineRenderer chain;Transform core;
        readonly HashSet<Combatant> hit=new HashSet<Combatant>();
        public static GhostProjectile Spawn(GameRoot world,Combatant source,GhostShot kind,Vector3 direction)
        {
            if(world.Transients.GetComponentsInChildren<GhostProjectile>().Length>=48)return null;
            var go=new GameObject("Ghost projectile • "+kind);go.transform.SetParent(world.Transients);var shot=go.AddComponent<GhostProjectile>();shot.Setup(world,source,kind,direction);return shot;
        }
        void Setup(GameRoot game,Combatant owner,GhostShot kind,Vector3 direction)
        {
            world=game;source=owner;Kind=kind;aim=direction.normalized;born=Time.time;origin=owner.transform.position;
            transform.position=origin+Vector3.up*.95f+aim*.65f;transform.rotation=Quaternion.LookRotation(aim);
            core=new GameObject("Flight art").transform;core.SetParent(transform,false);
            if(kind==GhostShot.Skull){GhostArt.Skull(core);SoulVfx.Attach(core,world,SoulEffect.Skull);target=world.FrontTarget(owner,aim,12,.45f);}
            if(kind==GhostShot.Chain)
            {
                chain=GhostArt.Line(transform,"Outbound iron chain",GhostArt.Teal,.075f,25);
                for(int i=0;i<12;i++){var link=Art.Ring("Iron oval",core,.15f,.045f,new Color(.13f,.22f,.24f));link.transform.localPosition=Vector3.back*i*.25f;link.transform.localScale=new Vector3(.65f,1,1);link.transform.localRotation=Quaternion.Euler(i%2==0?90:0,0,0);}
                Art.Shape("Hook point",PrimitiveType.Cube,core,Vector3.forward*.18f,new Vector3(.15f,.15f,.55f),GhostArt.Teal);
                SoulVfx.Attach(core,world,SoulEffect.Chain);
            }
            if(kind==GhostShot.Doom)
            {
                Color[] colors={GhostArt.Dark,GhostArt.Teal,GhostArt.Blue,GhostArt.Red};
                for(int i=0;i<4;i++){var blade=Art.Crescent(core,colors[i],240,i==0?.48f:.16f);blade.GetComponent<Renderer>().sharedMaterial.SetFloat("_Progress",.5f);blade.transform.localPosition=Vector3.back*i*.15f;blade.transform.localScale=Vector3.one*(2.8f-i*.19f);blade.transform.localRotation=Quaternion.Euler(-90,0,i*9-14);}
                SoulVfx.Attach(core,world,SoulEffect.Doom);
            }
        }
        void Update()
        {
            if(world.Paused)return;
            if(source==null || !source.Alive || Time.time-born> (Kind==GhostShot.Doom?3:Kind==GhostShot.Chain?.75f:3.5f)){Destroy(gameObject);return;}
            float age=Time.time-born;
            if(Kind==GhostShot.Skull && age<.35f){transform.position=source.transform.position+aim*.58f+Vector3.up*(1.13f+Mathf.Sin(age*15)*.12f)+Vector3.Cross(Vector3.up,aim)*.40f;return;}
            Flying=true;
            if(Kind==GhostShot.Skull && target!=null && target.Alive){Vector3 seek=target.transform.position+Vector3.up*.9f-transform.position;aim=Vector3.RotateTowards(aim,seek.normalized,Time.deltaTime*2.3f,0);}
            speed=Kind==GhostShot.Doom?24:Kind==GhostShot.Chain?19:Mathf.Min(18,3+(age-.35f)*24);
            Vector3 from=transform.position,to=from+aim*speed*Time.deltaTime;transform.position=to;transform.rotation=Quaternion.LookRotation(aim);
            if(chain!=null){Vector3 start=source.transform.position+Vector3.up;for(int j=0;j<25;j++){float t=j/24f;Vector3 p=Vector3.Lerp(start,to,t)+Vector3.up*Mathf.Sin(t*Mathf.PI)*-.12f;chain.SetPosition(j,transform.InverseTransformPoint(p));}}
            if(Kind==GhostShot.Doom && Time.time>=nextCrack)
            {
                nextCrack=Time.time+.065f;Crack(new Vector3(to.x,.025f,to.z));
            }
            Combatant first=null;float firstAt=float.PositiveInfinity;
            foreach(var actor in world.Actors.ToArray())
            {
                if(actor==null || !actor.Alive || actor.Team==source.Team || hit.Contains(actor))continue;
                Vector3 p=actor.transform.position+Vector3.up*.9f;Vector3 d=to-from;float t=Mathf.Clamp01(Vector3.Dot(p-from,d)/Mathf.Max(.0001f,d.sqrMagnitude));
                float r=(Kind==GhostShot.Doom?2.1f:.24f)+actor.Radius;
                if((p-Vector3.Lerp(from,to,t)).sqrMagnitude>r*r)continue;
                if(Kind==GhostShot.Doom)Strike(actor);else if(t<firstAt){first=actor;firstAt=t;}
            }
            if(first!=null){Strike(first);Destroy(gameObject);}
        }
        void Strike(Combatant actor)
        {
            hit.Add(actor);float damage=Kind==GhostShot.Doom?500:Kind==GhostShot.Chain?12:28;
            if(actor.Damage(damage*source.AttackScale,source,Kind==GhostShot.Chain?Vector3.zero:aim*.18f,0))HitCount++;
            if(Kind==GhostShot.Chain && actor.Bind(2))
            {
                var cage=new GameObject("Void chains • two second bind").AddComponent<SoulCage>();cage.transform.SetParent(world.Transients);cage.Setup(actor);world.Sound.Play(SoundCue.Clash,.7f);
            }
            world.Fx.Burst(actor.transform.position+Vector3.up,GhostArt.Teal,Kind==GhostShot.Doom?15:6);
            SoulVfx.Burst(world,SoulEffect.Impact,actor.transform.position+Vector3.up,aim);
        }
        void Crack(Vector3 point)
        {
            // Limit persistent ground marks independently of the short hit-particle budget.
            if(world.Fx.transform.childCount>=Feedback.EffectLimit-1)return;
            var root=new GameObject("Doom fractured ground");root.transform.SetParent(world.Fx.transform);root.transform.position=point;root.transform.rotation=Quaternion.LookRotation(aim);
            var dark=GhostArt.Line(root.transform,"Black fissure",GhostArt.Dark,.12f,7);var glow=GhostArt.Line(root.transform,"Red green fissure",GhostArt.Teal,.035f,7);
            for(int j=0;j<7;j++){Vector3 p=new Vector3(Mathf.Sin(j*11.8f+point.x)*.65f,0,(j-3)*.18f);dark.SetPosition(j,p);glow.SetPosition(j,p+Vector3.up*.006f);}
            root.AddComponent<TransientVisual>().Setup(4,1);
        }
    }
}
