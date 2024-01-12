using System.Collections;
using System.Collections.Generic;
using GarbageCan;
using GOAP;
using GOAP.Agent;
using Nut;
using Tree;
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

    public const int NumGarbageCans = 5;
    public const int NumTrees = 10;
    public const int NumSquirrels = 5; // Must be <= NumTrees

    public static GarbageCanController[] GarbageCans = new GarbageCanController[NumGarbageCans];
    public static TreeController[] Trees = new TreeController[NumTrees];
    //TODO maybe make squirrels gameObjects since not really single controller but 3 (4 if you count planner)
    public static SController[] Squirrels = new SController[NumSquirrels]; 
    public static LinkedList<NutController> Nuts = new LinkedList<NutController>();

    //TODO maybe move these to within squirrel controller (or targeting system)
    public static float SquirrelViewAngle = 45f;
    public static float SquirrelViewDistance = 10f;
}
