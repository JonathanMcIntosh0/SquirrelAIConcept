using UnityEngine;

namespace GOAP.Goals
{
    public class StockFoodGoal : BaseGoal
    {
        public override bool PreCondition(WorldState cur)
        {
            return !cur.isPlayerNear;
        }

        public override bool PostCondition(WorldState next)
        {
            return next.hasStored;
        }

        public override void RefreshPriority()
        {
            var nutCount = TarSystem.GetTargetCountOfType(TargetType.Nut);
            var garbageCount = TarSystem.GetTargetCountOfType(TargetType.GarbageCan);
            priority = nutCount < Controller.nutInventorySize - Controller.curState.nutsCarried
                        // || garbageCount < Controller.garbageInventorySize - Controller.curState.garbageCarried
                ? PriorityLevel.Low
                : PriorityLevel.High;
        }

        public override float GetCost(WorldState next, float cost)
        {
            var dToHome = Vector2.Distance(next.location, TarSystem.homeTreeTarget.location);
            
            // TODO make cost function admissible (since using A* we currently get suboptimal in certain cases)
            return (cost + dToHome) / next.inventoryFill; // Gives infinite cost when inventoryFill = 0
        }
    }
}