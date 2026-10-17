using UnityEngine;

namespace BigBattle
{
    public class BananaPlayer : Combatant
    {
        public static readonly float[] Cooldowns = { .52f, 3f, 5f, 2.2f, 10f };
        public static readonly string[] ComboNames={"一式 · 起手横斩","二式 · 反手追斩","三式 · 月牙终结"};
        static readonly float[] ComboCadence={.30f,.34f,.52f};
        static readonly float[] ComboDuration={.29f,.33f,.46f};
        public static readonly string[] SkillNames = { "普攻", "突刺", "香蕉皮", "闪避", "旋转连斩" };
        readonly float[] readyAt = new float[5];
        readonly float[] casts = new float[5];
        public Vector3 Facing { get; protected set; } = Vector3.back;
        public virtual string HeroName=>"香蕉刺客";
        public virtual string[] ActionNames=>SkillNames;
        public virtual float Cooldown(int index)=>Cooldowns[index];
        public virtual string ActionHint(int index)=>index==0?"按住连击":"就绪";
        public bool Spinning => Time.time < spinUntil;
        public float Speed = 5.1f;
        public float Travelled { get; private set; }
        public virtual float Casts(int index) => casts[index];
        public virtual float Remaining(int index) => Mathf.Max(0,readyAt[index]-Time.time);
        float dashUntil, dashSpeed, spinUntil, nextSpinHit;
        Vector3 dashDirection;
        int bufferedSkill=-1;
        float bufferedUntil;
        Vector3 bufferedMove;
        Transform arrow;
        public int ComboStep {get;private set;}=-1;
        public bool ComboActive=>ComboStep>=0 && Time.time-lastCombo<1.05f;
        public readonly int[] ComboUsage=new int[3];
        float lastCombo=-100,pendingAt=float.PositiveInfinity,nextGhost,nextFootstep;
        int pendingSkill=-1;
        Vector3 pendingAim;
        float attackBufferUntil=-100;bool attackHeld;

        public virtual void Setup(GameRoot world)
        {
            Initialize(world,0,180,0);
            var selection = Art.Ring("Player mint ring",transform,.65f,.075f,Art.Mint);
            selection.transform.localPosition=Vector3.zero;
            var pointer = new GameObject("Facing arrow"); pointer.transform.SetParent(transform,false); arrow = pointer.transform;
            var mesh = new Mesh { name="Facing arrow" };
            mesh.vertices=new[]{new Vector3(-.18f,.03f,.84f),new Vector3(0,.03f,1.18f),new Vector3(.18f,.03f,.84f)};
            mesh.triangles=new[]{0,1,2}; mesh.RecalculateNormals();
            Art.MeshObject("Facing arrow mesh",arrow,mesh,Art.Mint);
            transform.rotation=Quaternion.LookRotation(Facing);
        }

        public virtual void Tick(CombatInput input)
        {
            if (!Alive) return;
            Vector3 previous = transform.position;
            Vector3 movement = new Vector3(input.Move.x,0,input.Move.y);
            if(input.Attack && !attackHeld)attackBufferUntil=Time.time+.24f;
            attackHeld=input.Attack;
            if(pendingSkill>=0 && Time.time>=pendingAt)ResolveContact();
            if (Time.time >= dashUntil && Time.time >= StunnedUntil)
            {
                if (movement.sqrMagnitude>.01f) Facing=movement.normalized;
                transform.position=GardenArena.Clamp(transform.position+movement*Speed*Time.deltaTime*(Spinning?.8f:1f));
            }
            int requested=input.Dodge?3:input.Thrust?1:input.Peel?2:input.Ultimate?4:-1;
            if(requested>=0){bufferedSkill=requested;bufferedMove=movement;bufferedUntil=Time.time+.15f;}
            if (Time.time >= StunnedUntil)
            {
                if(bufferedSkill>=0 && Time.time<=bufferedUntil)
                {
                    if(Cast(bufferedSkill,bufferedMove))bufferedSkill=-1;
                }
                else
                {
                    bufferedSkill=-1;
                    if((input.Attack || Time.time<attackBufferUntil) && Cast(0,movement))attackBufferUntil=-100;
                }
            }
            if (Time.time<dashUntil) transform.position=GardenArena.Clamp(transform.position+dashDirection*dashSpeed*Time.deltaTime);
            if(Time.time<dashUntil && Time.time>=nextGhost){nextGhost=Time.time+.045f;World.Fx.Ghost(Visual);}
            if(movement.sqrMagnitude>.1f && Time.time>=nextFootstep && Time.time>=dashUntil)
            {nextFootstep=Time.time+.22f;World.Fx.Dust(transform.position);World.Sound.Play(SoundCue.Step);}
            if (Spinning && Time.time>=nextSpinHit)
            {
                nextSpinHit=Time.time+.16f;
                World.HitArea(this,transform.position,2.35f,Vector3.forward,-1,13,.20f);
                World.Fx.WhirlBeat(transform.position);
            }
            transform.rotation=Quaternion.Slerp(transform.rotation,Quaternion.LookRotation(Facing),Time.deltaTime*28);
            Travelled+=Vector3.Distance(previous,transform.position);
            Visual.Animate(movement.magnitude,Spinning);
            arrow.gameObject.SetActive(!Spinning);
        }

