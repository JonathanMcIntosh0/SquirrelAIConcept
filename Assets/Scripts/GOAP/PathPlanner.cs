using System;
using System.Collections.Generic;
using System.Linq;
using GOAP.Actions;
using GOAP.Goals;
using UnityEngine;
using UnityEngine.Serialization;

namespace GOAP
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
                var s = $"Goal = {Goal}, Cost = {Cost}\n";
                for (int i = 0; i < Actions.Count; i++)
                {
                    s += $"{Actions[i].name} - {Targets[i]}\n";
                }
                return s;
            }
        }

        private SController _controller;
        private WorldState _curState;
        [SerializeField] private List<BaseGoal> goals;
        [SerializeField] private List<BaseAction> actions;

        [SerializeField] private int maxIterations = 50; 
        
        private Plan _curPlan = null;
        public bool HasPlan => _curPlan != null;

        private void Awake()
        {
            goals = new List<BaseGoal>(GetComponents<BaseGoal>());
            actions = new List<BaseAction>(GetComponents<BaseAction>());

            _controller = GetComponent<SController>();
        }
        
        private void Update()
        {
            _curState = _controller.curState;
            goals.ForEach(goal => goal.RefreshPriority());
            
            // TODO don't actually need to find highest priority goal. Just see if HIGHER priority goal exists.
            var highestPriorityGoal = goals
                .Where(goal => goal.PreCondition(_curState))
                .Aggregate(_curPlan?.Goal,
                    (newGoal, nextGoal) =>
                        newGoal == null || nextGoal.Priority > newGoal.Priority
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

            var planQuery =
                from goal in goals
                where goal.PreCondition(_curState)
                let plan = BuildPlan(goal)
                where plan != null
                select plan;

            var newPlan = planQuery.Aggregate(_curPlan, 
                (newPlan, nextPlan) =>
                    newPlan == null
                    || newPlan.Goal.Priority < nextPlan.Goal.Priority
                    || newPlan.Goal == nextPlan.Goal && newPlan.Cost > nextPlan.Cost
                        ? nextPlan
                        : newPlan);

            if (newPlan == _curPlan) return;
            
            Debug.Log($"{gameObject.name}: Found new plan: \n{newPlan}");
            
            _controller.curState.ResetForPlanning();
            // _controller.curState.hasReplanned = true; // Will cause curState.distanceTravelled to be reset on next update
            _curPlan = newPlan;
            _curPlan.ResetActions();
        }

        private Plan BuildPlan(BaseGoal goal)
        {
            if (goal.PostCondition(_curState)) return null;

            List<Node> openList = new List<Node>();
            ExpandNode(openList, new Node(goal, _curState), goal);

            int iterations = 0;
            while (openList.Count > 0 && iterations++ < maxIterations)
            {
                var minNode = openList.Aggregate(
                    (minCostNode, nextNode) => minCostNode.Cost > nextNode.Cost ? nextNode : minCostNode);

                if (goal.PostCondition(minNode.State)) 
                    return new Plan(goal, minNode);

                openList.Remove(minNode);
                ExpandNode(openList, minNode, goal);
            }

            Debug.Log($"{gameObject.name}: Could not find plan - openList.Count = {openList.Count}, iter = {iterations}\n");
            return null;
        }
        
        private void ExpandNode(List<Node> openList, Node workingNode, BaseGoal goal)
        {
            openList.AddRange(
                from action in actions 
                where action.PreCondition(workingNode.State) 
                from target in action.GetTargets(workingNode.State) 
                where target.type != TargetType.Nut || !workingNode.HasUsedTarget(target) 
                let nextState = action.CalculateState(workingNode.State, target) 
                where nextState != null 
                select new Node(goal, nextState.Value, action, target, workingNode));
        }
    }
}