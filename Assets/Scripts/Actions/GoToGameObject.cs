using Squirrel;
using UnityEngine;
using UnityEngine.AI;

namespace Actions
{
    public class GoToGameObject : SquirrelAction
    {
        // private SquirrelController _controller;
        private Vector3 _target;
        private GameObject _targetGO;
        private IClimbable _climbable;

        private bool _checkedClimbableObject; //Used to avoid frequent calls to TryGetComponent
        private bool _isExecuting;

        public GoToGameObject(SquirrelController controller, GameObject targetGO) : base(controller)
        {
            _targetGO = targetGO;
            
            //Get point on NavMesh
            NavMesh.SamplePosition(targetGO.transform.position, out var closestHit, 5f, NavMesh.AllAreas);
            _target = closestHit.position;
        }

        public override bool PreCondition(WorldVector state)
        {
            if (_targetGO == null) return false; //Object was destroyed (or is missing)
            
            //Check if occupied
            if (!_checkedClimbableObject)
            {
                _checkedClimbableObject = true;
                _targetGO.TryGetComponent<>(out _climbable);
            }
            if (_climbable != null && _climbable.IsOccupied) return false;

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