using System.Collections.Generic;
using Tree;

namespace GOAP.Actions
{
    public class HideAction : BaseAction
    {
        public override bool PreCondition(WorldState cur)
        {
            return cur.isPlayerNear;
        }

        public override WorldState? CalculateState(WorldState cur, Target target)
        {
            // GetTargets assures target.type and target.state != Forgotten
            if (target.type != TargetType.Tree || target.state == TargetState.Forgotten) return null; // Safety check
            
            // If inFOV then check if we can climb (not "isUsable" in case we are already on target)
            if (target.state == TargetState.InFOV 
                && !NavSystem.CanClimb((TreeController) target.objController)) return null;

            Controller.MoveStateTo(ref cur, target.location, GameModel.SquirrelHomeHeight);
            Controller.SetIsPlayerNear(ref cur, false); // explicitly set IsPlayerNear to false
            return cur;
        }

        public override List<Target> GetTargets(WorldState cur)
        {
            return TarSystem.GetTargetsOfType(TargetType.Tree);
        }

        protected override ActionResult Tick_Use(Target target)
        {
            return Controller.curState.isPlayerNear ? ActionResult.Running : ActionResult.Success;
        }
    }
}