using Squirrel;
using Tree;
using UnityEngine;
using UnityEngine.AI;

namespace Actions
{
    public class ClimbUp : SquirrelAction
    {
        // private SquirrelController _controller;
        private Vector3 _target;
        private Transform _tf;
        private bool _isExecuting;

        private GameObject _climbableGO;
        private IClimbable _climbable;
        
        public ClimbUp(SquirrelController controller) : base(controller)
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
                _target.y = _climbable.MaxHeight;
            }
            //Check if still at same object
            else if (!_climbableGO.Equals(state.CurGameObject)) return false;
            
            // Check if object is occupied by a squirrel (could be this squirrel)
            // If it is occupied then check if we are on tree
            return !_climbable.IsOccupied || !state.AtHeight(GameModel.FloorHeight);
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
                // Stop navMeshAgent from updating while agent is off of the navmesh
                if (_controller.agent.updatePosition) _controller.agent.updatePosition = false;
                
                // Set Tree to occupied
                _climbable.IsOccupied = true;
            }
            
            var step = _controller.climbSpeed * Time.deltaTime;
            var p = Vector3.MoveTowards(_tf.position, _target, step);
            _tf.position = p;

            return Vector3.Distance(p, _target) <= Mathf.Epsilon;
        }
    }
}