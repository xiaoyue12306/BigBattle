using UnityEngine;

namespace BigBattle
{
    public class Combatant : MonoBehaviour
    {
        public int Team { get; private set; }
        public float MaxHealth { get; private set; }
        public float Health { get; private set; }
        public bool IsDummy { get; private set; }
        public bool Alive => Health > 0;
        public float Radius = .45f;
        public ActorVisual Visual { get; private set; }
        public GameRoot World { get; private set; }
        public float StunnedUntil { get; protected set; }
        public float InvulnerableUntil;
        public float LastHitAt { get; private set; } = -100;
        public float TotalDamageReceived { get; private set; }
        Vector3 knockbackVelocity;

        public void Initialize(GameRoot world, int team, float maxHealth, int style, bool dummy = false)
        {
            World = world; Team = team; MaxHealth = maxHealth; Health = maxHealth; IsDummy = dummy;
            Visual = ActorVisual.Create(transform, style);
            world.Actors.Add(this);
        }

        public bool Damage(float amount, Combatant source, Vector3 push, float stun = .08f)
        {
            if (!Alive || source == null || source.Team == Team || Time.time < InvulnerableUntil || amount <= 0) return false;
            LastHitAt = Time.time; TotalDamageReceived += amount;
            Health = Mathf.Max(0, Health-amount); Visual.Flash();
            Vector3 direction=transform.position-source.transform.position;direction.y=0;
            Visual.React(direction.normalized,amount>=30?1.3f:.8f);
            if(stun>=1 && !IsDummy)Visual.Slip(stun);
            World.Fx.Hit(transform.position + Vector3.up*.9f, amount, source == World.Player);
            if (this == World.Player) { World.CameraRig.Shake(.10f); World.Input.Rumble(.30f,.12f); World.Sound.Play(SoundCue.Hurt); }
            else if(source==World.Player)
            {
                bool heavy=amount>=30;
                World.Input.Rumble(heavy?.38f:.20f,heavy?.10f:.055f);
                World.Sound.Impact(heavy);World.CameraRig.Impact(direction,heavy?1.1f:.5f);
                if(amount>=20)World.HitPause(heavy);
            }
            if (!IsDummy)
            {
                StunnedUntil = Mathf.Max(StunnedUntil, Time.time+stun);
                knockbackVelocity+=push*18;
            }
            if (Health <= 0)
            {
                if (IsDummy) { Health = MaxHealth; World.Fx.Notice("漂亮！木桩已重置"); }
                else Die();
            }
            return true;
        }

        public void Restore() { Health = MaxHealth; InvulnerableUntil = 0; StunnedUntil = 0;knockbackVelocity=Vector3.zero; }
        protected virtual void LateUpdate()
        {
            if(World==null || World.Paused || !Alive)return;
            transform.position=GardenArena.Clamp(transform.position+knockbackVelocity*Time.deltaTime);
            knockbackVelocity*=Mathf.Exp(-18*Time.deltaTime);
        }
        protected virtual void Die()
        {
            World.Fx.Burst(transform.position+Vector3.up*.5f,Art.Coral,9);
            if (this == World.Player) { World.Finish(false); Visual.SetDefeated(); }
            else { World.Fx.Ghost(Visual,true);World.Score++; World.Sound.Play(SoundCue.Pop); World.Actors.Remove(this); Destroy(gameObject); }
        }
        protected virtual void OnDestroy() { if (World != null) World.Actors.Remove(this); }
    }
}
