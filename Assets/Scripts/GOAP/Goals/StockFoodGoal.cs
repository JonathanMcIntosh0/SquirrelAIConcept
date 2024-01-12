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
            priority = (Controller.curState.inventoryFill >= .5) ? PriorityLevel.High : PriorityLevel.Medium;
        }

        public override float GetCost(WorldState next, float cost)
        {
            // TODO make cost function admissible (since using A* we currently get suboptimal in certain cases)
            return cost / next.inventoryFill; // Gives infinite cost when inventoryFill = 0
        }
    }
}