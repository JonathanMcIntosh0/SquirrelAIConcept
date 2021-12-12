using Squirrel;
using Tree;
using UnityEngine;
using UnityEngine.AI;

namespace Actions
{
    public class ClimbDownTree : SquirrelAction
    {
        // private SquirrelController _controller;
        private Vector3 _target;
        private Transform _tf;
        private bool _isExecuting;
        
        private IClimbable _climbable;
        
        public ClimbDownTree(SquirrelController controller) : base(controller)
        {
            _tf = controller.transform;
        }

        public override bool PreCondition(WorldVector state)
        {
            // var state = _controller.WorldState;
            
            var tree = state.CurGameObject;
            if (tree == null || !tree.CompareTag("Tree")) return false;
            
            _climbable ??= tree.GetComponent<TreeController>();
            
            return true;
        }

        public override bool PostCondition(WorldVector state)
        {
            // var state = _controller.WorldState;
            return state.CurGameObject != null &&
                   state.CurGameObject.CompareTag("Tree") &&
                   state.HState == WorldVector.HeightState.Floor &&
                   Mathf.Approximately(state.CurPosition.y, 0f);
        }
        
        public override WorldVector Simulate(WorldVector state)
        {
            var newState = state;
            newState.HState = WorldVector.HeightState.Floor;
            var target = state.CurPosition;
            target.y = 0f;
            newState.CurPosition = target;
            return newState;
        }

        public override bool Execute()
        {
            if (!_isExecuting)
            {
                _isExecuting = true;
                
                // Stop navMeshAgent from updating while agent is off of the navmesh
                if (_controller.agent.updatePosition) _controller.agent.updatePosition = false; //This should never run
                
                // Set Tree to occupied
                _climbable.IsOccupied = true;
                
                //Set target
                _target = _tf.position;
                _target.y = 0f;
            }
            
            var step = _controller.climbSpeed * Time.deltaTime;
            _tf.position = Vector3.MoveTowards(_tf.position, _target, step);

            if (!(Vector3.Distance(_tf.position, _target) <= Mathf.Epsilon)) return false;
            
            _controller.agent.updatePosition = true; //Put agent back on NavMesh
            return true;
        }
    }
}