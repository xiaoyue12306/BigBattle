using UnityEngine;

namespace BigBattle
{
    public sealed class LaneMinion : Combatant
    {
        LaneMatch match;float lateral,ready,contact;Combatant victim;bool striking;public int Kind {get;private set;}
        public override int VisualStyle=>6;public override float BaseAttackDamage=>Kind==2?12:7;
        public void Setup(GameRoot world,LaneMatch mode,int team,float lane,int kind=0)
        {match=mode;lateral=lane;Kind=kind;float growth=Mathf.Clamp01(mode.Elapsed/900);AttackScale=1+growth*.5f;Initialize(world,team,(kind==2?105:kind==1?38:55)*(1+growth*.8f),6);Radius=kind==2?.4f:.3f;ready=Time.time+.3f;}
        void Update()
        {
            if(World.Paused || match.Finished || !match.Autonomous || !Alive)return;if(Controlled){striking=false;Visual.Animate(0,false);return;}
            var target=World.NearestEnemy(this,4);if(target==null)target=match.Goal(Team);if(target==null)return;Vector3 delta=target.transform.position-transform.position;delta.y=0;
            if(delta.sqrMagnitude>.01f)transform.rotation=Quaternion.Slerp(transform.rotation,Quaternion.LookRotation(delta),Time.deltaTime*14);
            float range=Kind>0?4:1.1f;
            if(striking && Time.time>=contact){striking=false;if(victim!=null && victim.Alive && Vector3.Distance(victim.transform.position,transform.position)<range+.6f+victim.Radius){if(Kind>0)LaneBolt.Spawn(World,this,victim,BaseAttackDamage*AttackScale);else victim.Damage(BaseAttackDamage*AttackScale,this,Vector3.zero,0);}}
            bool move=delta.magnitude>range+target.Radius;
            if(move){Vector3 goal=FlowerLaneMap.ToLane(target.transform.position);if(delta.magnitude>5)goal.x=lateral;goal=match.Steer(this,FlowerLaneMap.ToWorld(goal));transform.position=World.Clamp(transform.position+(goal-transform.position).normalized*2.65f*MovementScale*Time.deltaTime);}
            else if(Time.time>=ready){ready=Time.time+1;contact=Time.time+.22f;striking=true;victim=target;Visual.Swing();}Visual.Animate(move?1:0,false);
        }
    }
}
