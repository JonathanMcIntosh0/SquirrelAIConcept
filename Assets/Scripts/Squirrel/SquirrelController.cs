using System.Collections.Generic;
using Actions;
using UnityEngine;
using UnityEngine.AI;
using NavMeshBuilder = UnityEditor.AI.NavMeshBuilder;

namespace Squirrel
{
    public class SquirrelController : MonoBehaviour
    {
        public NavMeshAgent agent;

        public int squirrelID;
        public float climbSpeed = 5f;
    
        public SquirrelMemory Memory;
        public WorldVector WorldState;
        //TODO add Goal field
        public Queue<ISquirrelAction> Path = new Queue<ISquirrelAction>();


        // private Vector3 target;
        // private Vector3 tmpPos; //Position on NavMesh. Used when climbing trees.

        // Start is called before the first frame update
        void Start()
        {
            SetSpawnPoint();
            InitAgent();
            Memory.UpdateMemory();
        
            // TODO Init world state
            // TODO Get new Goal and Compute path using path planner (Path planer should compute path whenever set new goal)
        
        
        
        
            // SetAction(ISquirrelAction.Idle);
            //
            // var idx = Random.Range(0, 10);
            // SetAction(ISquirrelAction.ToTree, idx);
            //
            // target = GameModel.Trees[idx].transform.position;
            // // target.y = GameModel.MaxSquirrelHeight;
            //
            // agent.SetDestination(target);
        }

        // Update is called once per frame
        void Update()
        {
            //Update memory
            Memory.UpdateMemory();
        
            // TODO Update world state accordingly

            if (Path.Count == 0 || WorldState.IsPlayerNear)
            {
                //TODO set new Goal and repath
                return;
            }

            // Check if need to repath (Check if world vector does not satisfy precondition of current action)
            if (!Path.Peek().PreCondition(WorldState))
            {
                //TODO REPATH (For current Goal)
                return;
            }

            //Execute action
            if (Path.Peek().Execute(this) && !Path.Dequeue().PostCondition(WorldState))
            {
                //Unwanted outcome (since PostCondition returned false) so repath
                //TODO REPATH (For current Goal)
            }
        
        
        
        
            // do
            // {
            //     //Check if just finished current action
            //     if (Path.Count > 0 && Path.Peek().PostCondition(worldState)) Path.Dequeue();
            //     
            //     // Check if need to repath (Check if finished current path or world vector does not satisfy precondition of current action)
            //     if (Path.Count == 0 || !Path.Peek().PreCondition(worldState))
            //     {
            //         //TODO REPATH (For current Goal)
            //     }
            //     // If reached here then precondition is satisfied (since re-pathing guarantees this), thus we will execute the current action
            //     Path.Peek().Execute(gameObject);
            // } while (Path.Peek().PostCondition(worldState)); // If we were able to finish the action this frame then loop
        
        
        
            //
            //
            // if (!agent.updatePosition)
            // {
            //     //Move manually to target since off mesh
            //     var step = climbSpeed * Time.deltaTime;
            //     transform.position = Vector3.MoveTowards(transform.position, target, step);
            //
            //     if (Vector3.Distance(transform.position, target) < 0.001f)
            //     {
            //         //Arrived at target
            //         if (target.y == 0f)
            //         {
            //             //At Bottom
            //             agent.updatePosition = true;
            //             
            //             ClimbUpTree(); //For debugging
            //         }
            //         else
            //         {
            //             //At Top
            //             ClimbDownTree(); //For debugging
            //         }
            //     }
            // }
            // else
            // {
            //     if (!agent.pathPending && !agent.hasPath)
            //     {
            //         Debug.Log($"Squirrel {squirrelID}: path complete. Action: {Action}");
            //         if (Action.action == ISquirrelAction.ToTree && !agent.isOnOffMeshLink)
            //         {
            //             Debug.Log($"Squirrel {squirrelID}: Climbing up tree. Action: {Action}");
            //             ClimbUpTree();
            //         }
            //     }
            // }
        
        }

        // private void SetAction(ISquirrelAction a, int idx = 0)
        // {
        //     Action = (a, idx);
        // }

        private void SetSpawnPoint()
        {
            transform.position = Memory.HomeTree.transform.position;
        }

        //Used once squirrel has arrived at base of tree
        // private void ClimbUpTree()
        // {
        //     if (agent.updatePosition) agent.updatePosition = false;
        //     target = transform.position;
        //     target.y = GameModel.MaxSquirrelHeight;
        // }
    
        // Use if squirrel is at home point of tree[idx]
        // private void ClimbDownTree()
        // {
        //     if (agent.updatePosition) agent.updatePosition = false;
        //     
        //     target = transform.position;
        //     target.y = 0f;
        // }
    
        private void InitAgent()
        {
            if (NavMesh.SamplePosition(transform.position, out var closestHit, 5f, NavMesh.AllAreas))
            {
                transform.position = closestHit.position;
                gameObject.AddComponent<NavMeshAgent>();
                agent = gameObject.GetComponent<NavMeshAgent>();
                agent.agentTypeID = 0;
                agent.height = 1;
                agent.radius = 0.3f;
                agent.updatePosition = true;
                // agent.autoTraverseOffMeshLink = true;

            } else Debug.LogError("Could not find position on NavMesh!");
        }
    
        /*
     * Path planner - Sets goals and paths to reach those goals
     */
        // private class PathPlanner
        // {
        //     //TODO change output to Goal type and implement
        //     public void GetGoal()
        //     {
        //     
        //     }
        //
        // }
    }
}
