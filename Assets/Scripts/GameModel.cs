using System;
using System.Collections.Generic;
using GarbageCan;
using GOAP.Agent;
using Nut;
using Tree;
using UnityEngine;

//TODO PROBABLY REMOVE MONOBEHAVIOUR
public class GameModel : MonoBehaviour
{
    private static GameModel _instance;
    
    // TODO maybe make some of these values non serialized and get them on awake from prefabs and scene or something.
    // This would avoid issues if we change proportions within editor.

    [SerializeField] private float minX = 0f;
    public static float MinX => _instance.minX;
    [SerializeField] private float minZ = 0f;
    public static float MinZ => _instance.minZ;
    [SerializeField] private float maxX = 75f;
    public static float MaxX => _instance.maxX;
    [SerializeField] private float maxZ = 75f;
    public static float MaxZ => _instance.maxZ;
    
    private float squirrelRadius = 0.15f;
    public static float SquirrelRadius => _instance.squirrelRadius;
    private float treeTrunkRadius = 0.5f;
    public static float TreeTrunkRadius => _instance.treeTrunkRadius;
    private float treeRadius = 2.5f; //Including foliage
    public static float TreeRadius => _instance.treeRadius;
    private float nutRadius = 0.125f;
    public static float NutRadius => _instance.nutRadius;
    private float garbageCanRadius = 1f;
    public static float GarbageCanRadius => _instance.garbageCanRadius;
    
    //Want MinNutSpawnRadius to be large enough that squirrel cannot stand on a nut while next to tree (for detection reasons)
    public static float MinNutSpawnRadius => TreeTrunkRadius + 2*SquirrelRadius + NutRadius;
    public static float MaxNutSpawnRadius => TreeRadius - NutRadius; //Want Nut fully under tree

    private float squirrelHomeHeight = 5f;
    public static float SquirrelHomeHeight => _instance.squirrelHomeHeight;
    private float garbageCanHeight = 1f;
    public static float GarbageCanHeight => _instance.garbageCanHeight;
    private float floorHeight = 0.05f;
    public static float FloorHeight => _instance.floorHeight;

    [SerializeField] private int numGarbageCans = 5;
    public static int NumGarbageCans => _instance.numGarbageCans;
    [SerializeField] private int numTrees = 10;
    public static int NumTrees => _instance.numTrees;
    [SerializeField] private int numSquirrels = 5; // Must be <= NumTrees
    public static int NumSquirrels => _instance.numSquirrels;

    public static GarbageCanController[] GarbageCans;
    public static TreeController[] Trees;
    //TODO maybe make squirrels gameObjects since not really single controller but 3 (4 if you count planner)
    public static SController[] Squirrels; 
    public static LinkedList<NutController> Nuts;

    //TODO maybe move these to within squirrel controller (or targeting system)
    public static float SquirrelViewAngle = 45f;
    public static float SquirrelViewDistance = 10f;

    private void Awake()
    {
        if (_instance != null) throw new Exception("GameModel has already been created!");
        _instance = this;

        GarbageCans = new GarbageCanController[NumGarbageCans];
        Trees = new TreeController[NumTrees];
        Squirrels = new SController[NumSquirrels];
        Nuts = new LinkedList<NutController>();

        GetComponent<GameGenerator>().GenerateGame();
    }
}
