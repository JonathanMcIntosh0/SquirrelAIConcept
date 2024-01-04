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
    public class SController : MonoBehaviour
    {
        [Header("Components")]
        public PathPlanner planner;
        public NavMeshAgent agent;
        public GameObject homeTree; // Needs to be set by GameGenerator
        
        [Header("Constants")]
        [SerializeField] private float climbSpeed = 5f;
        
        [SerializeField] private int nutMemorySize = 5;
        [SerializeField] private int garbageCanMemorySize = 2;
        [SerializeField] private int treeMemorySize = 1;
        private readonly int[] _maxMemSizes;

        [SerializeField] private int nutInventorySize = 3;
        [SerializeField] private int garbageInventorySize = 1;
        
        [Header("State info")]
        public WorldState curState;

        [SerializeField] public bool isInventoryFull => curState.nutsCarried >= 3;

        
        [Header("Navigation")]
        [SerializeField] private Vector2 offMeshDestination;
        [SerializeField] private Vector3 destination;
        private Vector3? _curTarget = null;
        public bool reachedDestination = false;
        public bool destinationSet = false;

        [Header("Targets")]
        public Target homeTreeTarget; // Needs to be created on start
        [SerializeField] private List<Target> targetsInFOV = new List<Target>();
        [SerializeField] private List<Target> targetsInMemory = new List<Target>();

        public SController()
        {
            _maxMemSizes = new[] {
                nutMemorySize,           // TargetType.Nut = 0
                garbageCanMemorySize,    // TargetType.GarbageCan = 1
                treeMemorySize           // TargetType.Tree = 2
            };
        }

        private void Awake()
        {
            Random.InitState(DateTime.Now.Millisecond); //Set random seed
            // agent = GetComponent<NavMeshAgent>();
            planner = GetComponent<PathPlanner>();
        }

        private void Start()
        {
            homeTreeTarget = new Target(homeTree, TargetType.HomeTree, TargetState.InMemory);
            
            // var spawnPos = homeTree.transform.position;
            // spawnPos += Vector3.forward * (GameModel.TreeTrunkRadius + GameModel.SquirrelRadius);
            // transform.position = spawnPos;
            
            if (NavMesh.SamplePosition(homeTree.transform.position, out var closestHit, 5f, NavMesh.AllAreas))
            {
                transform.position = closestHit.position;
                gameObject.AddComponent<NavMeshAgent>();
                agent = gameObject.GetComponent<NavMeshAgent>();
                agent.agentTypeID = 0;
                agent.height = 1;
                agent.radius = 0.3f;
                // agent.updatePosition = true;
                // agent.autoTraverseOffMeshLink = true;

            } else Debug.LogError("Could not find spawn position on NavMesh!");

            // Init curState
            curState.location = transform.position.GetHorizVector2();
            curState.height = transform.position.y;
        }

        private void Update()
        {
            MoveToDestination();
        }

        private void LateUpdate()
        {
            UpdateTargets();
            UpdateWorldState();
        }

        public void SetDestination(Vector2 loc, float height)
        {
            if (Math.Abs(height - destination.y) < 0.000001f && loc == offMeshDestination)
                return;

            offMeshDestination = loc;
            
            var sourcePos = new Vector3(loc.x, GameModel.FloorHeight, loc.y);
            NavMesh.SamplePosition(sourcePos, out var closestHit, 5f, NavMesh.AllAreas);

            destination = closestHit.position;
            destination.y = height;
            destinationSet = true;
        }

        public Target GetRandomLocationTarget(float range, Vector2 fromLoc)
        {
            Vector3 source = new Vector3(fromLoc.x, GameModel.FloorHeight, fromLoc.y);
            Vector2 targetLoc = fromLoc;
            
            source.x += Random.Range(-range, range);
            source.z += Random.Range(-range, range);

            if (NavMesh.SamplePosition(source, out var closestHit, 5f, NavMesh.AllAreas))
                targetLoc = closestHit.position.GetHorizVector2();
            
            return new Target(targetLoc);
        }
        
        public List<Target> GetTargetsOfType(TargetType type)
        {
            if (!Target.IsDetectable(type)) 
                return null;
            
            return new List<Target>(
                targetsInFOV
                    .Concat(targetsInMemory)
                    .Where(target => target.type == type));
        }

        private void MoveToDestination()
        {
            if (!destinationSet) return;
            var position = transform.position;
            reachedDestination = position == destination;
            if (reachedDestination)
            {
                destinationSet = false;
                return;
            }

            if (_curTarget == null)
            {
                // Get next target
                // target can be:
                // targetFloor = Pos(HPos(transform.position), FloorHeight)
                // targetOnMesh = Pos(HPos(destination), FloorHeight)
                // targetDest = destination
                
                // if nearDest then targetDest
                // if !nearDest and onFloor then targetOnMesh
                // if !nearDest and !onFloor then targetFloor

                if (destination.GetHorizVector2() == position.GetHorizVector2())
                {
                    _curTarget = destination;
                    agent.updatePosition = false;
                    agent.updateRotation = false;
                    agent.isStopped = true;
                    curState.location = offMeshDestination;
                }
                else if (Math.Abs(position.y - GameModel.FloorHeight) < 0.000001f)
                {
                    _curTarget = new Vector3(destination.x, GameModel.FloorHeight, destination.z);
                    agent.updatePosition = true;
                    agent.updateRotation = true;
                    agent.isStopped = false;
                    if (agent.destination != _curTarget) agent.SetDestination(_curTarget.Value);
                    curState.height = GameModel.FloorHeight;
                }
                else
                {
                    _curTarget = new Vector3(position.x, GameModel.FloorHeight, position.z);
                }
            }
            
            // Move vertically towards curTarget 
            if (agent.isStopped)
            {
                var step = climbSpeed * Time.deltaTime;
                transform.position = Vector3.MoveTowards(transform.position, _curTarget.Value, step);
            }
            
            if (_curTarget.Value == transform.position) 
                _curTarget = null;
        }

        private void UpdateTargets()
        {
            Dictionary<GameObject, Target> newTargetsInFOV = new Dictionary<GameObject, Target>();
            List<Target> newTargetsInMem = new List<Target>();

            int[] counts = {0, 0, 0};
            var transform1 = transform;
            var curPos = transform1.position;
            var curForward = transform1.forward;

            foreach (var target in targetsInFOV.Concat(targetsInMemory))
            {
                if (IsInFOV(curForward, curPos, target.location)) 
                {
                    if (target.obj == null) continue; // Skip collected nuts
                    newTargetsInFOV[target.obj] = target;
                    target.state = TargetState.InFOV;
                } 
                else if (counts[(int) target.type]++ < _maxMemSizes[(int) target.type])
                {
                    newTargetsInMem.Add(target); //Add to end not front
                    target.state = TargetState.InMemory;
                } 
                else 
                {
                    target.state = TargetState.Forgotten;
                }
            }

            var objQuery = 
                from obj in GameModel.Nuts.Concat(GameModel.GarbageCans).Concat(GameModel.Trees)
                where !newTargetsInFOV.ContainsKey(obj) && IsInFOV(curForward, curPos, obj.transform.position)
                select obj;

            foreach (var obj in objQuery)
            {
                // Maybe force a replan to happen here (in case better cost plan)
                newTargetsInFOV[obj] = new Target(obj);
            }
            
            targetsInFOV = newTargetsInFOV.Values.ToList();
            targetsInMemory = newTargetsInMem;
            
            //Update homeTree target
            homeTreeTarget.state = newTargetsInFOV.ContainsKey(homeTree) ? TargetState.InFOV : TargetState.InMemory;
        }

        private bool IsInFOV(Vector3 curForward, Vector3 curPos, Vector3 posToCheck)
        {
            var angle = Vector3.Angle(curForward, posToCheck - curPos);
            var distance = Vector3.Distance(curPos, posToCheck);
            return distance <= GameModel.SquirrelViewDistance
                   && angle <= GameModel.SquirrelViewAngle;
        }

        private void UpdateWorldState()
        {
            // Update Location parts of State
            curState.distanceTravelled += Vector2.Distance(curState.location, transform.position.GetHorizVector2());
            
            if (agent.isStopped) curState.height = transform.position.z;
            else curState.location = transform.position.GetHorizVector2();

            if (curState.hasReplanned)
            {
                curState.distanceTravelled = 0f;
                curState.hasReplanned = false;
            }
            
            // Update Inventory part of State
            curState.isInventoryFull = curState.nutsCarried >= nutInventorySize
                                       || curState.garbageCarried >= garbageInventorySize;
        }
    }
}