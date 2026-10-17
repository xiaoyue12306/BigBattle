using System.Collections.Generic;
using UnityEngine;

namespace BigBattle
{
    // Announce during windup; resolve after every actor's Update. Clashes consume
    // both strikes before either can deal damage, regardless of registration order.
    public sealed class MeleeResolver
    {
        public const float ClashWindow=.075f;
        sealed class Strike
        {
            public Combatant Source;public Vector3 Aim,Origin;public float At;public int Combo;
            public bool Thrust,Consumed;
        }
        readonly List<Strike> strikes=new List<Strike>();
        readonly GameRoot world;
        public int ClashCount {get;private set;}
        public int PendingCount=>strikes.Count;
        public MeleeResolver(GameRoot game){world=game;}
        public void Slash(Combatant source,Vector3 aim,int combo,float delay)
        {Register(source,aim,combo,delay,false,source.transform.position);}
        public void Thrust(Combatant source,Vector3 aim,Vector3 origin,float delay)
        {Register(source,aim,2,delay,true,origin);}
        void Register(Combatant source,Vector3 aim,int combo,float delay,bool thrust,Vector3 origin)
        {
            Cancel(source);
            strikes.Add(new Strike{Source=source,Aim=aim.normalized,Combo=combo,At=Time.time+delay,Thrust=thrust,Origin=origin});
        }
        public void Cancel(Combatant source){foreach(var strike in strikes)if(strike.Source==source)strike.Consumed=true;}
        public void Clear(){strikes.Clear();}
        static bool Valid(Strike strike)=>!strike.Consumed && strike.Source!=null && strike.Source.Alive && strike.Source.isActiveAndEnabled && Time.time>=strike.Source.StunnedUntil;
        static bool Meets(Strike a,Strike b)
        {
            if(a.Source.Team==b.Source.Team || Mathf.Abs(a.At-b.At)>ClashWindow)return false;
            Vector3 gap=b.Source.transform.position-a.Source.transform.position;gap.y=0;
            if(gap.sqrMagnitude<.04f || gap.sqrMagnitude>2.3f*2.3f)return false;
            Vector3 axis=gap.normalized;
            return Vector3.Dot(a.Aim,axis)>.35f && Vector3.Dot(b.Aim,-axis)>.35f && Vector3.Dot(a.Aim,b.Aim)<-.15f;
        }
        public void Tick()
        {
            if(world.Paused)return;
            // Find all reciprocal contacts before resolving any damage.
            for(int i=0;i<strikes.Count;i++)
            {
                var a=strikes[i];if(!Valid(a) || a.At>Time.time)continue;
                for(int j=0;j<strikes.Count;j++)
                {
                    var b=strikes[j];if(i==j || !Valid(b) || !Meets(a,b))continue;
                    a.Consumed=b.Consumed=true;ClashCount++;
                    Vector3 contact=(a.Source.transform.position+b.Source.transform.position)*.5f+Vector3.up*1.05f;
                    world.Fx.Slash(a.Source.transform.position,a.Aim,Art.Mint,a.Combo);
                    world.Fx.Slash(b.Source.transform.position,b.Aim,Art.Coral,b.Combo);
                    world.Fx.Clash(contact);world.Sound.Play(SoundCue.Clash);
                    a.Source.WeaponRecoil(-a.Aim);b.Source.WeaponRecoil(-b.Aim);
                    world.HitPause(true);world.CameraRig.Impact(a.Aim,.8f);world.Input.Rumble(.5f,.09f);
                    break;
                }
            }
            foreach(var strike in strikes)
            {
                if(!Valid(strike) || strike.At>Time.time)continue;
                strike.Consumed=true;
                float damage=strike.Combo==2?36:strike.Combo==1?27:24;
                if(strike.Thrust)world.HitCapsule(strike.Source,strike.Origin,strike.Source.transform.position+strike.Aim*.95f,.85f,42,.55f);
                else world.HitArea(strike.Source,strike.Source.transform.position,1.85f,strike.Aim,.32f,damage,strike.Combo==2?.45f:.18f);
                if(strike.Thrust)world.Fx.Thrust(strike.Origin,strike.Source.transform.position+strike.Aim*.95f,strike.Aim);
                else world.Fx.Slash(strike.Source.transform.position,strike.Aim,strike.Source==world.Player?Art.Mint:Art.Coral,strike.Combo);
            }
            strikes.RemoveAll(strike=>!Valid(strike));
        }
    }
}
