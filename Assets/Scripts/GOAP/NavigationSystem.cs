using System;
using UnityEngine;
using UnityEngine.AI;

namespace GOAP
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
        private Vector3? _curTarget = null;
        public bool reachedDestination = false;
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
            var position = transform.position;
            reachedDestination = position == destination;
            if (reachedDestination)
            {
                destinationSet = false;
                return;
            }

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
                transform.position = Vector3.MoveTowards(transform.position, _curTarget.Value, step);
            }
            
            if (_curTarget.Value == transform.position) 
                _curTarget = null;
        }

        private void LateUpdate()
        {
            if (!_planner.HasPlan) destinationSet = false; // Stop moving
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
    }
}