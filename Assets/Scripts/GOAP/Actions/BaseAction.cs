using System;
using System.Collections.Generic;
using GarbageCan;
using GOAP.Agent;
using JetBrains.Annotations;
using Nut;
using Tree;
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
    
    //TODO maybe use coroutines
    public abstract class BaseAction : MonoBehaviour
    {
        private enum ActionStage
        {
            MovingHorizontally = 0,
            MovingVertically = 1,
            Using = 2
        }
        private ActionStage _stage = 0;
        protected SController Controller;
        protected NavigationSystem NavSystem;
        protected TargetingSystem TarSystem;

        public abstract bool PreCondition(WorldState cur);
        // public abstract bool PostCondition(WorldState next);
        public abstract WorldState? CalculateState(WorldState cur, Target target);

        // Returns possible targets from cur state. E.g. Nuts in sight and memory for GatherNut
        [NotNull] public abstract List<Target> GetTargets(WorldState cur);

        public virtual float GetCost(WorldState cur, Target target)
        {
            var h = target.objController switch
            {
                NutController _ => GameModel.FloorHeight,
                GarbageCanController _ => GameModel.GarbageCanHeight,
                TreeController _ => GameModel.SquirrelHomeHeight,
                null => GameModel.FloorHeight, // Location type
                _ => throw new ArgumentOutOfRangeException()
            };
            var d = Vector2.Distance(cur.location, target.location);
            
            // Note: Cost a little off if currently on target (Adds cost to go down to floor and back up to h) 
            return d + h + cur.height;
        }

        public ActionResult Tick(Target target)
        {
            var result = _stage switch
            {
                ActionStage.MovingHorizontally => Tick_MoveHorizontally(target),
                ActionStage.MovingVertically => Tick_ClimbUp(target),
                ActionStage.Using => Tick_Use(target),
                _ => throw new ArgumentOutOfRangeException()
            };
            
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

        protected virtual ActionResult Tick_MoveHorizontally(Target target)
        {
            NavSystem.SetHorizDestination(target.location, target.objController as IClimbable);
            if (NavSystem.ReachedDestination) return ActionResult.Success; // Check if already there

            if (target.state == TargetState.Forgotten 
                || target.state == TargetState.InFOV && !target.IsUsable()) 
                return ActionResult.Fail;

            return ActionResult.Running;
        }

        protected virtual ActionResult Tick_ClimbUp(Target target)
        {
            // Default success if not climbable object
            if (!(target.objController is IClimbable climbable)) return ActionResult.Success; 
            if (!NavSystem.ClimbUp(climbable)) return ActionResult.Fail;
            return NavSystem.ReachedDestination ? ActionResult.Success : ActionResult.Running;
        }
        protected abstract ActionResult Tick_Use(Target target); // Could make this default to success

        public virtual void Reset()
        {
            _stage = ActionStage.MovingHorizontally;
        }

        private void Awake()
        {
            Controller = GetComponent<SController>();
            NavSystem = GetComponent<NavigationSystem>();
            TarSystem = GetComponent<TargetingSystem>();
        }
        
        //Empty start method to allow enable/disable for debug purposes
        private void Start()
        {
            
        }

        public override string ToString()
        {
            return GetType().Name;
        }
    }
}