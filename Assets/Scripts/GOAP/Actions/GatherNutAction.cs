using System;
using System.Collections.Generic;
using UnityEngine;

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
            // GetTargets also does not return targets where (inFOV && !isUsable) 
            if (target.type != TargetType.Nut || target.state == TargetState.Forgotten) return null; // Safety check
            if (target.state == TargetState.InFOV && target.obj == null) return null; // Safety check

            var pos = new Vector3(target.location.x, GameModel.FloorHeight, target.location.y);
            Controller.MoveStateTo(ref cur, pos);
            Controller.AddNut(ref cur);
            return cur;
        }

        public override float GetCost(WorldState cur, Target target)
        {
            return Vector2.Distance(cur.location, target.location);
        }

        public override List<Target> GetTargets(WorldState cur)
        {
            return TarSystem.GetTargetsOfType(TargetType.Nut);
        }

        protected override ActionResult Tick_MoveToInspect(ref WorldState cur, Target target)
        {
            if (target.state == TargetState.InMemory)
                NavSystem.SetDestination(target.location, GameModel.FloorHeight);

            return target.state switch
            {
                TargetState.InFOV => ActionResult.Success,
                TargetState.InMemory => ActionResult.Running,
                TargetState.Forgotten => ActionResult.Fail,
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        protected override ActionResult Tick_MoveToUse(ref WorldState cur, Target target)
        {
            // If here then target within FOV so check if destroyed or not
            if (!target.IsUsable()) return ActionResult.Fail;
            NavSystem.SetDestination(target.location, GameModel.FloorHeight); // Redundant
            return NavSystem.reachedDestination ? ActionResult.Success : ActionResult.Running;
        }

        protected override ActionResult Tick_Use(ref WorldState cur, Target target)
        {
            if (!target.IsUsable()) return ActionResult.Fail; // Final check before pickup
            
            //TODO implement a pickUp method within nutController that destroys nut and removes it from GameModel.Nuts
            
            // Destroy(target.obj); // Destroy nut
                                 // Note: onDestroy will get called after all Update and LateUpdate have been run.
                                 // Thus GameModel.Nuts will still contain a fake null ref to Nut during remaining updates.
                                 // Also target.obj does not get set to null till after 
            Controller.AddNut(ref cur); // Update state
            
            throw new System.NotImplementedException();
        }
    }
}