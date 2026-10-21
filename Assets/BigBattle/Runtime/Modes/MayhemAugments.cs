using System.Collections.Generic;
using UnityEngine;

namespace BigBattle
{
    public sealed class AugmentCard
    {
        public int Id,Tier,Hero;public string Name,Description;
        public AugmentCard(int id,int tier,int hero,string name,string description){Id=id;Tier=tier;Hero=hero;Name=name;Description=description;}
    }
    public sealed class MayhemAugments : MonoBehaviour
    {
        public static readonly AugmentCard[] Cards={
            new AugmentCard(0,0,-1,"精密打击","造成的伤害提高 18%。"),new AugmentCard(1,0,-1,"坚韧骨甲","受到的伤害降低 12%。"),
            new AugmentCard(2,0,-1,"寒地健步","移动速度提高 12%。"),new AugmentCard(3,0,-1,"迅捷施法","所有技能冷却缩短 15%。"),
            new AugmentCard(4,1,-1,"巨人之躯","最大生命提高 35%，立即获得新增生命。"),new AugmentCard(5,1,-1,"嗜血打击","对英雄造成实际伤害的 12% 转为治疗。"),
            new AugmentCard(6,1,-1,"破城者","对建筑伤害提高 45%。"),new AugmentCard(7,0,-1,"英雄猎手","对敌方英雄伤害提高 20%。"),
            new AugmentCard(8,1,-1,"终结本能","对生命低于 35% 的目标伤害提高 30%。"),new AugmentCard(9,1,-1,"同袍回响","命中英雄时，治疗 6 范围内友军 12 生命；冷却 4 秒。"),
            new AugmentCard(10,2,-1,"破冰冲击","每命中 6 次引发 2.5 范围、16 伤害的冲击。"),new AugmentCard(11,1,-1,"冰霜触碰","伤害使敌人减速 25%，持续 0.8 秒。"),
            new AugmentCard(12,2,-1,"绝境冰甲","受伤后生命低于 35% 时，后续伤害降低 40%，持续 2 秒；冷却 15 秒。"),new AugmentCard(13,0,-1,"再起之势","复活保护额外持续 2 秒，复活后 4 秒移速提高 30%。"),
            new AugmentCard(14,1,-1,"北境泉涌","脱战 5 秒后，每秒回复最大生命的 1.5%。"),new AugmentCard(15,1,-1,"霜光护盾","获得最大生命 20% 的护盾；复活或脱战 10 秒后补充。"),
            new AugmentCard(16,2,-1,"双重虹光","每第 3 次命中，追加该次实际伤害 25% 的回响，不触发其他强化。"),new AugmentCard(17,0,-1,"孤勇者","6 范围内没有其他友方英雄时，伤害提高 18%、移速提高 8%。"),
            new AugmentCard(18,1,0,"月刃终结","第三段普通攻击伤害提高 35%。"),new AugmentCard(19,1,0,"追风突刺","突刺冷却缩短 35%，突进速度提高 20%。"),
            new AugmentCard(20,2,0,"金叶风暴","旋转连斩伤害提高 40%，作用半径提高 20%。"),new AugmentCard(21,1,0,"金叶护身","每次闪避回复最大生命的 10%。"),
            new AugmentCard(22,2,3,"三魂并进","普通攻击额外射出两枚半伤害骷髅。"),new AugmentCard(23,1,3,"深牢铁索","铁索命中的禁锢延长至 2.8 秒。"),
            new AugmentCard(24,2,3,"亡魂军令","召回亡魂拥有原身 70% 生命、65% 攻击，存在 28 秒。"),new AugmentCard(25,2,3,"灭世锋芒","万魂魔剑剑气伤害提高 40%，仍每目标只命中一次。"),
            new AugmentCard(26,1,8,"爆泡弹仓","待爆泡泡上限由 5 提高至 7。"),new AugmentCard(27,2,8,"十字延展","泡泡每条爆炸臂长度由 5 提高至 7。"),
            new AugmentCard(28,1,8,"连爆狂潮","被其他泡泡提前引爆时，自己的泡泡伤害提高 35%。"),new AugmentCard(29,1,8,"急速引信","之后放下的泡泡引信由 1.8 秒缩短至 1.35 秒。")};
        public readonly List<int> Chosen=new List<int>();
        public LaneMatch Match {get;private set;}public Combatant Owner {get;private set;}
        public bool Active=>Match!=null && !Match.Finished && Owner.World.IsLane && Owner.World.Lane==Match;
        public float Shield {get;private set;}
        float baseHealth,nextHeal,armorUntil,armorReady,nextShield,spawnAt=-100;int hits;static bool proc;
        public static MayhemAugments Attach(BananaPlayer owner,LaneMatch match)
        {var state=owner.GetComponent<MayhemAugments>();if(state==null)state=owner.gameObject.AddComponent<MayhemAugments>();state.Owner=owner;state.Match=match;state.baseHealth=owner.MaxHealth;owner.Augments=state;return state;}
        public bool Has(int id)=>Active && Chosen.Contains(id);
        public bool Apply(int id)
        {
            if(id<0 || id>=Cards.Length || Chosen.Contains(id) || Cards[id].Hero>=0 && Cards[id].Hero!=Owner.VisualStyle)return false;
            Chosen.Add(id);if(id==4)Owner.SetMaximumHealth(baseHealth*1.35f);if(id==15){Shield=Owner.MaxHealth*.2f;nextShield=Time.time+10;}return true;
        }
        public void Clear(){if(Owner!=null && baseHealth>0)Owner.SetMaximumHealth(baseHealth);Chosen.Clear();Shield=0;hits=0;nextHeal=armorUntil=armorReady=nextShield=0;spawnAt=-100;Owner.Augments=null;Match=null;}
        bool Alone()
        {foreach(var actor in Owner.World.Actors)if(actor!=null && actor!=Owner && actor is BananaPlayer && actor.Team==Owner.Team && actor.Alive && actor.gameObject.activeInHierarchy && Vector3.Distance(actor.transform.position,Owner.transform.position)<6)return false;return true;}
        public float Movement=>!Active?1:(Has(2)?1.12f:1)*(Has(17)&&Alone()?1.08f:1)*(Has(13)&&Time.time-spawnAt<4?1.3f:1);
        public float Cooldown(int skill)=>!Active?1:(Has(3)?.85f:1)*(Has(19)&&skill==1?.65f:1);
        public void OnRespawn(){spawnAt=Time.time;if(Has(13))Owner.InvulnerableUntil+=2;if(Has(15))Shield=Owner.MaxHealth*.2f;}
        public float Outgoing(Combatant target,float damage)
        {
            if(!Active || proc)return damage;float scale=Has(0)?1.18f:1;
            if(Has(6)&&target.IsStructure)scale*=1.45f;if(Has(7)&&target is BananaPlayer)scale*=1.2f;
            if(Has(8)&&target.Health<target.MaxHealth*.35f)scale*=1.3f;if(Has(17)&&Alone())scale*=1.18f;
            return damage*scale;
        }
        public float Incoming(float damage)
        {
            if(!Active)return damage;if(Has(1))damage*=.88f;if(Time.time<armorUntil)damage*=.6f;
            float absorbed=Mathf.Min(Shield,damage);Shield-=absorbed;return damage-absorbed;
        }
        public void Damaged()
        {if(Has(12)&&Owner.Health<Owner.MaxHealth*.35f && Time.time>=armorReady){armorUntil=Time.time+2;armorReady=Time.time+15;}}
        public void Hit(Combatant victim,float actual)
        {
            if(!Active || proc || actual<=0)return;hits++;
            if(victim is BananaPlayer && Has(5))Owner.Heal(actual*.12f);
            if(Has(11)&&!victim.IsStructure)victim.Slow(.8f);
            if(victim is BananaPlayer && Has(9) && Time.time>=nextHeal)
            {nextHeal=Time.time+4;foreach(var ally in Owner.World.Actors)if(ally!=null && ally.Team==Owner.Team && !ally.IsStructure && Vector3.Distance(ally.transform.position,Owner.transform.position)<6)ally.Heal(12);Owner.World.Fx.Circle(Owner.transform.position,2,Art.Mint,.3f);}
            try
            {
                proc=true;
                if(Has(16)&&hits%3==0 && victim!=null && victim.Alive)victim.Damage(actual*.25f,Owner,Vector3.zero,0);
                if(Has(10)&&hits%6==0){Owner.World.HitArea(Owner,Owner.transform.position,2.5f,Vector3.forward,-1,16,0);Owner.World.Fx.Circle(Owner.transform.position,2.5f,Art.Yellow,.3f);}
            }
            finally{proc=false;}
        }
        void Update()
        {
            if(!Active || Owner.World.Paused || !Owner.Alive)return;
            if(Has(14)&&Time.time-Owner.LastHitAt>5)Owner.Heal(Owner.MaxHealth*.015f*Time.deltaTime);
            if(Has(15)&&Time.time-Owner.LastHitAt>10 && Time.time>=nextShield){Shield=Owner.MaxHealth*.2f;nextShield=Time.time+10;}
        }
    }
}
