using GarbageCan;
using Nut;
using Tree;
using UnityEngine;
using Random = UnityEngine.Random;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

namespace GOAP.Agent
{
    [RequireComponent(typeof(PathPlanner), 
        typeof(NavigationSystem), 
        typeof(TargetingSystem))]
    public class SController : MonoBehaviour
    {
        [Header("Components")]
        // TODO check what components can be removed. Might just do GetComponent within BaseAction and Goal.
        // TODO Probably can make this class a "StateModifier" and remove notion of "main controller"
        // public PathPlanner planner;
        // public NavigationSystem navSystem;
        // public TargetingSystem tarSystem;
        public PlayerController playerController;

        [Header("Settings")]
        [SerializeField] private int nutInventorySize = 3;
        [SerializeField] private int garbageInventorySize = 1;

        [Header("State info")]
        public WorldState curState;
        public bool isTrapped;
        private GarbageCanController _canController; // Used to check if still trapped

        private void Awake()
        {
            // planner = GetComponent<PathPlanner>();
            // navSystem = GetComponent<NavigationSystem>();
            // tarSystem = GetComponent<TargetingSystem>();
            playerController = GameObject.FindWithTag("Player").GetComponent<PlayerController>();
        }

        private void Start()
        {
            // Init curState
            curState.SetLocation(transform.position);
        }

        private void LateUpdate()
        {
            if (isTrapped && _canController.State != State.Trap)
            {
                isTrapped = false;
                _canController = null; // Unnecessary but for safety
            }
            
            // Update state location
            UpdateStateLocation(transform.position);
        }

        //************************************************************************//
        //                  CURRENT STATE MODIFYING METHODS
        //************************************************************************//
        
        private void UpdateStateLocation(Vector3 location)
        {
            bool isPlayerNear = Vector3.Distance(location, playerController.transform.position) 
                                <= GameModel.SquirrelViewDistance;
            SetIsPlayerNear(ref curState, isPlayerNear);
            MoveStateTo(ref curState, location.GetHorizVector2(), location.y);
        }

        public void PickUpNut(NutController nutController)
        {
            AddNut(ref curState);
            nutController.PickUp();
        }

        public bool PickUpGarbage(GarbageCanController canController)
        {
            if (canController.TryGetGarbage())
            {
                AddGarbage(ref curState);
                return true;
            }
            // if here then trapped
            _canController = canController;
            isTrapped = true;
            return false;
        }
        
        public void StoreInventory(TreeController treeController)
        {
            // TODO maybe keep track of how much is stored in tree
            EmptyInventory(ref curState);
            SetStoredFlag(ref curState); // Unnecessary
        }

        //************************************************************************//
        //                  GENERAL STATE MODIFYING METHODS
        //************************************************************************//
        
        public void SetIsPlayerNear(ref WorldState cur, bool isPlayerNear)
        {
            cur.isPlayerNear = isPlayerNear;
        }
        
        public void MoveStateTo(ref WorldState cur, Vector2 location, float height)
        {
            cur.distanceTravelled += Vector2.Distance(cur.location, location);
            cur.location = location;
            cur.height = height;
        }

        public void AddNut(ref WorldState cur)
        {
            cur.nutsCarried++;
            cur.inventoryFill = (float) cur.nutsCarried / nutInventorySize 
                                + (float) cur.garbageCarried / garbageInventorySize;
            cur.isInventoryFull = cur.inventoryFill + float.Epsilon >= 1f; // Add epsilon in case of floating point error
        }

        public void AddGarbage(ref WorldState cur)
        {
            cur.garbageCarried++;
            cur.inventoryFill = (float) cur.nutsCarried / nutInventorySize 
                                + (float) cur.garbageCarried / garbageInventorySize;
            cur.isInventoryFull = cur.inventoryFill + float.Epsilon >= 1f; // Add epsilon in case of floating point error
        }

        public void SetStoredFlag(ref WorldState cur)
        {
            cur.hasStored = true;
        }
        
        public void EmptyInventory(ref WorldState cur)
        {
            cur.nutsCarried = 0;
            cur.garbageCarried = 0;
            cur.inventoryFill = 0;
            cur.isInventoryFull = false;
        }
        
    }
}