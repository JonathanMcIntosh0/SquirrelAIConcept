using System;
using UnityEngine;

/*
 * A world vector - this value type is used for current world vectors, and post and preconditions.
 * All fields should be nullable, if a field is null this means that it need not matter when checking post and pre conditions.
 */
public struct WorldVector
{
    public GameObject Squirrel; //Squirrel the world vector is attached to (TODO Might delete)
    public SquirrelMemory Memory;
    public bool IsPlayerNear;


    public bool CheckCondition(WorldVector cond)
    {
        //Check through every field. If cond.field != null && cond.field != this.field then 
        return true;
    }

    //Add parameter for each field of world vector
    public static Func<WorldVector, bool> CreateCondition(
        Func<SquirrelMemory, bool> memoryF = null,
        Func<bool, bool> isPlayerNearF = null
        )
    {
        return x =>
        {
            if (memoryF != null && !memoryF(x.Memory)) return false;
            if (isPlayerNearF != null && !isPlayerNearF(x.IsPlayerNear)) return false;
            return true;
        };
    }
}