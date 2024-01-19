using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GOAP.Actions;
using GOAP.Goals;
using UnityEngine;

namespace GOAP.Agent
{
    public enum PlanResult
    {
        // Unknown, //TODO maybe use unknown to show when between actions (currently just use Running)
        Running,
        Success,
        Fail
    }
    
    [RequireComponent(typeof(SController), 
        typeof(NavigationSystem), 
        typeof(TargetingSystem))]
    public class PathPlanner : MonoBehaviour
    {
        private class Node
        {
            public Node Parent;

            public BaseAction Action;
            public Target Target;
            public WorldState State;
            public float Cost; // Goal cost

            private readonly float _totalCost; // Action cost

            public Node(BaseGoal goal, WorldState state, BaseAction action = null, Target target = null, Node parent = null)
            {
                Action = action;
                State = state;
                Parent = parent;
                Target = target;
                _totalCost = parent == null ? 0f : action.GetCost(parent.State, target) + parent._totalCost;
                Cost = parent == null ? 0f : goal.GetCost(state, _totalCost);
            }
            
            public bool HasUsedTarget(Target target)
            {
                if (Parent == null) return false;
                return Target == target || Parent.HasUsedTarget(target);
            }
        }

        private class Plan
        {
            public BaseGoal Goal { get; private set; }
            public List<BaseAction> Actions { get; private set; }
            public List<Target> Targets { get; private set; }
            public float Cost { get; }

            private int _curIndex = 0;
            private BaseAction CurAction => Actions[_curIndex];
            private Target CurTarget => Targets[_curIndex];
            
            public Plan(BaseGoal goal, Node endNode)
            {
                Goal = goal;
                Cost = endNode.Cost;

                // build the action set
                Node currentNode = endNode;
                Actions = new List<BaseAction>();
                Targets = new List<Target>();
                while (currentNode.Parent != null)
                {
                    Actions.Insert(0, currentNode.Action);
                    Targets.Insert(0, currentNode.Target);
                    currentNode = currentNode.Parent;
                }
            }

            public PlanResult Tick()
            {
                var result = CurAction.Tick(CurTarget);
                return result switch
                {
                    ActionResult.Running => PlanResult.Running,
                    ActionResult.Fail => PlanResult.Fail,
                    ActionResult.Success => 
                        ++_curIndex == Actions.Count ? PlanResult.Success : PlanResult.Running,
                    _ => throw new ArgumentOutOfRangeException()
                };
            }

            public void ResetActions()
            {
                Actions.ForEach(action => action.Reset());
            }

            public override string ToString()
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine($"Goal = {Goal}, Cost = {Cost}");
                sb.AppendLine($"Current Action = {_curIndex}");
                sb.AppendLine("Action List:");
                for (int i = 0; i < Actions.Count; i++)
                {
                    sb.AppendLine($"[{i}]{Actions[i]} - {Targets[i]}");
                }
                return sb.ToString();
            }
        }

        private SController _controller;
        private WorldState _curState;
        [SerializeField] private List<BaseGoal> goals;
        [SerializeField] private List<BaseAction> actions;

        [SerializeField] private int maxIterations = 100; 
        
        private Plan _curPlan = null;
        public bool HasPlan => _curPlan != null;
        public string PlanText => _curPlan != null ? _curPlan.ToString() : "No current plan!";

        private void Awake()
        {
            goals = new List<BaseGoal>(GetComponents<BaseGoal>());
            actions = new List<BaseAction>(GetComponents<BaseAction>());

            _controller = GetComponent<SController>();
        }
        
        private void Update()
        {
            if (_controller.isTrapped) // Do not plan while trapped
            {
                _curPlan = null; // Stop current plan
                return;
            } 
            
            goals.ForEach(goal => goal.RefreshPriority());
            _curState = _controller.curState;
            
            // TODO don't actually need to find highest priority goal. Just see if HIGHER priority goal exists.
            var highestPriorityGoal = goals
                .Where(goal => goal.enabled && goal.PreCondition(_curState))
                .Aggregate(_curPlan?.Goal,
                    (newGoal, nextGoal) =>
                        newGoal is null || nextGoal.Priority > newGoal.Priority
                            ? nextGoal
                            : newGoal);
            
            if (_curPlan == null || _curPlan.Goal != highestPriorityGoal)
                Replan();
            
            if (_curPlan == null) return;
            var result = _curPlan.Tick();
            if (result == PlanResult.Success || result == PlanResult.Fail)
                _curPlan = null;
        }

