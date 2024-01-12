using System.Collections.Generic;
using Tree;

namespace GOAP.Actions
{
    public class StoreInventoryAction : BaseAction
    {
        public override bool PreCondition(WorldState cur)
        {
            return !cur.isPlayerNear
                   && (cur.nutsCarried > 0 || cur.garbageCarried > 0);
        }

        public override WorldState? CalculateState(WorldState cur, Target target)
        {
            // GetTargets assures target.type == HomeTree
            if (target.type != TargetType.HomeTree) return null; // Safety check
            
            // If inFOV then check if we can climb (not "isUsable" in case we are already on target)
            if (target.state == TargetState.InFOV 
                && !NavSystem.CanClimb((TreeController) target.objController)) return null;

            Controller.MoveStateTo(ref cur, target.location, GameModel.SquirrelHomeHeight);
            // Controller.EmptyInventory(ref cur); // DO NOT EMPTY (For cost calculations)
            Controller.SetStoredFlag(ref cur);
            return cur;
        }

        public override List<Target> GetTargets(WorldState cur)
        {
            return new List<Target> {TarSystem.homeTreeTarget};
        }

        protected override ActionResult Tick_Use(Target target)
        {
            Controller.StoreInventory((TreeController) target.objController);
            return ActionResult.Success;
        }
    }
}