using UnityEngine;

namespace BigBattle
{
    public sealed class LaneStructure : Combatant
    {
        LaneMatch match;int order;float nextAttack;GameObject shield;
        public bool Core {get;private set;}
        public override bool IsStructure=>true;
        public override int VisualStyle=>Core?5:4;
        public override float BaseAttackDamage=>Core?0:22;
        public bool Protected
        {
            get {if(match==null)return true;for(int i=0;i<(Core?2:order);i++)if(match.Towers[Team,i]!=null && match.Towers[Team,i].Alive)return true;return false;}
        }
        public void Setup(GameRoot world,LaneMatch mode,int team,bool core,int index)
        {
            match=mode;order=index;Core=core;Initialize(world,team,core?1800:900,core?5:4);Radius=core?1.2f:.85f;Visual.SetAccent(team==0?Art.Mint:Art.Coral);
            shield=Art.Ring("Protected flower shield",transform,core?2.1f:1.35f,.10f,new Color(.50f,.82f,1));shield.SetActive(Protected);nextAttack=Time.time+1;
        }
        public override bool Damage(float amount,Combatant source,Vector3 push,float stun=.08f)
        {
            if(match.Finished || Protected || World.State!=SessionState.Battle)return false;
            float siege=match.MinionSupport(source!=null?source.Team:1-Team,transform.position)?1:.2f;
            return base.Damage(amount*siege,source,Vector3.zero,0);
        }
        void Update()
        {
            if(World.Paused || match.Finished)return;shield.SetActive(Protected);
            if(Core || !match.Autonomous || Time.time<nextAttack)return;
            Combatant target=null;float distance=7;
            foreach(var actor in World.Actors)
            {
                if(actor==null || actor.IsStructure || actor.Team==Team || !actor.Alive || !actor.gameObject.activeInHierarchy)continue;
                float d=Vector3.Distance(transform.position,actor.transform.position);if(d>7)continue;
                if(target==null || actor is LaneMinion && !(target is LaneMinion) || (actor is LaneMinion)==(target is LaneMinion) && d<distance){target=actor;distance=d;}
            }
            if(target==null)return;nextAttack=Time.time+1.05f;LaneBolt.Spawn(World,this,target,22);World.Sound.Play(SoundCue.Toss,.32f);
        }
        protected override void Die(){match.StructureDown(this);World.Actors.Remove(this);gameObject.SetActive(false);Destroy(gameObject);}
    }
    public sealed class LaneBolt : MonoBehaviour
    {
        GameRoot world;Combatant source,target;float damage,born;
        public static void Spawn(GameRoot world,Combatant source,Combatant target,float damage)
        {
            var go=new GameObject("Guardian flower seed");go.transform.SetParent(world.Transients);go.transform.position=source.transform.position+Vector3.up*2.4f;
            var bolt=go.AddComponent<LaneBolt>();bolt.world=world;bolt.source=source;bolt.target=target;bolt.damage=damage;bolt.born=Time.time;
            Art.Shape("Warm seed light",PrimitiveType.Sphere,go.transform,Vector3.zero,Vector3.one*.22f,source.Team==0?Art.Mint:Art.Coral);
        }
        void Update()
        {
            if(world.Paused)return;
            if(!world.IsLane || world.Lane.Finished || target==null || source==null || !target.Alive || !target.gameObject.activeInHierarchy || Time.time-born>2){Destroy(gameObject);return;}
            Vector3 goal=target.transform.position+Vector3.up*.85f;transform.position=Vector3.MoveTowards(transform.position,goal,14*Time.deltaTime);
            if(Vector3.Distance(transform.position,goal)<.12f){target.Damage(damage,source,Vector3.zero,.03f);world.Fx.Burst(goal,source.Team==0?Art.Mint:Art.Coral,4);Destroy(gameObject);}
        }
    }
}
