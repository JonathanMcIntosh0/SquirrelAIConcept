using System.Collections.Generic;

namespace GOAP
{
    public class GatherNutAction : BaseAction
    {
        public override bool PreCondition(WorldState cur)
        {
            return !cur.isPlayerNear
                   && !cur.isInventoryFull
                   && cur.garbageCarried == 0;
        }

        public override WorldState? CalculateState(WorldState cur, Target target)
        {
            throw new System.NotImplementedException();
        }

        public override float GetCost(WorldState cur, Target target)
        {
            throw new System.NotImplementedException();
        }

        public override List<Target> GetTargets(WorldState cur)
        {
            throw new System.NotImplementedException();
        }

        protected override ActionResult Tick_MoveToInspect(ref WorldState cur, Target target)
        {
            throw new System.NotImplementedException();
        }

        protected override ActionResult Tick_MoveToUse(ref WorldState cur, Target target)
        {
            throw new System.NotImplementedException();
        }

        protected override ActionResult Tick_Use(ref WorldState cur, Target target)
        {
            throw new System.NotImplementedException();
        }
    }
}