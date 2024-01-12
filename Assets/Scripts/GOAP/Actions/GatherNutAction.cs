using System.Collections.Generic;
using Nut;

namespace GOAP.Actions
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
            // GetTargets assures target.type == Nut and target.state != Forgotten
            // GetTargets also does not return nuts where (inFOV && !isUsable) 
            if (target.type != TargetType.Nut || target.state == TargetState.Forgotten) return null; // Safety check
            if (target.state == TargetState.InFOV && !target.IsUsable()) return null; // Safety check

            Controller.MoveStateTo(ref cur, target.location, GameModel.FloorHeight);
            Controller.AddNut(ref cur);
            return cur;
        }

        public override List<Target> GetTargets(WorldState cur)
        {
            return TarSystem.GetTargetsOfType(TargetType.Nut);
        }

        protected override ActionResult Tick_ClimbUp(Target target)
        {
            return ActionResult.Success;
        }

        protected override ActionResult Tick_Use(Target target)
        {
            if (!target.IsUsable()) return ActionResult.Fail; // Final check before pickup

            Controller.PickUpNut((NutController) target.objController);
            return ActionResult.Success;
        }
    }
}