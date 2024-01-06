using System.Collections.Generic;
using UnityEngine;

namespace GOAP.Actions
{
    public class WanderAction : BaseAction
    {
        [SerializeField] private int numberOfTargets = 10;
        [SerializeField] private float randomRange = 3f;
        public override bool PreCondition(WorldState cur)
        {
            return true;
        }

        public override WorldState? CalculateState(WorldState cur, Target target)
        {
            cur.distanceTravelled += Vector2.Distance(cur.location, target.location);
            cur.location = target.location;
            return cur;
        }

        public override float GetCost(WorldState cur, Target target)
        {
            return Vector2.Distance(cur.location, target.location);
        }

        public override List<Target> GetTargets(WorldState cur)
        {
            var targets = new List<Target>();
            for (int i = 0; i < numberOfTargets; i++)
            {
                targets.Add(TarSystem.GetRandomLocationTarget(randomRange, cur.location));
            }

            return targets;
        }

        protected override ActionResult Tick_MoveToInspect(ref WorldState cur, Target target)
        {
            return ActionResult.Success;
        }

        protected override ActionResult Tick_MoveToUse(ref WorldState cur, Target target)
        {  
            NavSystem.SetDestination(target.location, GameModel.FloorHeight);
            return NavSystem.reachedDestination ? ActionResult.Success : ActionResult.Running;
        }

        protected override ActionResult Tick_Use(ref WorldState cur, Target target)
        {
            return ActionResult.Success;
        }
    }
}