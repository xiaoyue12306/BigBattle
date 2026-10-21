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
            if(match.Transiting(hero))return default(CombatInput);
            Vector3 gate=match.Gates[hero.Team].position;
            if(FlowerLaneMap.ToLane(hero.transform.position).z*(hero.Team==0?-1:1)>35)
            {if(Vector3.Distance(hero.transform.position,gate)<1.7f){match.TryGate(hero);return default(CombatInput);}Vector3 toGate=(gate-hero.transform.position).normalized;return new CombatInput{Move=new Vector2(toGate.x,toGate.z)};}
            if(hero.Health<hero.MaxHealth*.25f)retreat=true;else if(hero.Health>hero.MaxHealth*.82f)retreat=false;
            var objective=match.Goal(hero.Team);var target=match.PickFight(hero,10);
            // Convert a minion escort into a siege instead of chasing endless respawns.
            if(objective!=null && match.MinionSupport(hero.Team,objective.transform.position) && Vector3.Distance(hero.transform.position,objective.transform.position)<10)target=objective;
            if(target==null)target=objective;if(target==null)return default(CombatInput);
            Vector3 delta=target.transform.position-transform.position;delta.y=0;float distance=delta.magnitude;
            var mage=hero as YueyuePlayer;var ali=hero as AliPlayer;float attackRange=ali!=null?2.6f:mage!=null?6.2f:1.65f;
            Vector3 goal=target.transform.position;
            if(retreat){var relic=match.RelicGoal(hero);if(relic.HasValue)goal=relic.Value;else retreat=false;}
            else if(target is LaneStructure && !match.MinionSupport(hero.Team,target.transform.position))goal=target.transform.position-FlowerLaneMap.Forward(hero.Team)*8.5f+FlowerLaneMap.Across*offset;
            else if(target.IsStructure)goal+=FlowerLaneMap.Across*offset*.4f;
            goal=match.Steer(hero,goal);Vector3 move=goal-transform.position;move.y=0;
            bool advancing=retreat?move.magnitude>1:Vector3.Distance(goal,target.transform.position)>7?move.magnitude>.5f:distance>attackRange;
            var input=new CombatInput{Move=advancing?new Vector2(move.normalized.x,move.normalized.z):Vector2.zero,Attack=!retreat && distance<attackRange+.5f};
            hero.Face(delta);
            if(mage!=null)
            {
                input.Thrust=!retreat && !target.IsStructure && distance<9 && mage.Remaining(1)==0;
                input.Peel=!retreat && distance<8 && mage.Fallen.Valid && mage.Remaining(2)==0;
                input.Dodge=retreat && distance<4 && mage.Remaining(3)==0;
                input.UltimateHeld=mage.Charging?mage.ChargeProgress<1:!mage.NeedsUltimateRelease && !retreat && distance<11 && mage.Souls>=3 && mage.Remaining(4)==0;
                if(input.UltimateHeld || mage.Charging)input.Move=Vector2.zero;
            }
            else if(ali!=null)
            {
                // Align to a map axis so a cross can actually reach the target.
                Vector3 local=FlowerLaneMap.ToLane(delta);bool along=Mathf.Abs(local.z)>=Mathf.Abs(local.x);
                Vector3 axis=along?FlowerLaneMap.Forward(0):FlowerLaneMap.Across;
                Vector3 line=target.transform.position-axis*(2.4f*((along?local.z:local.x)>=0?1:-1));
                if(!retreat && !(target is LaneStructure && !match.MinionSupport(hero.Team,target.transform.position)))
                {
                    Vector3 align=WorldPoint(line)-hero.transform.position;input.Move=align.magnitude>.35f?new Vector2(align.normalized.x,align.normalized.z):Vector2.zero;
                    input.Attack=distance<4.2f && (Mathf.Abs(local.x)<.75f || Mathf.Abs(local.z)<.75f);
                }
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
        Vector3 WorldPoint(Vector3 point)=>hero.World.Clamp(point);
    }
}
