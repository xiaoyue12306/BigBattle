using UnityEngine;

namespace BigBattle
{
    public sealed class LaneHeroBot : MonoBehaviour
    {
        LaneMatch match;BananaPlayer hero;float offset;bool retreat;
        public bool ExternalInput;
        public void Setup(LaneMatch mode,BananaPlayer actor,float lateral){match=mode;hero=actor;offset=lateral;}
        void Update()
        {if(!ExternalInput && match.Autonomous && !match.Finished && !hero.World.Paused && hero.Alive)hero.Tick(Plan());}
        public CombatInput Plan()
        {
            if(!match.Autonomous || match.Finished || hero.World.Paused || !hero.Alive)return default(CombatInput);
            if(hero.Health<hero.MaxHealth*.25f)retreat=true;else if(hero.Health>hero.MaxHealth*.82f)retreat=false;
            var target=match.PickFight(hero,10);if(target==null)target=match.Goal(hero.Team);if(target==null)return default(CombatInput);
            Vector3 delta=target.transform.position-transform.position;delta.y=0;float distance=delta.magnitude;
            var mage=hero as YueyuePlayer;float attackRange=mage!=null?6.2f:1.65f;
            Vector3 goal=target.transform.position;
            if(retreat)goal=FlowerLaneMap.Spawn(hero.Team)+FlowerLaneMap.Across*offset;
            else if(target is LaneStructure && !match.MinionSupport(hero.Team,target.transform.position))goal=target.transform.position-FlowerLaneMap.Forward(hero.Team)*8.5f+FlowerLaneMap.Across*offset;
            else if(target.IsStructure)goal+=FlowerLaneMap.Across*offset*.4f;
            Vector3 move=goal-transform.position;move.y=0;
            bool advancing=retreat?move.magnitude>1:Vector3.Distance(goal,target.transform.position)>7?move.magnitude>.5f:distance>attackRange;
            var input=new CombatInput{Move=advancing?new Vector2(move.normalized.x,move.normalized.z):Vector2.zero,Attack=!retreat && distance<attackRange+.5f};
            hero.Face(delta);
            if(mage!=null)
            {
                input.Thrust=!retreat && !target.IsStructure && distance<9 && mage.Remaining(1)==0;
                input.Peel=!retreat && distance<8 && mage.Fallen.Valid && mage.Remaining(2)==0;
                input.Dodge=retreat && distance<4 && mage.Remaining(3)==0;
                input.UltimateHeld=mage.Charging?mage.ChargeProgress<1:!retreat && distance<11 && mage.Souls>=3 && mage.Remaining(4)==0;
                if(input.UltimateHeld || mage.Charging)input.Move=Vector2.zero;
            }
            else
            {
                input.Thrust=!retreat && distance>2.1f && distance<4 && hero.Remaining(1)==0;
                input.Peel=!retreat && !target.IsStructure && distance<3.8f && hero.Remaining(2)==0;
                input.Dodge=retreat && distance<2.5f && hero.Remaining(3)==0;
                input.Ultimate=!retreat && distance<2.4f && hero.Remaining(4)==0;
            }
            return input;
        }
    }
}
