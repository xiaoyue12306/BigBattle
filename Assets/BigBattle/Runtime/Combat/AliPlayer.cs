using UnityEngine;

namespace BigBattle
{
    public class AliPlayer : BananaPlayer
    {
        static readonly string[] Names={"爆泡连锁","未配置","未配置","未配置","未配置"};
        public const float PlacementInterval=.22f;
        float ready,uses;AliVisual rig;
        public override string HeroName=>"阿狸";
        public override int VisualStyle=>8;
        public override float BaseAttackDamage=>80;
        public override string[] ActionNames=>Names;
        public override float Cooldown(int i)=>i==0?PlacementInterval:0;
        public override float Remaining(int i)=>i==0?Mathf.Max(0,ready-Time.time):0;
        public override float Casts(int i)=>i==0?uses:0;
        public int PendingBubbles=>World.Bubbles.PendingFor(this);
        public override string ActionHint(int i)=>i==0?"在场 "+PendingBubbles+" / 5":"尚未配置";
        public override void Setup(GameRoot world){SetupTeam(world,0);}
        public override void SetupTeam(GameRoot world,int team)
        {SetupAli(world,team,180,false);}
        protected void SetupAli(GameRoot world,int team,float health,bool dummy)
        {
            Initialize(world,team,health,8,dummy);Speed=5;rig=Visual.GetComponent<AliVisual>();
            Art.Ring("Ali team ring",transform,.62f,.055f,team==0?Art.Mint:Art.Coral);GhostArt.Pointer(transform,team==0?Art.Yellow:Art.Coral);
            Facing=Vector3.back;transform.rotation=Quaternion.LookRotation(Facing);
        }
        public override void Tick(CombatInput input)
        {
            if(!Alive)return;Vector3 move=new Vector3(input.Move.x,0,input.Move.y);
            if(!Controlled)
            {
                if(move.sqrMagnitude>.001f)Facing=move.normalized;
                transform.position=World.Clamp(transform.position+move*Speed*Time.deltaTime);
                if(input.Attack)Cast(0,move);
            }
            transform.rotation=Quaternion.Slerp(transform.rotation,Quaternion.LookRotation(Facing),1-Mathf.Exp(-Time.deltaTime*18));Visual.Animate(Controlled?0:move.magnitude,false);
        }
        public override bool Cast(int skill,Vector3 movement)
        {
            if(skill!=0 || !Alive || Controlled || Remaining(0)>0 || World.Paused || (World.State!=SessionState.Practice && World.State!=SessionState.Battle))return false;
            if(!World.Bubbles.Place(this,transform.position,BaseAttackDamage*AttackScale))return false;
            ready=Time.time+PlacementInterval;uses++;if(rig!=null)rig.PlayPlace();return true;
        }
        public override void ResetCombat(){base.ResetCombat();ready=uses=0;if(rig!=null)rig.ResetPose();}
        public override void RespawnAt(Vector3 point,Vector3 direction){base.RespawnAt(point,direction);if(rig!=null)rig.ResetPose();}
    }

    public sealed partial class GameRoot
    {
        BubbleField bubbles;
        public BubbleField Bubbles
        {
            get
            {
                if(bubbles==null){var go=new GameObject("Session bubble combat");go.transform.SetParent(Transients);bubbles=go.AddComponent<BubbleField>();bubbles.Setup(this);}
                return bubbles;
            }
        }
    }
}
