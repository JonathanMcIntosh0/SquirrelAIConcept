using System;
using System.Collections.Generic;
using UnityEngine;

namespace GOAP.Actions
{
    public enum ActionResult
    {
        // Unknown,
        Running,
        Success,
        Fail
    }
    
    public abstract class BaseAction : MonoBehaviour
    {
        private enum ActionStage
        {
            MovingToInspect = 0,
            MovingToUse = 1,
            Using = 2
        }
        private ActionStage _stage = 0;
        protected SController Controller;
        protected NavigationSystem NavSystem;
        protected TargetingSystem TarSystem;
        
        // private delegate ActionResult TickFunc(ref WorldState cur, Target target);
        // private TickFunc[] TickFuncs;

        // TODO maybe make baseAction class just have tick (no stages) then create subclass for useActions
        // TODO also make planner call reset if tick != ActionResult.Running instead of resetting within tick
        
        // TODO or could have list/array of delegate functions
        
        // TODO maybe create startMove stage so we don't keep calling setDestination repeatedly.
        
        public abstract bool PreCondition(WorldState cur);
        // public abstract bool PostCondition(WorldState next);
        public abstract WorldState? CalculateState(WorldState cur, Target target); //TODO might not need nullable
        public abstract float GetCost(WorldState cur, Target target);
        public abstract List<Target> GetTargets(WorldState cur); // Returns possible targets from cur state. E.g. Nuts in sight and memory for GatherNut

        public ActionResult Tick(Target target)
        {
            WorldState curState = Controller.curState;
            var result = _stage switch
            {
                ActionStage.MovingToInspect => Tick_MoveToInspect(ref curState, target),
                ActionStage.MovingToUse => Tick_MoveToUse(ref curState, target),
                ActionStage.Using => Tick_Use(ref curState, target),
                _ => throw new ArgumentOutOfRangeException()
            };
            Controller.curState = curState;

            if (result != ActionResult.Success) return result;
            if (_stage != ActionStage.Using)
            {
                result = ActionResult.Running;
                _stage++;
                // Debug.Log($"{gameObject.name}: Moving to stage {_stage} of {GetType().Name}");
            }
            else _stage = 0;
            
            return result;
        }

        //TODO maybe combine MoveToInspect and MoveToUse into the function below
        //TODO also maybe move TargetState checks to outside IsUsable to allow reuse of function in Use
        //TODO as mentioned above maybe generalise this to a UseAction class
        // protected virtual ActionResult Tick_Move(ref WorldState cur, Target target)
        // {
        //     switch (target.state)
        //     {
        //         case TargetState.Forgotten:
        //         case TargetState.InFOV when !target.IsUsable():
        //             return ActionResult.Fail;
        //         default: // InMemory or InFOV && Usable
        //             NavSystem.SetDestination(target.location, GameModel.FloorHeight);
        //             return NavSystem.reachedDestination ? ActionResult.Success : ActionResult.Running;
        //     }
        // }
        protected abstract ActionResult Tick_MoveToInspect(ref WorldState cur, Target target);
        protected abstract ActionResult Tick_MoveToUse(ref WorldState cur, Target target);
        protected abstract ActionResult Tick_Use(ref WorldState cur, Target target);

        public void Reset() { _stage = ActionStage.MovingToInspect; }

        private void Awake()
        {
            Controller = GetComponent<SController>();
            NavSystem = GetComponent<NavigationSystem>();
            TarSystem = GetComponent<TargetingSystem>();
        }
    }
}