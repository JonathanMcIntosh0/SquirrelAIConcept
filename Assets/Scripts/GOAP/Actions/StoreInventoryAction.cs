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
            
            // If distanceTravelled > 0 then not first action in plan so no point in checking if canClimb just yet
            // since we will take some time to gather food. 
            // If inFOV then check if we can climb (not "isUsable" in case we are already on target)
            if (cur.distanceTravelled == 0 && target.state == TargetState.InFOV 
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