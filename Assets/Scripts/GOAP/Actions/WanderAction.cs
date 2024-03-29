﻿using System.Collections.Generic;
using UnityEngine;

namespace GOAP.Actions
{
    public class WanderAction : BaseAction
    {
        [SerializeField] private int numberOfTargets = 10;
        [SerializeField] private float randomRange = 3f;
        public override bool PreCondition(WorldState cur)
        {
            return !cur.isPlayerNear;
        }

        public override WorldState? CalculateState(WorldState cur, Target target)
        {
            Controller.MoveStateTo(ref cur, target.location, GameModel.FloorHeight);
            return cur;
        }

        public override List<Target> GetTargets(WorldState cur)
        {
            var targets = new List<Target>();
            for (int i = 0; i < numberOfTargets; i++)
            {
                targets.Add(TarSystem.GetRandomLocationTarget(randomRange, cur.location));
            }

            return targets;
        }

        protected override ActionResult Tick_ClimbUp(Target target)
        {  
            return ActionResult.Success;
        }

        protected override ActionResult Tick_Use(Target target)
        {
            return ActionResult.Success;
        }
    }
}