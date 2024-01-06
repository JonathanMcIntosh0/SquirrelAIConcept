using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

//TODO PROBABLY REMOVE MONOBEHAVIOUR
public class GameModel : MonoBehaviour
{

    // TODO maybe get these values on awake from prefabs and scene or something.
    // This would avoid issues if we change proportions within editor

    public const float MinX = 0f;
    public const float MinZ = 0f;
    public const float MaxX = 75f;
    public const float MaxZ = 75f;
    
    public const float SquirrelRadius = 0.15f;
    public const float TreeTrunkRadius = 0.5f;
    public const float TreeRadius = 2.5f; //Including foliage
    public const float NutRadius = 0.125f;
    public const float GarbageCanRadius = 1f;
    
    //Want MinNutSpawnRadius to be large enough that squirrel cannot stand on a nut while next to tree (for detection reasons)
    public const float MinNutSpawnRadius = TreeTrunkRadius + 2*SquirrelRadius + NutRadius;
    public const float MaxNutSpawnRadius = TreeRadius - NutRadius; //Want Nut fully under tree

    public const float SquirrelHomeHeight = 5f;
    public const float GarbageCanHeight = 1f;
    public const float FloorHeight = 0.05f;

    public static GameObject[] GarbageCans = new GameObject[5];
    public static GameObject[] Trees = new GameObject[10];
    public static GameObject[] Squirrels = new GameObject[5];
    public static LinkedList<GameObject> Nuts = new LinkedList<GameObject>();
    // TODO REDO NUT SPAWNING
    // TODO Create NutController which has field for LLNode (used in onDestroy method).
    // TODO Also hold either index of respective tree or ref to TreeController to decrement nut count.

    
    //TODO maybe move these to within squirrel controller
    public static float SquirrelViewAngle = 45f;
    public static float SquirrelViewDistance = 10f;

    
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
