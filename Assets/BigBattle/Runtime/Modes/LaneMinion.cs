using UnityEngine;

namespace BigBattle
{
    public sealed class LaneMinion : Combatant
    {
        LaneMatch match;float lateral,ready,contact;Combatant victim;bool striking;
        public override int VisualStyle=>6;
        public override float BaseAttackDamage=>7;
        public void Setup(GameRoot world,LaneMatch mode,int team,float lane)
        {
            match=mode;lateral=lane;Initialize(world,team,45,6);Visual.SetAccent(team==0?Art.Mint:Art.Coral);Radius=.30f;
            Art.Ring("Flower sprout team marker",transform,.34f,.04f,team==0?Art.Mint:Art.Coral);ready=Time.time+.3f;
        }
        void Update()
        {
            if(World.Paused || match.Finished || !match.Autonomous || !Alive)return;
            if(Controlled){striking=false;Visual.Animate(0,false);return;}
            var target=World.NearestEnemy(this,4);if(target==null)target=match.Goal(Team);if(target==null)return;
            Vector3 delta=target.transform.position-transform.position;delta.y=0;
            if(delta.sqrMagnitude>.01f)transform.rotation=Quaternion.Slerp(transform.rotation,Quaternion.LookRotation(delta),Time.deltaTime*14);
            if(striking && Time.time>=contact)
            {
                striking=false;if(victim!=null && victim.Alive && Vector3.Distance(victim.transform.position,transform.position)<1.7f+victim.Radius)victim.Damage(7,this,Vector3.zero,.03f);
            }
            bool move=delta.magnitude>1.1f+target.Radius;
            if(move){Vector3 goal=FlowerLaneMap.ToLane(target.transform.position);if(delta.magnitude>5)goal.x=lateral;goal=FlowerLaneMap.ToWorld(goal);Vector3 velocity=(goal-transform.position).normalized;transform.position=World.Clamp(transform.position+velocity*2.65f*Time.deltaTime);}
            else if(Time.time>=ready){ready=Time.time+.95f;contact=Time.time+.25f;striking=true;victim=target;Visual.Swing();}
            Visual.Animate(move?1:0,false);
        }
    }
}
