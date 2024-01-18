using System.Collections.Generic;
using GarbageCan;

namespace GOAP.Actions
{
    public class GatherGarbageAction : BaseAction
    {
        private Target _failedTarget; // Allows us to check during planning if previous plan failed on this target
                                     // Eg. If we try to get garbage but get trapped then we should not try again
        public override bool PreCondition(WorldState cur)
        {
            return !cur.isPlayerNear
                   && !cur.isInventoryFull
                   && cur.nutsCarried == 0;
        }

        public override WorldState? CalculateState(WorldState cur, Target target)
        {
            if (target == _failedTarget) return null; // Don't try again 
            
            // GetTargets assures target.type and target.state != Forgotten
            if (target.type != TargetType.GarbageCan || target.state == TargetState.Forgotten) return null; // Safety check
            
            // If inFOV then check if we can climb (not "isUsable" in case we are already on target)
            if (target.state == TargetState.InFOV 
                && !NavSystem.CanClimb((GarbageCanController) target.objController)) return null;
            
            Controller.MoveStateTo(ref cur, target.location, GameModel.GarbageCanHeight);
            Controller.AddGarbage(ref cur);
            return cur;
        }

        public override List<Target> GetTargets(WorldState cur)
        {
            return TarSystem.GetTargetsOfType(TargetType.GarbageCan);
        }
        
        protected override ActionResult Tick_Use(Target target)
        {
            var res = Controller.PickUpGarbage((GarbageCanController) target.objController);
            if (!res) _failedTarget = target;
            
            return res ? ActionResult.Success : ActionResult.Fail;
        }

        public override void Reset()
        {
            base.Reset();
            _failedTarget = null;
        }
    }
}