using System;
using UnityEngine;

namespace GOAP
{
    public abstract class BaseGoal : MonoBehaviour
    {
        //TODO Add enum of preset priority levels
        [SerializeField] public int priority = 0;
        public int Priority
        {
            get => priority;
            protected set => priority = value;
        }
        
        //TODO Add Agent field to allow goals to access memory and such
        
        public abstract bool PreCondition(WorldState cur);
        public abstract bool PostCondition(WorldState next);
        public abstract void RefreshPriority();
        public abstract float GetCost(WorldState next, float cost);

        protected void Start()
        {
            throw new NotImplementedException(); //TODO init Agent field
        }
    }
}