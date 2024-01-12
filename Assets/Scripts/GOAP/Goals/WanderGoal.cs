using UnityEngine;

namespace GOAP.Goals
{
    public class WanderGoal : BaseGoal
    {
        [SerializeField] private float minTravelDistance = 10f;
        
        public override bool PreCondition(WorldState cur)
        {
            return !cur.isPlayerNear;
        }

        public override bool PostCondition(WorldState next)
        {
            return next.distanceTravelled >= minTravelDistance;
        }

        public override void RefreshPriority()
        {
            // Most of the time we will want to be exploring for new targets if higher priority goals cannot be completed.
            // However if inventory full then highest priority goal is most likely StockFood
            // Thus if no valid plan for StockFood we will be near homeTree and it will be occupied 
            // So want to wander around it and wait instead of explore
            // Note: Explore Priority is opposite
            
            priority = Controller.curState.isInventoryFull ? PriorityLevel.Low : PriorityLevel.VeryLow;
        }

        public override float GetCost(WorldState next, float cost)
        {
            return -cost;
        }
    }
}