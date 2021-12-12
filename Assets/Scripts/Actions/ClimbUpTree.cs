using Squirrel;
using Tree;
using UnityEngine;
using UnityEngine.AI;

namespace Actions
{
    public class ClimbUpTree : SquirrelAction
    {
        // private SquirrelController _controller;
        private Vector3 _target;
        private Transform _tf;
        private bool _isExecuting;

        private IClimbable _climbable;
        
        public ClimbUpTree(SquirrelController controller) : base(controller)
        {
            _tf = controller.transform;
        }

        public override bool PreCondition(WorldVector state)
        {
            // var state = _controller.WorldState;
            if (!state.AtClimbableObject) return false; 
            //Note that if AtClimbableObject then CurGameObject != null
            _climbable ??= state.CurGameObject.GetComponent<IClimbable>();
            
            var tree = state.CurGameObject;
            if (tree == null || !tree.CompareTag("Tree")) return false;
            
            if (state.HState == WorldVector.HeightState.Floor && _climbable.IsOccupied) return false; //Tree already occupied
            
            return true;
        }

        public override bool PostCondition(WorldVector state)
        {
            // var state = _controller.WorldState;
            return state.CurGameObject != null &&
                   state.CurGameObject.CompareTag("Tree") &&
                   state.HState == WorldVector.HeightState.TreeTop &&
                   Mathf.Approximately(state.CurPosition.y, GameModel.SquirrelHomeHeight);
        }

        public override WorldVector Simulate(WorldVector state)
        {
            var newState = state;
            newState.HState = WorldVector.HeightState.TreeTop;
            var target = state.CurPosition;
            target.y = GameModel.SquirrelHomeHeight;
            newState.CurPosition = target;
            return newState;
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
                
                //Set target
                _target = _tf.position;
                _target.y = GameModel.SquirrelHomeHeight;
            }
            
            var step = _controller.climbSpeed * Time.deltaTime;
            _tf.position = Vector3.MoveTowards(_tf.position, _target, step);

            return Vector3.Distance(_tf.position, _target) <= Mathf.Epsilon;
        }
    }
}