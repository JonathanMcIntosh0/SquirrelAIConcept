using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Debug = System.Diagnostics.Debug;

namespace GOAP
{
    public enum PlanResults
    {
        // Unknown, //TODO maybe use unknown to show when between actions (currently just use Running)
        Running,
        Success,
        Fail
    }
    
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
                _totalCost = parent == null ? 0f : action.GetCost(state) + parent._totalCost;
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

            public PlanResults Tick(ref WorldState curState)
            {
                var result = CurAction.Tick(ref curState, CurTarget);
                return result switch
                {
                    ActionResults.Running => PlanResults.Running,
                    ActionResults.Fail => PlanResults.Fail,
                    ActionResults.Success => 
                        ++_curIndex == Actions.Count ? PlanResults.Success : PlanResults.Running,
                    _ => throw new ArgumentOutOfRangeException()
                };
            }

            public void ResetActions()
            {
                Actions.ForEach(action => action.Reset());
            }
        }
        
        private WorldState _curState = new WorldState(); // TODO Probably move to Character. Also maybe remove passing to Tick methods.
        private List<BaseGoal> _goals;
        private List<BaseAction> _actions;

        private Plan _curPlan = null;

        private void Awake()
        {
            _goals = new List<BaseGoal>(GetComponents<BaseGoal>());
            _actions = new List<BaseAction>(GetComponents<BaseAction>());
        }
        
        private void Update()
        {
            _goals.ForEach(goal => goal.RefreshPriority());
            
            // TODO don't actually need to find highest priority goal. Just see if HIGHER priority goal exists.
            var highestPriorityGoal = _goals
                .Where(goal => goal.PreCondition(_curState))
                .Aggregate(_curPlan?.Goal,
                    (newGoal, nextGoal) =>
                        newGoal == null
                        || nextGoal.Priority > newGoal.Priority
                            ? nextGoal
                            : newGoal);
            
            
            if (_curPlan == null || _curPlan.Goal != highestPriorityGoal)
                Replan();
            
            if (_curPlan == null) return;
            var result = _curPlan.Tick(ref _curState);
            if (result == PlanResults.Success || result == PlanResults.Fail)
                _curPlan = null;
        }

        private void Replan()
        {
            var planQuery =
                from goal in _goals
                where goal.PreCondition(_curState)
                let plan = BuildPlan(goal)
                where plan != null
                select plan;

            var newPlan = planQuery.Aggregate(_curPlan, 
                (newPlan, nextPlan) =>
                    newPlan == null
                    || newPlan.Goal.Priority < nextPlan.Goal.priority
                    || newPlan.Goal == nextPlan.Goal && newPlan.Cost > nextPlan.Cost
                        ? nextPlan
                        : newPlan);

            if (newPlan == _curPlan) return;
            
            _curPlan = newPlan;
            _curPlan.ResetActions();
        }

        private Plan BuildPlan(BaseGoal goal)
        {
            if (goal.PostCondition(_curState)) return null;

            List<Node> openList = new List<Node>();
            ExpandNode(openList, new Node(goal, _curState), goal);
            
            while (openList.Count > 0)
            {
                var minNode = openList.Aggregate(
                    (minCostNode, nextNode) => minCostNode.Cost > nextNode.Cost ? nextNode : minCostNode);

                if (goal.PostCondition(minNode.State)) 
                    return new Plan(goal, minNode);

                openList.Remove(minNode);
                ExpandNode(openList, minNode, goal);
            }

            return null;
        }
        
        private void ExpandNode(List<Node> openList, Node workingNode, BaseGoal goal)
        {
            openList.AddRange(
                from action in _actions 
                where action.PreCondition(workingNode.State) 
                from target in action.GetTargets() 
                where target.Type != TargetTypes.Nut || !workingNode.HasUsedTarget(target) 
                let nextState = action.CalculateState(workingNode.State, target) 
                where nextState != null 
                select new Node(goal, nextState.Value, action, target, workingNode));
        }
    }
}