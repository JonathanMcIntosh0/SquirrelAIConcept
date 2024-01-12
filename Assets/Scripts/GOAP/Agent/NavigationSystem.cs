using System;
using UnityEngine;
using UnityEngine.AI;

namespace GOAP.Agent
{
    [RequireComponent(typeof(PathPlanner))]
    public class NavigationSystem : MonoBehaviour
    {
        private PathPlanner _planner;
        [SerializeField] private NavMeshAgent agent;
        
        [SerializeField] private float climbSpeed = 5f;

        [Header("Navigation")]
        [SerializeField] private Vector2 offMeshDestination;
        [SerializeField] private Vector3 destination;
        [SerializeField] private IClimbable _curObj = null;
        private Vector3? _curTarget = null;
        public bool ReachedDestination => transform.position == destination;
        public bool destinationSet = false;

        private void Awake()
        {
            _planner = GetComponent<PathPlanner>();
        }

        private void Start()
        {
            // transform.position should be set to homeTree.transform.position by Instantiate()
            if (NavMesh.SamplePosition(transform.position, out var closestHit, 5f, NavMesh.AllAreas))
            {
                transform.position = closestHit.position;
                agent = gameObject.AddComponent<NavMeshAgent>();
                agent.agentTypeID = 0;
                agent.height = 1;
                agent.radius = 0.3f;
            } else Debug.LogError("Could not find spawn position on NavMesh!");
        }

        private void Update()
        {
            if (!destinationSet) return;
            if (ReachedDestination)
            {
                StopMoving();
                return;
            }
            var position = transform.position;
            
            if (_curTarget == null)
            {
                if (destination.GetHorizVector2() == position.GetHorizVector2())
                {
                    // Move vertically UP
                    _curTarget = destination;
                    agent.updatePosition = false;
                    agent.updateRotation = false;
                    agent.isStopped = true;
                    // curState.location = offMeshDestination;
                }
                else if (Math.Abs(position.y - GameModel.FloorHeight) < 0.000001f)
                {
                    // Move horizontally
                    _curTarget = new Vector3(destination.x, GameModel.FloorHeight, destination.z);
                    agent.updatePosition = true;
                    agent.updateRotation = true;
                    agent.isStopped = false;
                    if (agent.destination != _curTarget) agent.SetDestination(_curTarget.Value);
                    // curState.height = GameModel.FloorHeight;
                }
                else
                {
                    // Move vertically DOWN
                    _curTarget = new Vector3(position.x, GameModel.FloorHeight, position.z);
                }
            }
            
            if (agent.isStopped)
            {
                // Move vertically towards curTarget 
                var step = climbSpeed * Time.deltaTime;
                transform.position = Vector3.MoveTowards(position, _curTarget.Value, step);
            }

            if (_curObj != null && Math.Abs(transform.position.y - GameModel.FloorHeight) < 0.000001f)
            {
                ((IClimbable) _curObj).IsOccupied = false;
                _curObj = null;
            }

            if (_curTarget.Value == transform.position)
                _curTarget = null;
        }

        private void LateUpdate()
        {
            if (!_planner.HasPlan) StopMoving(); // Stop moving
        }

        private void StopMoving()
        {
            destinationSet = false;
            _curTarget = null;
            agent.isStopped = true;
        }

        public void SetHorizDestination(Vector2 loc)
        {
            if (destinationSet && loc == offMeshDestination) return; // Destination already set
            offMeshDestination = loc;
            
            // Check if currently on climbable obj
            if (_curObj != null && _curObj.transform.position.GetHorizVector2() == loc)
                destination = transform.position;
            else
            {
                var sourcePos = new Vector3(loc.x, GameModel.FloorHeight, loc.y);
                NavMesh.SamplePosition(sourcePos, out var closestHit, 5f, NavMesh.AllAreas);

                destination = closestHit.position;
                destination.y = GameModel.FloorHeight;
            }

            _curTarget = null;
            destinationSet = true;
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns>false if cannot climb, otherwise true</returns>
        public bool ClimbUp(IClimbable obj)
        {
            if (!CanClimb(obj)) return false;
            // if (curObj == obj)
            // {
            //     destination = transform.position;
            //     destination.y = climbable.MaxHeight;
            //     return true;
            // }
            
            // Note: We don't check horizontal position. We just assume we are at the obj we want to climb.
            // This means the agent will climb even if pushed away from obj after reaching it.
            
            obj.IsOccupied = true;
            _curObj = obj;
            
            destination = transform.position;
            destination.y = obj.MaxHeight;
            _curTarget = null;
            destinationSet = true;
            return true;
        }

        public bool CanClimb(IClimbable obj)
        {
            return _curObj == obj || !obj.IsOccupied;
        }
    }
}