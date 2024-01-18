using GOAP.Agent;
using UnityEngine;

namespace GOAP.Goals
{
    public abstract class BaseGoal : MonoBehaviour
    {
        protected enum PriorityLevel : int
        {
            None = 0,
            VeryLow = 5,
            Low = 10,
            Medium = 25,
            High = 50,
            Immediate = 100
        }
        
        [SerializeField] protected PriorityLevel priority = 0;
        public int Priority
        {
            get => (int) priority;
            protected set => priority = (PriorityLevel) value;
        }

        protected SController Controller;
        protected TargetingSystem TarSystem;

        public abstract bool PreCondition(WorldState cur);
        public abstract bool PostCondition(WorldState next);
        public abstract void RefreshPriority();
        public abstract float GetCost(WorldState next, float cost);

        private void Awake()
        {
            Controller = GetComponent<SController>();
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