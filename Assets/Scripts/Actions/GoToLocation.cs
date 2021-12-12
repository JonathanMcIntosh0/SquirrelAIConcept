using Squirrel;
using UnityEngine;
using UnityEngine.AI;

namespace Actions
{
    public class GoToLocation : SquirrelAction
    {
        // private SquirrelController _controller;
        private readonly Vector3 _target;

        private bool _isExecuting;

        public GoToLocation(SquirrelController controller, Vector3 target) : base(controller)
        {
            //Get point on NavMesh
            NavMesh.SamplePosition(target, out var closestHit, 5f, NavMesh.AllAreas);
            _target = closestHit.position;
        }

        public override bool PreCondition(WorldVector state)
        {
            return state.HState == WorldVector.HeightState.Floor &&
                   Mathf.Approximately(state.CurPosition.y, 0f); //TODO Might remove
        }

        public override bool PostCondition(WorldVector state)
        {
            return state.HState == WorldVector.HeightState.Floor &&
                   Vector3.Distance(state.CurPosition, _target) <= Mathf.Epsilon;
        }

        public override WorldVector Simulate(WorldVector state)
        {
            var newState = state;
            newState.CurPosition = _target;
            return newState;
        }

        public override bool Execute()
        {
            if (!_isExecuting)
            {
                _isExecuting = true;
                _controller.agent.SetDestination(_target);
            }

            return !_controller.agent.pathPending && !_controller.agent.hasPath;
        }
    }
}