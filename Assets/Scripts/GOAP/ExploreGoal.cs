using UnityEngine;
using UnityEngine.Serialization;

namespace GOAP
{
    public class ExploreGoal : BaseGoal
    {
        [SerializeField] private float minExploreDistance = 15f;
        
        public override bool PreCondition(WorldState cur)
        {
            return true;
        }

        public override bool PostCondition(WorldState next)
        {
            return Vector2.Distance(next.location, Controller.curState.location) >= minExploreDistance;
        }

        public override void RefreshPriority()
        {
            // Priority = 10;
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