using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameModel : MonoBehaviour
{

    public const float Max_X = 75f;
    public const float Max_Z = 75f;
    
    public const float MaxSquirrelHeight = 5f; //Height of squirrel home
    public const float SquirrelRadius = 0.15f;
    public const float TreeTrunkRadius = 0.5f;
    public const float TreeRadius = 5f;
    public const float TreeNutRadius = 0.125f;

    public static GameObject[] GarbageCans = new GameObject[5];
    public static GameObject[] Trees = new GameObject[10];
    public static GameObject[] Squirrels = new GameObject[5];
    
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
