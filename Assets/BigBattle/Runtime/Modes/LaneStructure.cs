using UnityEngine;

namespace BigBattle
{
    public sealed class LaneStructure : Combatant
    {
        LaneMatch match;int order;float nextAttack;GameObject shield;Combatant lockTarget;int ramp;
        public bool Core {get;private set;}public bool Inhibitor=>order==2 && !Core;
        public override bool IsStructure=>true;public override int VisualStyle=>Inhibitor?9:Core?5:4;public override float BaseAttackDamage=>Core || Inhibitor?0:22;
        public bool Protected
        {
            get
            {
                if(match==null)return true;for(int i=0;i<Mathf.Min(order,2);i++)if(match.Towers[Team,i]!=null && match.Towers[Team,i].Alive)return true;
                if(order>=3 && match.Inhibitors[Team]!=null && match.Inhibitors[Team].Alive)return true;
                if(Core)for(int i=0;i<2;i++)if(match.NexusTowers[Team,i]!=null && match.NexusTowers[Team,i].Alive)return true;return false;
            }
        }
        public void Setup(GameRoot world,LaneMatch mode,int team,bool core,int index)
        {
            match=mode;order=index;Core=core;Initialize(world,team,core?1800:Inhibitor?600:900,VisualStyle);Radius=core?1.2f:.85f;
            shield=Art.Ring("Protected crystal shield",transform,core?2.1f:1.3f,.065f,new Color(.25f,.65f,.9f));shield.SetActive(Protected);nextAttack=Time.time+1;
        }
        public override bool Damage(float amount,Combatant source,Vector3 push,float stun=.08f)
        {if(match.Finished || Protected || World.State!=SessionState.Battle)return false;float siege=match.MinionSupport(source!=null?source.Team:1-Team,transform.position)?1:.2f;return base.Damage(amount*siege,source,Vector3.zero,0);}
        void Update()
        {
            if(World.Paused || match.Finished)return;shield.SetActive(Protected);if(Core || Inhibitor || !match.Autonomous || Time.time<nextAttack)return;
            Combatant target=null;float distance=7;int priority=99;
            foreach(var actor in World.Actors)
            {
                if(actor==null || actor.IsStructure || actor.Team==Team || !actor.Alive || !actor.gameObject.activeInHierarchy || !match.VisibleTo(Team,actor))continue;
                float d=Vector3.Distance(transform.position,actor.transform.position);if(d>7)continue;
                int rank=match.ProvokedTower(Team,actor)?0:actor is LaneMinion?1:2;
                if(rank<priority || rank==priority && d<distance){target=actor;distance=d;priority=rank;}
            }
            if(target==null){lockTarget=null;ramp=0;return;}ramp=target==lockTarget?Mathf.Min(4,ramp+1):0;lockTarget=target;nextAttack=Time.time+1.05f;LaneBolt.Spawn(World,this,target,22*(target is BananaPlayer?1+ramp*.22f:1));World.Sound.Play(SoundCue.Toss,.3f);
        }
        protected override void Die(){match.StructureDown(this);gameObject.SetActive(false);if(!Inhibitor){World.Actors.Remove(this);Destroy(gameObject);}}
    }
    public sealed class LaneBolt : MonoBehaviour
    {
        GameRoot world;Combatant source,target;float damage,born;
        public static void Spawn(GameRoot world,Combatant source,Combatant target,float damage)
        {
            if(world.Transients.GetComponentsInChildren<LaneBolt>().Length>=96)return;
            var go=new GameObject("Ice bridge magic projectile");go.transform.SetParent(world.Transients);go.transform.position=source.transform.position+Vector3.up*(source.IsStructure?2.5f:.8f);
            var b=go.AddComponent<LaneBolt>();b.world=world;b.source=source;b.target=target;b.damage=damage;b.born=Time.time;Art.Shape("Crystal bolt",PrimitiveType.Sphere,go.transform,Vector3.zero,Vector3.one*.23f,source.Team==0?new Color(.2f,.7f,1):Art.Coral);
        }
        void Update()
        {
            if(world.Paused)return;if(!world.IsLane || world.Lane.Finished || target==null || source==null || !target.Alive || !target.gameObject.activeInHierarchy || Time.time-born>2){Destroy(gameObject);return;}
            Vector3 goal=target.transform.position+Vector3.up*.85f;transform.position=Vector3.MoveTowards(transform.position,goal,18*Time.deltaTime);
            if(Vector3.Distance(transform.position,goal)<.12f){target.Damage(damage,source,Vector3.zero,0);world.Fx.Burst(goal,source.Team==0?new Color(.2f,.7f,1):Art.Coral,4);Destroy(gameObject);}
        }
    }
}