        private void Replan()
        {
            // Debug.Log($"{gameObject.name}: Attempting to replan!");
            // _curState.distanceTravelled = 0; // Reset local curState.distanceTravelled before attempting replan
            _curState.ResetForPlanning();

            // Go through valid goals in descending order of priority (greater than curPlan) until we find a valid plan
            var newPlan =
                (from goal in goals
                where goal.enabled && goal.PreCondition(_curState)
                                   && (_curPlan == null || _curPlan.Goal.Priority < goal.Priority)
                orderby goal.Priority descending
                select BuildPlan(goal)).FirstOrDefault(plan => plan != null);

            // NOTE: Removed this since it doesn't make much sense.
            //       If curPlan != null and couldn't find valid plan with higher priority then just complete curPlan.
            //       Just added complexity to try replan for "lower cost" with same goal.
            // if (newPlan == null && _curPlan != null)
            // {
            //     var nextPlan = BuildPlan(_curPlan.Goal);
            //     // TODO maybe _curPlan.Cost should be "current cost" since curPlan could be partially complete
            //     if (nextPlan != null && nextPlan.Cost < _curPlan.Cost) newPlan = nextPlan;
            // }
            
            if (newPlan == null) return; // Continue running curPlan

            // Debug.Log($"{gameObject.name}: Found new plan: \n{newPlan}");
            
            _controller.curState.ResetForPlanning();
            // _controller.curState.hasReplanned = true; // Will cause curState.distanceTravelled to be reset on next update
            _curPlan = newPlan;
            _curPlan.ResetActions();
        }

        private Plan BuildPlan(BaseGoal goal)
        {
            if (goal.PostCondition(_curState)) return null;

            //TODO maybe make openList a minHeap
            LinkedList<Node> openList = new LinkedList<Node>();
            ExpandNode(openList, new Node(goal, _curState), goal);

            int iterations = 0;
            while (openList.Count > 0 && iterations++ < maxIterations)
            {
                var minNode = openList.First.Value;
                // var minNode = openList.Aggregate(
                //     (minCostNode, nextNode) => minCostNode.Cost > nextNode.Cost ? nextNode : minCostNode);

                if (goal.PostCondition(minNode.State))
                {
                    // Debug.Log($"{gameObject.name}: Found plan for {goal} - openList.Count = {openList.Count}, iter = {iterations}\n");
                    return new Plan(goal, minNode);
                }

                openList.RemoveFirst();
                // openList.Remove(minNode);
                ExpandNode(openList, minNode, goal);
            }

            Debug.Log($"{gameObject.name}: Could not find plan for {goal} - openList.Count = {openList.Count}, iter = {iterations}\n");
            return null;
        }
        
        private void ExpandNode(LinkedList<Node> openList, Node workingNode, BaseGoal goal)
        {
            IEnumerable<BaseAction> actionList;
            if (goal is WanderGoal || goal is ExploreGoal)
                actionList = actions.Where(action => action is WanderAction);
            else
                actionList = actions.Where(action => !(action is WanderAction));

            var nodeQuerry = from action in actionList
                where action.enabled && action.PreCondition(workingNode.State)
                from target in action.GetTargets(workingNode.State)
                where target.type != TargetType.Nut || !workingNode.HasUsedTarget(target)
                let nextState = action.CalculateState(workingNode.State, target)
                where nextState != null
                select new Node(goal, nextState.Value, action, target, workingNode);
            
            foreach (var node in nodeQuerry)
            {
                if (openList.Count == 0 || node.Cost <= openList.First.Value.Cost)
                {
                    openList.AddFirst(node);
                    continue;
                }
                
                var cur = openList.First;
                while (cur != openList.Last && cur.Value.Cost < node.Cost) 
                    cur = cur.Next;
                openList.AddAfter(cur, node);
            }
            
            //
            // openList.AddRange(
            //     from action in actionList 
            //     where action.enabled && action.PreCondition(workingNode.State) 
            //     from target in action.GetTargets(workingNode.State) 
            //     where target.type != TargetType.Nut || !workingNode.HasUsedTarget(target) 
            //     let nextState = action.CalculateState(workingNode.State, target) 
            //     where nextState != null 
            //     select new Node(goal, nextState.Value, action, target, workingNode));
        }
    }
}