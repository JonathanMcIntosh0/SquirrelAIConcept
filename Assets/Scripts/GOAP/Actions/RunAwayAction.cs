using System.Collections.Generic;
using UnityEngine;

namespace GOAP.Actions
{
    public class RunAwayAction : BaseAction
    {
        [SerializeField] private float distance = 3f;
        public override bool PreCondition(WorldState cur)
        {
            return cur.isPlayerNear;
        }

        public override WorldState? CalculateState(WorldState cur, Target target)
        {
            Controller.MoveStateTo(ref cur, target.location, GameModel.FloorHeight);
            Controller.SetIsPlayerNear(ref cur, false);
            return cur;
        }

        public override float GetCost(WorldState cur, Target target)
        {
            return float.PositiveInfinity;
        }

        public override List<Target> GetTargets(WorldState cur)
        {
            return new List<Target> {TarSystem.GetRunAwayTarget(distance, cur.location)};
        }

        protected override ActionResult Tick_ClimbUp(Target target)
        {  
            return ActionResult.Success;
        }

        protected override ActionResult Tick_Use(Target target)
        {
            return ActionResult.Success;
        }
    }
}