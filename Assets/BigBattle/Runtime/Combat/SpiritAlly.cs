using UnityEngine;

namespace BigBattle
{
    public sealed class SpiritAlly : Combatant
    {
        public YueyuePlayer Owner {get;private set;}
        public float ExpiresAt {get;private set;}
        int style;float damage,nextAttack,contactAt;Combatant target;bool winding;
        public override int VisualStyle=>style;
        public override float BaseAttackDamage=>damage;
        public void Setup(GameRoot world,YueyuePlayer owner,FallenRecord record)
        {
            Owner=owner;style=record.Style;damage=record.Damage*.5f;ExpiresAt=Time.time+20;
            Initialize(world,owner.Team,record.Health*.5f,style);
            // Fixed snapshot: never halve again when a returned spirit defeats a foe.
            AttackScale=style==3?damage/28:damage/24;
            var aura=new GameObject("Teal returned-spirit aura");aura.transform.SetParent(transform,false);aura.AddComponent<SpiritAura>().Setup(this);
            nextAttack=Time.time+.5f;
        }
        void Update()
        {
            if(World.Paused)return;
            if(Owner==null || !Owner.Alive || Time.time>=ExpiresAt){World.Actors.Remove(this);Destroy(gameObject);return;}
            if(!Alive || Controlled || (World.State!=SessionState.Battle && World.State!=SessionState.Practice)){winding=false;return;}
            target=World.NearestEnemy(this,11);
            Vector3 goal=target!=null?target.transform.position:Owner.transform.position-Owner.Facing*1.5f;
            Vector3 delta=goal-transform.position;delta.y=0;
            if(delta.sqrMagnitude>.01f)transform.rotation=Quaternion.Slerp(transform.rotation,Quaternion.LookRotation(delta),Time.deltaTime*16);
            float range=style==3?6:style==1?1.35f:1.8f;
            bool move=delta.magnitude>(target!=null?range:1.2f);
            if(move)transform.position=GardenArena.Clamp(transform.position+delta.normalized*3.6f*Time.deltaTime);
            if(winding && Time.time>=contactAt)
            {
                winding=false;
                if(target!=null)World.HitArea(this,transform.position,1.45f,transform.forward,.1f,damage,.08f);
                World.Fx.Circle(transform.position,1.2f,GhostArt.Teal,.2f);
            }
            if(target!=null && delta.magnitude<=range+.15f && Time.time>=nextAttack)
            {
                nextAttack=Time.time+(style==3?.65f:style==1?1.1f:.65f);
                if(style==3){GhostProjectile.Spawn(World,this,GhostShot.Skull,delta.normalized);Visual.PlayMotion(MotionId.Throw,.45f);}
                else if(style==0){Visual.PlayMotion(MotionId.Attack1,.34f);World.Melee.Slash(this,delta.normalized,0,.10f);}
                else{winding=true;contactAt=Time.time+.35f;Visual.Swing();}
            }
            Visual.Animate(move?1:0,false);
        }
        protected override void Die(){World.Fx.Burst(transform.position+Vector3.up,GhostArt.Teal,10);World.Actors.Remove(this);Destroy(gameObject);}
    }
    public sealed class YueyueDuelist : YueyuePlayer
    {
        public bool TrainingEnabled=true;float nextChain;
        public override void Setup(GameRoot world){SetupMage(world,1,280,true);nextChain=Time.time+2;}
        public void SetupEnemy(GameRoot world){SetupMage(world,1,220,false);nextChain=Time.time+3;}
        void Update()
        {
            if(!TrainingEnabled || World.Paused || !Alive || (World.State!=SessionState.Practice && World.State!=SessionState.Battle))return;
            Combatant target=World.NearestEnemy(this,30);if(target==null)return;
            Vector3 delta=target.transform.position-transform.position;Vector2 move=Vector2.zero;
            if(delta.magnitude>6)move=new Vector2(delta.x,delta.z).normalized*.65f;
            else if(delta.magnitude<3.5f)move=-new Vector2(delta.x,delta.z).normalized*.5f;
            Facing=delta.normalized;
            bool chain=Time.time>=nextChain;if(chain)nextChain=Time.time+10;
            // Preserve attack aim while retreating; generic movement otherwise sets facing.
            Tick(new CombatInput{Move=Vector2.zero,Attack=true,Thrust=chain});
            if(!Controlled)transform.position=GardenArena.Clamp(transform.position+new Vector3(move.x,0,move.y)*Speed*Time.deltaTime);
            Visual.Animate(move.magnitude,false);
        }
    }
}
