using UnityEngine;

namespace BigBattle
{
    public sealed class MochiEnemy : Combatant
    {
        public bool WindingUp { get; private set; }
        float attackAt, nextAttack;
        Vector3 strikePoint;
        GameObject warning;
        public void Setup(GameRoot world, float health)
        {
            Initialize(world,1,health,1);
            warning=Art.Ring("Enemy attack warning",world.Transients,1.10f,.11f,Art.Coral);
            warning.SetActive(false);
            nextAttack=Time.time+.6f;
        }
        void Update()
        {
            if (World==null || World.Paused || World.State!=SessionState.Battle || !Alive || !World.Player.Alive) return;
            if (Time.time<StunnedUntil) { WindingUp=false; warning.SetActive(false); Visual.Animate(0,false); return; }
            Combatant target=World.NearestEnemy(this,40);if(target==null)return;Vector3 delta=target.transform.position-transform.position; delta.y=0;
            if (WindingUp)
            {
                if (Time.time>=attackAt)
                {
                    WindingUp=false; warning.SetActive(false); nextAttack=Time.time+1.1f;
                    World.HitArea(this,strikePoint,1.12f,Vector3.forward,-1,13,.14f);
                    World.Fx.Circle(strikePoint,1.10f,Art.Coral,.18f); Visual.Swing();
                }
                Visual.Animate(0,false); return;
            }
            if (delta.magnitude<1.65f && Time.time>=nextAttack)
            {
                WindingUp=true; attackAt=Time.time+.65f;
                strikePoint=transform.position+delta.normalized*.72f;
                warning.transform.position=strikePoint; warning.SetActive(true);
            }
            else if (delta.magnitude>1.25f)
            {
                Vector3 separation=Vector3.zero;
                foreach(Combatant other in World.Actors)
                {
                    if(other==null || other==this || other.Team!=Team) continue;
                    Vector3 away=transform.position-other.transform.position;
                    if(away.sqrMagnitude<1.4f && away.sqrMagnitude>.001f) separation+=away.normalized*.8f;
                }
                Vector3 velocity=(delta.normalized+separation).normalized*(1.65f+World.Wave*.12f);
                transform.position=GardenArena.Clamp(transform.position+velocity*Time.deltaTime);
            }
            if(delta.sqrMagnitude>.01f) transform.rotation=Quaternion.Slerp(transform.rotation,Quaternion.LookRotation(delta),Time.deltaTime*12);
            Visual.Animate(delta.magnitude>1.25f?1:0,false);
        }
        protected override void OnDestroy() { if(warning!=null) Destroy(warning); base.OnDestroy(); }
        void OnDisable(){if(warning!=null)warning.SetActive(false);WindingUp=false;}
    }
}
