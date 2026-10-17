using UnityEngine;

namespace BigBattle
{
    // A single-player practice opponent using the same weapon resolver as P1.
    public sealed class BananaDuelist : Combatant
    {
        float nextAttack,contactAt;Vector3 lockedAim;int combo=-1;GameObject warning;bool released;
        public bool WindingUp {get;private set;}
        public bool TrainingEnabled=true;
        public void SetupEnemy(GameRoot world){SetupInternal(world,false);}
        public void Setup(GameRoot world)
        {SetupInternal(world,true);}
        void SetupInternal(GameRoot world,bool dummy)
        {
            Initialize(world,1,dummy?360:120,0,dummy);
            Visual.SetAccent(Art.Coral);
            var ring=Art.Ring("Coral sparring ring",transform,.68f,.075f,Art.Coral);
            ring.transform.localPosition=Vector3.zero;
            warning=Art.Ring("Sparring attack cue",world.Transients,1.2f,.055f,Art.Coral);warning.SetActive(false);
            nextAttack=Time.time+.9f;
        }
        void Update()
        {
            if(!TrainingEnabled || World.Paused || (World.State!=SessionState.Practice && World.State!=SessionState.Battle) || !World.Player.Alive)return;
            if(Time.time<StunnedUntil){WindingUp=false;warning.SetActive(false);Visual.Animate(0,false);return;}
            Combatant target=World.NearestEnemy(this,40);if(target==null)return;Vector3 delta=target.transform.position-transform.position;delta.y=0;
            if(WindingUp)
            {
                if(!released && Time.time>=contactAt-.34f*.27f)
                {released=true;Visual.PlayMotion((MotionId)((int)MotionId.Attack1+combo),.34f);World.Sound.Play(SoundCue.SlashBack,.75f);}
                if(Time.time>=contactAt)
                {WindingUp=false;warning.SetActive(false);nextAttack=Time.time+1.05f;}
                Visual.Animate(0,false);return;
            }
            if(delta.magnitude<2.05f && Time.time>=nextAttack)
            {
                lockedAim=delta.sqrMagnitude>.01f?delta.normalized:transform.forward;
                transform.rotation=Quaternion.LookRotation(lockedAim);combo=(combo+1)%3;
                contactAt=Time.time+.60f;WindingUp=true;released=false;
                warning.transform.position=transform.position+lockedAim*.65f+Vector3.up*.025f;warning.SetActive(true);
                World.Melee.Slash(this,lockedAim,combo,.60f);
            }
            else if(delta.magnitude>1.6f)
            {
                transform.position=World.Clamp(transform.position+delta.normalized*2.2f*Time.deltaTime);
                transform.rotation=Quaternion.Slerp(transform.rotation,Quaternion.LookRotation(delta),Time.deltaTime*14);
            }
            Visual.Animate(delta.magnitude>1.6f? .55f:0,false);
        }
        protected override void OnDestroy(){if(warning!=null)Destroy(warning);base.OnDestroy();}
    }
}
