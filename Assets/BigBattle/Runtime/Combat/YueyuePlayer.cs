using UnityEngine;

namespace BigBattle
{
    public struct FallenRecord
    {
        public bool Valid;public int Style;public float Health,Damage;public string Name;
    }
    public class YueyuePlayer : BananaPlayer
    {
        static readonly string[] Names={"追魂骷髅","幽冥铁索","亡者归来","虚无鬼影","万魂魔剑"};
        static readonly float[] Recovery={.65f,10,25,8,20};
        readonly float[] ready=new float[5],uses=new float[5];
        public override string HeroName=>"越越大王";
        public override string[] ActionNames=>Names;
        public override float Cooldown(int i)=>Recovery[i];
        public override float Remaining(int i)=>Mathf.Max(0,ready[i]-Time.time);
        public override float Casts(int i)=>uses[i];
        public override float BaseAttackDamage=>28;
        public int Souls {get;private set;}
        public FallenRecord Fallen {get;private set;}
        public SpiritAlly Servant {get;private set;}
        public bool Charging {get;private set;}
        public float ChargeProgress=>Charging?Mathf.Clamp01((Time.time-chargeAt)/4):0;
        public bool Phasing=>Time.time<phaseUntil;
        public int ReleasedWaves {get;private set;}
        float chargeAt,phaseUntil;bool requireRelease;
        YueyueVisual rig;
        public override string ActionHint(int i)
        {
            if(i==4)return Charging?(ChargeProgress>=1?"松开 · 斩！":"蓄力 "+(ChargeProgress*4).ToString("0.0")+" / 4秒"):"魂印 "+Souls+" / 3";
            if(i==2)return Fallen.Valid?"唤回 · "+Fallen.Name:"尚无亡魂";
            if(i==3 && Phasing)return "虚无无敌";
            return i==0?"按住连发":"就绪";
        }
        public override void Setup(GameRoot world){SetupMage(world,0,155,false);}
        public override void SetupTeam(GameRoot world,int team){SetupMage(world,team,155,false);}
        public override void RespawnAt(Vector3 point,Vector3 direction){InterruptCharge();phaseUntil=0;requireRelease=true;base.RespawnAt(point,direction);if(rig!=null)rig.ResetPose();}
        protected void SetupMage(GameRoot world,int team,float health,bool dummy)
        {
            Initialize(world,team,health,3,dummy);Speed=4.8f;
            rig=Visual.GetComponent<YueyueVisual>();
            Art.Ring("Yueyue spectral ring",transform,.68f,.055f,team==0?GhostArt.Teal:Art.Coral);
            GhostArt.Pointer(transform,team==0?GhostArt.Teal:Art.Coral);
            Facing=Vector3.back;transform.rotation=Quaternion.LookRotation(Facing);
        }
        public void EarnDefeat(Combatant victim)
        {
            if(victim==null || victim.IsDummy || victim.Team==Team || victim is SpiritAlly)return;
            Souls=Mathf.Min(99,Souls+1);
            Fallen=new FallenRecord{Valid=true,Style=victim.VisualStyle,Health=victim.MaxHealth,Damage=victim.BaseAttackDamage*victim.AttackScale,Name=victim.VisualStyle==8?"阿狸":victim.VisualStyle==3?"越越大王":victim.VisualStyle==0?"香蕉刺客":victim.VisualStyle==6?"花灵":"团子"};
            if(this==World.Player)World.Fx.Notice(Souls>=3?"三枚魂印已齐 · 按住 I / Y 蓄力魔剑":"收获魂印 · "+Souls+" / 3");
        }
        public void FillPracticeSouls(){if(World.State==SessionState.Practice){Souls=Mathf.Max(3,Souls);ready[4]=0;World.Fx.Notice("练习魂印已充满 · 按住 I / Y 四秒，再松开");}}
        public override void Tick(CombatInput input)
        {
            if(!Alive)return;
            Vector3 move=new Vector3(input.Move.x,0,input.Move.y);
            bool held=input.UltimateHeld || input.Ultimate;
            if(!held)requireRelease=false;
            if(input.PracticeCharge && this==World.Player)FillPracticeSouls();
            if(Charging)
            {
                if(Time.time<BoundUntil){InterruptCharge();return;}
                if(move.sqrMagnitude>.01f)Facing=move.normalized;
                if(!Controlled)transform.position=World.Clamp(transform.position+move*Speed*.1f*Time.deltaTime);
                if(!held){if(ChargeProgress>=1)ReleaseSword();else InterruptCharge();}
            }
            else
            {
                if(!Controlled && move.sqrMagnitude>.01f){Facing=move.normalized;transform.position=World.Clamp(transform.position+move*Speed*Time.deltaTime);}
                if(!Phasing && !Controlled)
                {
                    if(input.Dodge)Cast(3,move);
                    else if(held && !requireRelease)Cast(4,move);
                    else if(input.Thrust)Cast(1,move);
                    else if(input.Peel)Cast(2,move);
                    else if(input.Attack)Cast(0,move);
                }
                // Dodge can cleanse an existing bind, including on the frame it lands.
                else if(input.Dodge && !Phasing)Cast(3,move);
            }
            transform.rotation=Quaternion.Slerp(transform.rotation,Quaternion.LookRotation(Facing),1-Mathf.Exp(-Time.deltaTime*24));
            Visual.Animate(Controlled?0:move.magnitude,false);
            if(rig!=null)rig.Present(Charging,ChargeProgress,Phasing);
        }
        public override bool Cast(int skill,Vector3 movement)
        {
            if(skill<0 || skill>4 || !Alive || Remaining(skill)>.001f || Phasing || Charging || (Controlled && skill!=3))return false;
            if(skill==2 && !Fallen.Valid)return false;
            if(skill==4 && (Souls<3 || requireRelease))return false;
            if(skill==4){Charging=true;chargeAt=Time.time;World.Sound.Play(SoundCue.Ultimate,.6f);if(rig!=null)rig.Present(true,0,false);return true;}
            ready[skill]=Time.time+Recovery[skill];uses[skill]++;
            if(rig!=null)rig.PlayCast(skill);
            if(skill==0){GhostProjectile.Spawn(World,this,GhostShot.Skull,Facing);Visual.PlayMotion(MotionId.Throw,.42f);World.Sound.Play(SoundCue.Toss,.8f);}
            if(skill==1){GhostProjectile.Spawn(World,this,GhostShot.Chain,Facing);Visual.PlayMotion(MotionId.Thrust,.45f);World.Sound.Play(SoundCue.Clash,.55f);}
            if(skill==2)
            {
                if(Servant!=null){World.Actors.Remove(Servant);Destroy(Servant.gameObject);}
                Servant=new GameObject("Returned spirit • "+Fallen.Name).AddComponent<SpiritAlly>();Servant.transform.SetParent(World.Transients);
                Servant.transform.position=World.Clamp(transform.position-Facing*.9f+Vector3.right*.6f);Servant.Setup(World,this,Fallen);
                World.Fx.Burst(Servant.transform.position+Vector3.up, GhostArt.Teal,14);World.Sound.Play(SoundCue.Ultimate,.65f);
                SoulVfx.Burst(World,SoulEffect.Summon,Servant.transform.position);
            }
            if(skill==3){Cleanse();phaseUntil=Time.time+3;InvulnerableUntil=phaseUntil;World.Melee.Cancel(this);World.Sound.Play(SoundCue.Dodge);if(rig!=null)rig.Present(false,0,true);}
            return true;
        }
        public void InterruptCharge()
        {
            if(!Charging)return;Charging=false;requireRelease=true;if(rig!=null)rig.Present(false,0,Phasing);
        }
        void ReleaseSword()
        {
            Charging=false;requireRelease=true;Souls-=3;ready[4]=Time.time+20;uses[4]++;ReleasedWaves++;
            GhostProjectile.Spawn(World,this,GhostShot.Doom,Facing);Visual.PlayMotion(MotionId.Attack3,.7f);
            SoulVfx.Burst(World,SoulEffect.Release,transform.position+Vector3.up*.5f,Facing);
            if(rig!=null)rig.Release();
            World.CameraRig.DoomImpact();World.CameraRig.Impact(Facing,2.5f);World.Input.Rumble(.95f,.48f);
            World.Sound.Play(SoundCue.Ultimate,1.3f);World.Sound.Impact(true);World.Fx.Notice("万魂归剑 · 斩！");World.Fx.DoomFlashUntil=Time.unscaledTime+.24f;
        }
        public override void ResetCombat()
        {
            base.ResetCombat();for(int i=0;i<5;i++){ready[i]=uses[i]=0;}Charging=false;requireRelease=false;phaseUntil=0;Souls=0;Fallen=default(FallenRecord);Servant=null;
            if(rig!=null)rig.ResetPose();
        }
        protected override void Die(){InterruptCharge();base.Die();}
    }
}
