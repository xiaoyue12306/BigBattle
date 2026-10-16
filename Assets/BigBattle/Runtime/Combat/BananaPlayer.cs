using UnityEngine;

namespace BigBattle
{
    public sealed class BananaPlayer : Combatant
    {
        public static readonly float[] Cooldowns = { .38f, 3f, 5f, 2.2f, 10f };
        public static readonly string[] SkillNames = { "普攻", "突刺", "香蕉皮", "闪避", "旋转连斩" };
        readonly float[] readyAt = new float[5];
        readonly float[] casts = new float[5];
        public Vector3 Facing { get; private set; } = Vector3.back;
        public bool Spinning => Time.time < spinUntil;
        public float Speed = 5.1f;
        public float Travelled { get; private set; }
        public float Casts(int index) => casts[index];
        public float Remaining(int index) => Mathf.Max(0,readyAt[index]-Time.time);
        float dashUntil, dashSpeed, spinUntil, nextSpinHit;
        Vector3 dashDirection;
        int bufferedSkill=-1;
        float bufferedUntil;
        Vector3 bufferedMove;
        Transform arrow;

        public void Setup(GameRoot world)
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

        public void Tick(CombatInput input)
        {
            if (!Alive) return;
            Vector3 previous = transform.position;
            Vector3 movement = new Vector3(input.Move.x,0,input.Move.y);
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
                else {bufferedSkill=-1;if(input.Attack)Cast(0,movement);}
            }
            if (Time.time<dashUntil) transform.position=GardenArena.Clamp(transform.position+dashDirection*dashSpeed*Time.deltaTime);
            if (Spinning && Time.time>=nextSpinHit)
            {
                nextSpinHit=Time.time+.16f;
                World.HitArea(this,transform.position,2.35f,Vector3.forward,-1,13,.20f);
                World.Fx.Circle(transform.position,2.35f,Art.Cream,.18f);
            }
            transform.rotation=Quaternion.Slerp(transform.rotation,Quaternion.LookRotation(Facing),Time.deltaTime*28);
            Travelled+=Vector3.Distance(previous,transform.position);
            Visual.Animate(movement.magnitude,Spinning);
            arrow.gameObject.SetActive(!Spinning);
        }

        public bool Cast(int skill, Vector3 movement)
        {
            if (skill<0 || skill>=readyAt.Length || Remaining(skill)>.001f || Time.time<dashUntil || !Alive) return false;
            if (Spinning && skill!=3) return false;
            readyAt[skill]=Time.time+Cooldowns[skill]; casts[skill]++;
            Vector3 aim = World.Assist(this,Facing,skill==2?5.2f:3f);
            switch(skill)
            {
                case 0:
                    Visual.Swing(); World.Sound.Play(SoundCue.Slash);
                    World.HitArea(this,transform.position,1.85f,aim,.32f,24,.18f);
                    World.Fx.Slash(transform.position,aim,Art.Cream); break;
                case 1:
                    Facing=aim; dashDirection=aim; dashSpeed=17; dashUntil=Time.time+.16f;
                    InvulnerableUntil=Time.time+.10f;
                    World.HitArea(this,transform.position+aim*1.45f,1.75f,aim,-.2f,42,.45f);
                    World.Fx.Slash(transform.position+aim*.6f,aim,Art.Yellow); World.Sound.Play(SoundCue.Thrust); break;
                case 2:
                    World.SpawnPeel(transform.position,aim); Visual.Swing(); World.Sound.Play(SoundCue.Toss); break;
                case 3:
                    dashDirection=movement.sqrMagnitude>.01f?movement.normalized:Facing;
                    dashSpeed=18; dashUntil=Time.time+.16f; InvulnerableUntil=Time.time+.30f;
                    World.Fx.Burst(transform.position+Vector3.up*.4f,Art.Mint,5); World.Sound.Play(SoundCue.Dodge); break;
                case 4:
                    spinUntil=Time.time+.85f; nextSpinHit=0;
                    World.Fx.Circle(transform.position,2.35f,Art.Yellow,.85f); World.Sound.Play(SoundCue.Ultimate); World.CameraRig.Shake(.06f); break;
            }
            return true;
        }

        public void ResetCombat()
        {
            for(int i=0;i<readyAt.Length;i++) readyAt[i]=0;
            dashUntil=spinUntil=0; bufferedSkill=-1; Restore(); Visual.gameObject.SetActive(true);
            Facing=Vector3.back; transform.rotation=Quaternion.LookRotation(Facing);
        }
    }
}
