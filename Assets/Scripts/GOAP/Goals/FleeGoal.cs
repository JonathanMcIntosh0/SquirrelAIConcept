namespace GOAP.Goals
{
    public class FleeGoal : BaseGoal
    {
        public override bool PreCondition(WorldState cur)
        {
            return cur.isPlayerNear;
        }

        public override bool PostCondition(WorldState next)
        {
            return !next.isPlayerNear;
        }

        public override void RefreshPriority()
        {
            priority = PriorityLevel.Immediate;
        }

        public override float GetCost(WorldState next, float cost)
        {
            return cost;
        }
    }
}