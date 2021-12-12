using System.Collections;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.AI;
using NavMeshBuilder = UnityEditor.AI.NavMeshBuilder;

public class SquirrelController : MonoBehaviour
{
    public NavMeshAgent agent;
    private Vector3 target;
    public (SquirrelAction action, int idx) Action;

    public int squirrelID;
    public SquirrelMemory Memory;
    

    public float climbSpeed = 5f;
    
    // private Vector3 tmpPos; //Position on NavMesh. Used when climbing trees.

    // Start is called before the first frame update
    void Start()
    {
        SetSpawnPoint();
        InitAgent();
        SetAction(SquirrelAction.Idle);

        var idx = Random.Range(0, 10);
        SetAction(SquirrelAction.ToTree, idx);
        
        target = GameModel.Trees[idx].transform.position;
        // target.y = GameModel.MaxSquirrelHeight;
        
        agent.SetDestination(target);
    }

    // Update is called once per frame
    void Update()
    {
        //Update memory
        Memory.UpdateMemory();
        // Update world vector accordingly
        
        // Check if need to repath (Check if world vector still satisfies precondition of current action, or finished current plan)
        
        // While(curAction not finished) execute curAction
        // Ex: if curAction is Idle then check if 
        
        
        
        
        if (!agent.updatePosition)
        {
            //Move manually to target since off mesh
            var step = climbSpeed * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, target, step);

            if (Vector3.Distance(transform.position, target) < 0.001f)
            {
                //Arrived at target
                if (target.y == 0f)
                {
                    //At Bottom
                    agent.updatePosition = true;
                    
                    ClimbUpTree(); //For debugging
                }
                else
                {
                    //At Top
                    ClimbDownTree(); //For debugging
                }
            }
        }
        else
        {
            if (!agent.pathPending && !agent.hasPath)
            {
                Debug.Log($"Squirrel {squirrelID}: path complete. Action: {Action}");
                if (Action.action == SquirrelAction.ToTree && !agent.isOnOffMeshLink)
                {
                    Debug.Log($"Squirrel {squirrelID}: Climbing up tree. Action: {Action}");
                    ClimbUpTree();
                }
            }
        }
        
    }

    private void SetAction(SquirrelAction a, int idx = 0)
    {
        Action = (a, idx);
    }

    private void SetSpawnPoint()
    {
        transform.position = Memory.HomeTree.transform.position;
    }

    //Used once squirrel has arrived at base of tree
    private void ClimbUpTree()
    {
        if (agent.updatePosition) agent.updatePosition = false;
        target = transform.position;
        target.y = GameModel.MaxSquirrelHeight;
    }
    
    //Use if squirrel is at home point of tree[idx]
    private void ClimbDownTree()
    {
        if (agent.updatePosition) agent.updatePosition = false;
        
        target = transform.position;
        target.y = 0f;
    }
    
    private void InitAgent()
    {
        if (NavMesh.SamplePosition(transform.position, out var closestHit, 500f, NavMesh.AllAreas))
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
}
