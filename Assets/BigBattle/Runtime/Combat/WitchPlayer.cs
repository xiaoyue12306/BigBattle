using UnityEngine;

namespace BigBattle
{
    // Character preview only. No inherited banana attacks or invented witch abilities.
    public sealed class WitchPlayer : BananaPlayer
    {
        static readonly string[] Pending={"普攻待设计","技能待设计","技能待设计","技能待设计","大招待设计"};
        public override string HeroName=>"女巫";
        public override string[] ActionNames=>Pending;
        public override int VisualStyle=>7;
        public override float BaseAttackDamage=>0;
        public override float Cooldown(int i)=>0;
        public override float Remaining(int i)=>0;
        public override float Casts(int i)=>0;
        public override string ActionHint(int i)=>"尚未开放";
        public override void Setup(GameRoot world){Initialize(world,0,100,7);Speed=4.5f;Art.Ring("Witch preview selection",transform,.58f,.045f,new Color(.67f,.53f,.83f));Facing=Vector3.back;transform.rotation=Quaternion.LookRotation(Facing);}
        public override void Tick(CombatInput input)
        {
            if(!Alive)return;Vector3 move=new Vector3(input.Move.x,0,input.Move.y);
            if(move.sqrMagnitude>.001f)Facing=move.normalized;
            if(!Controlled)transform.position=World.Clamp(transform.position+move*Speed*Time.deltaTime);
            transform.rotation=Quaternion.Slerp(transform.rotation,Quaternion.LookRotation(Facing),1-Mathf.Exp(-Time.deltaTime*15));Visual.Animate(move.magnitude,false);
        }
        public override bool Cast(int skill,Vector3 movement)=>false;
        public override bool Damage(float amount,Combatant source,Vector3 push,float stun=.08f)=>false;
    }
}
