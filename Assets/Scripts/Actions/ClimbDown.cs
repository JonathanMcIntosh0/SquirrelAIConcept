using Squirrel;
using Tree;
using UnityEngine;
using UnityEngine.AI;

namespace Actions
{
    public class ClimbDown : SquirrelAction
    {
        // private SquirrelController _controller;
        private Vector3 _target;
        private Transform _tf;
        private bool _isExecuting;
        
        private GameObject _climbableGO;
        private IClimbable _climbable;
        
        public ClimbDown(SquirrelController controller) : base(controller)
        {
            _tf = controller.transform;
        }

        public override bool PreCondition(WorldVector state)
        {
            if (!state.AtClimbableObject) return false; 
            //Note that if AtClimbableObject then CurGameObject != null
            if (_climbableGO == null)
            {
                _climbableGO = state.CurGameObject;
                _climbable = _climbableGO.GetComponent<IClimbable>();
                _target = _tf.position;
                _target.y = GameModel.FloorHeight;
            }
            //Check if still at same object
            else if (!state.CurGameObject.Equals(_climbableGO)) return false;

            return true;
        }

        public override bool PostCondition(WorldVector state)
        {
            return state.AtClimbableObject &&
                   state.CurGameObject.Equals(_climbableGO) &&
                   Vector3.Distance(state.CurPosition, _target) <= Mathf.Epsilon;
        }
        
        public override WorldVector Simulate(WorldVector state)
        {
            state.CurPosition = _target;
            return state;
        }

        public override bool Execute()
        {
            if (!_isExecuting)
            {
                _isExecuting = true;
                
                // Stop navMeshAgent from updating while agent is off of the navmesh.
                // updatePosition should already be true if we are climbing down thus there is no need for this (safety measure)
                if (_controller.agent.updatePosition) _controller.agent.updatePosition = false; 
                
                // Set Tree to occupied (This should also already be true)
                _climbable.IsOccupied = true;
            }
            
            var step = _controller.climbSpeed * Time.deltaTime;
            var p = Vector3.MoveTowards(_tf.position, _target, step);
            _tf.position = p;

            if (!(Vector3.Distance(p, _target) <= Mathf.Epsilon)) return false; //Not at target yet
            
            _controller.agent.updatePosition = true; //Put agent back on NavMesh
            _climbable.IsOccupied = false;
            return true;
        }
    }
}