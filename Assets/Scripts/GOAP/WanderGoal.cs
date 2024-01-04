using UnityEngine;
using UnityEngine.Serialization;

namespace GOAP
{
    public class WanderGoal : BaseGoal
    {
        [SerializeField] private float minTravelDistance = 10f;
        
        public override bool PreCondition(WorldState cur)
        {
            return true;
        }

        public override bool PostCondition(WorldState next)
        {
            return next.distanceTravelled >= minTravelDistance;
        }

        public override void RefreshPriority()
        {
            // Priority = 10;
        }

        public override float GetCost(WorldState next, float cost)
        {
            return -cost;
        }
    }
}