        public virtual bool Cast(int skill, Vector3 movement)
        {
            if (skill<0 || skill>=readyAt.Length || Remaining(skill)>.001f || Time.time<dashUntil || Time.time<StunnedUntil || !Alive) return false;
            if (Spinning && skill!=3) return false;
            readyAt[skill]=Time.time+Cooldowns[skill]; casts[skill]++;
            Vector3 aim = World.Assist(this,Facing,skill==2?5.2f:3f);
            switch(skill)
            {
                case 0:
                    ComboStep=ComboActive?(ComboStep+1)%3:0;lastCombo=Time.time;ComboUsage[ComboStep]++;
                    readyAt[0]=Time.time+ComboCadence[ComboStep];
                    Facing=aim;Visual.PlayMotion((MotionId)((int)MotionId.Attack1+ComboStep),ComboDuration[ComboStep]);
                    World.Melee.Slash(this,aim,ComboStep,ComboDuration[ComboStep]*.27f);
                    World.Sound.Play(ComboStep==2?SoundCue.SlashFinish:ComboStep==1?SoundCue.SlashBack:SoundCue.Slash,.85f+ComboStep*.08f);break;
                case 1:
                    Facing=aim; dashDirection=aim; dashSpeed=17; dashUntil=Time.time+.16f;
                    InvulnerableUntil=Time.time+.10f;
                    World.Melee.Thrust(this,aim,transform.position,.28f*.27f);Visual.PlayMotion(MotionId.Thrust,.28f);
                    World.Sound.Play(SoundCue.Thrust); break;
                case 2:
                    World.Melee.Cancel(this);
                    QueueContact(2,aim,.40f*.27f);Visual.PlayMotion(MotionId.Throw,.40f);World.Sound.Play(SoundCue.Toss);break;
                case 3:
                    dashDirection=movement.sqrMagnitude>.01f?movement.normalized:Facing;
                    dashSpeed=18; dashUntil=Time.time+.16f; InvulnerableUntil=Time.time+.30f;
                    pendingSkill=-1;World.Melee.Cancel(this);attackBufferUntil=-100;Visual.PlayMotion(MotionId.Dodge,.28f);
                    World.Fx.Burst(transform.position+Vector3.up*.4f,Art.Mint,5); World.Sound.Play(SoundCue.Dodge); break;
                case 4:
                    spinUntil=Time.time+.85f; nextSpinHit=0;
                    pendingSkill=-1;World.Melee.Cancel(this);Visual.PlayMotion(MotionId.Spin,.85f);
                    World.Fx.Whirlwind(transform,.85f); World.Sound.Play(SoundCue.Ultimate); World.CameraRig.Shake(.06f); break;
            }
            return true;
        }
        void QueueContact(int skill,Vector3 aim,float delay){pendingSkill=skill;pendingAim=aim;pendingAt=Time.time+delay;}
        void ResolveContact()
        {
            int skill=pendingSkill;pendingSkill=-1;
            if(skill==2){World.SpawnPeel(transform.position,pendingAim);return;}
        }

        public virtual void ResetCombat()
        {
            for(int i=0;i<readyAt.Length;i++) readyAt[i]=0;
            dashUntil=spinUntil=0; bufferedSkill=-1;pendingSkill=-1;ComboStep=-1;lastCombo=-100;
            attackHeld=false;attackBufferUntil=-100;
            Restore(); Visual.gameObject.SetActive(true);Visual.ResetState();
            Facing=Vector3.back; transform.rotation=Quaternion.LookRotation(Facing);
        }
    }
}
