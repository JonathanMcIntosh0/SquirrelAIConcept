using UnityEngine;

namespace GOAP.Goals
{
    public abstract class BaseGoal : MonoBehaviour
    {
        //TODO Add enum of preset priority levels
        [SerializeField] private int priority = 0;
        public int Priority
        {
            get => priority;
            protected set => priority = value;
        }

        protected SController Controller;
        
        public abstract bool PreCondition(WorldState cur);
        public abstract bool PostCondition(WorldState next);
        public abstract void RefreshPriority();
        public abstract float GetCost(WorldState next, float cost);

        protected void Start()
        {
            Controller = GetComponent<SController>();
        }

        public override string ToString()
        {
            return GetType().Name;
        }
    }
}