
/*
 * Squirrel Actions have:
 * - execute functions (what to do each update if not completed yet)
 * - valid check functions (takes as input world vector and checks against precondition)
 * - completion check functions (takes as input world vector and checks against postcondition)
 */

using System;
using Squirrel;
using UnityEngine;

namespace Actions
{
    public interface ISquirrelAction
    {
        public bool PreCondition(WorldVector state);
        public bool PostCondition(WorldVector state);
        // public Func<WorldVector, bool> PostCondition;

        //Used for path planning
        public WorldVector Simulate(WorldVector state);

        //Execute action and return true if action was completed (this frame), false otherwise
        public bool Execute();


        //TODO might use for A*
        // public float Cost(GameObject squirrel);


        // public SquirrelAction(Func<WorldVector, bool> pre, Func<WorldVector, bool> post, Action exec)
        // {
        //     PreCondition = pre;
        //     PostCondition = post;
        //     Execute = exec;
        // }

        /*
         * POSSIBLE SQUIRREL ACTIONS 
        */

        // public static SquirrelAction GoToLocation = new SquirrelAction(
        //     pre: v =>
        //     {
        //         return true;
        //     },
        //     post: v =>
        //     {
        //         return true;
        //     },
        //     exec: () =>
        //     {
        //         
        //     }
        // )
    }
}
