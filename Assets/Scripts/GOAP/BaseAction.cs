using System;
using System.Collections.Generic;
using UnityEngine;

namespace GOAP
{
    public enum ActionResults
    {
        // Unknown,
        Running,
        Success,
        Fail
    }
    
    public abstract class BaseAction : MonoBehaviour
    {
        
        
        private enum ActionStages
        {
            MovingToInspect = 0,
            MovingToUse = 1,
            Using = 2
        }
        private ActionStages _stage = 0;
        
        // TODO Add Memory and Controller fields (or just an agent that has both)

        public abstract bool PreCondition(WorldState cur);
        public abstract bool PostCondition(WorldState next);
        public abstract WorldState? CalculateState(WorldState cur, Target target); //TODO might not need nullable
        public abstract float GetCost(WorldState cur);
        public abstract List<Target> GetTargets(); // Returns possible targets. E.g. Nuts in sight and memory for GatherNut

        public ActionResults Tick(ref WorldState cur, Target target)
        {
            var result = _stage switch
            {
                ActionStages.MovingToInspect => Tick_MoveToInspect(ref cur, target),
                ActionStages.MovingToUse => Tick_MoveToUse(ref cur, target),
                ActionStages.Using => Tick_Use(ref cur, target),
                _ => throw new ArgumentOutOfRangeException()
            };
            
            if (_stage != ActionStages.Using && result == ActionResults.Success) 
            {
                _stage++;
                result = ActionResults.Running;
            }

            return result;
        }
        protected abstract ActionResults Tick_MoveToInspect(ref WorldState cur, Target target);
        protected abstract ActionResults Tick_MoveToUse(ref WorldState cur, Target target);
        protected abstract ActionResults Tick_Use(ref WorldState cur, Target target);

        public void Reset() { _stage = ActionStages.MovingToInspect; }

        private void Awake()
        {
            throw new NotImplementedException();
        }
        // public void Awake() { // Get components }
    }
}