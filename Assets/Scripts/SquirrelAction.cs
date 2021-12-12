
/*
 * Squirrel Actions have:
 * - execute functions (what to do each update if not completed yet)
 * - valid check functions (takes as input world vector and checks against precondition)
 * - completion check functions (takes as input world vector and checks against postcondition)
 */

using System;

public class SquirrelAction
{
    public Func<WorldVector, bool> PreCondition;
    public Func<WorldVector, bool> PostCondition;
    // public bool PostCondition(WorldVector state);

    public Action Execute;

    public SquirrelAction(Func<WorldVector, bool> pre, Func<WorldVector, bool> post, Action exec)
    {
        PreCondition = pre;
        PostCondition = post;
        Execute = exec;
    }

    /*
     * POSSIBLE SQUIRREL ACTIONS 
    */
    
    public static SquirrelAction GoToLocation = new SquirrelAction(
        pre: v =>
        {
            return true;
        },
        post: v =>
        {
            return true;
        },
        exec: () =>
        {
            
        }
    )
}