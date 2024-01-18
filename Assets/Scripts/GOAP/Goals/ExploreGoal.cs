using UnityEngine;

namespace GOAP.Goals
{
    public class ExploreGoal : BaseGoal
    {
        [SerializeField] private float minExploreDistance = 15f;

        public override bool PreCondition(WorldState cur)
        {
            return !cur.isPlayerNear;
        }

        public override bool PostCondition(WorldState next)
        {
            return Vector2.Distance(next.location, Controller.curState.location) >= minExploreDistance;
        }

        public override void RefreshPriority()
        {
            // Most of the time we will want to be exploring for new targets if higher priority goals cannot be completed.
            // However if inventory full then highest priority goal is most likely StockFood
            // Thus if no valid plan for StockFood we will be near homeTree and it will be occupied 
            // So want to wander around it and wait instead of explore
            // Note: Wander Priority is opposite
            
            priority = Controller.curState.isInventoryFull ? PriorityLevel.Low : PriorityLevel.Medium;
        }

        public override float GetCost(WorldState next, float cost)
        {
            // if distanceTravelled == distanceFromCur then cost = -distanceFromCur
            // Ordering should be by largest distanceFromCur then if = we sort by smallest distanceTravelled 
            
            // if dFromCur == 0 then return 0;
            // dTravelled >= dFromCur > 0 =>  0 < dFromCur/dTravelled <= 1
            // -cost = dFromCur - (1 - dFromCur/dTravelled)
            
            // -cost = dFromCur * (dFromCur/dTravelled)
            var distanceFromCur = Vector2.Distance(next.location, Controller.curState.location);
            return cost == 0 ? 0 : -distanceFromCur * distanceFromCur / cost;
            // return cost == 0 ? 0 : -(distanceFromCur - (1 - distanceFromCur / cost));
        }
    }
}