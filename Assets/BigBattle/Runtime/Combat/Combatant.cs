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
            if(stun>=1 && !IsDummy)Visual.Slip(stun);
            World.Fx.Hit(transform.position + Vector3.up*.9f, amount, source == World.Player);
            if (this == World.Player) { World.CameraRig.Shake(.10f); World.Input.Rumble(.30f,.12f); World.Sound.Play(SoundCue.Hurt); }
            else World.Input.Rumble(.12f,.04f);
            if (!IsDummy)
            {
                StunnedUntil = Mathf.Max(StunnedUntil, Time.time+stun);
                transform.position = GardenArena.Clamp(transform.position + push);
            }
            if (Health <= 0)
            {
                if (IsDummy) { Health = MaxHealth; World.Fx.Notice("漂亮！木桩已重置"); }
                else Die();
            }
            return true;
        }

        public void Restore() { Health = MaxHealth; InvulnerableUntil = 0; StunnedUntil = 0; }
        protected virtual void Die()
        {
            World.Fx.Burst(transform.position+Vector3.up*.5f,Art.Coral,9);
            if (this == World.Player) { World.Finish(false); Visual.gameObject.SetActive(false); }
            else { World.Score++; World.Sound.Play(SoundCue.Pop); World.Actors.Remove(this); Destroy(gameObject); }
        }
        protected virtual void OnDestroy() { if (World != null) World.Actors.Remove(this); }
    }
}
