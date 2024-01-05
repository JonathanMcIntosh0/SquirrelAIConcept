using System;
using System.Collections.Generic;
using System.Linq;
using Tree;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

namespace GOAP
{
    [RequireComponent(typeof(PathPlanner), 
        typeof(NavigationSystem), 
        typeof(TargetingSystem))]
    public class SController : MonoBehaviour
    {
        [Header("Components")]
        // TODO check what components can be removed. Might just do GetComponent within BaseAction and Goal.
        // TODO Probably can make this class "StateModifier" and remove notion of "main controller"
        public PathPlanner planner;
        public NavigationSystem navSystem;
        public TargetingSystem tarSystem;
        public PlayerController playerController;
        
        // TODO probably move homeTree to targetingSystem
        public GameObject homeTree; // Needs to be set by GameGenerator
        
        [Header("Settings")]
        [SerializeField] private int nutInventorySize = 3;
        [SerializeField] private int garbageInventorySize = 1;

        [Header("State info")]
        public WorldState curState; // TODO maybe move to PathPlanner

        // [SerializeField] public bool isInventoryFull => curState.nutsCarried >= 3;

        private void Awake()
        {
            planner = GetComponent<PathPlanner>();
            navSystem = GetComponent<NavigationSystem>();
            tarSystem = GetComponent<TargetingSystem>();
            playerController = GameObject.FindWithTag("Player").GetComponent<PlayerController>();
        }

        private void Start()
        {
            // Init curState
            curState.SetLocation(transform.position);
        }

        private void LateUpdate()
        {
            MoveStateTo(ref curState, transform.position);
        }

        public void MoveStateTo(ref WorldState cur, Vector3 location)
        {
            cur.distanceTravelled += Vector2.Distance(cur.location, location.GetHorizVector2());
            cur.isPlayerNear = Vector3.Distance(location, playerController.transform.position) <=
                               GameModel.SquirrelViewDistance;
            cur.SetLocation(location);
        }

        public void AddNut(ref WorldState cur)
        {
            cur.nutsCarried++;
            cur.isInventoryFull = cur.nutsCarried >= nutInventorySize;
        }

        public void AddGarbage(ref WorldState cur)
        {
            cur.garbageCarried++;
            cur.isInventoryFull = cur.garbageCarried >= garbageInventorySize;
        }
        
    }
}