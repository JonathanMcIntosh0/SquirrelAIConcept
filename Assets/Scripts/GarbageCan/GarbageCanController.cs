using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace GarbageCan
{
    public class GarbageCanController : MonoBehaviour, IClimbable
    {
        //Climbable properties
        public bool IsOccupied
        {
            get => _hasSquirrel;
            set
            {
                //If squirrel leaving then reset _timeSinceLastChange
                if (!value && _hasSquirrel) _timeSinceLastChange = 0f; 
                _hasSquirrel = value;
            }
        }
        private bool _hasSquirrel;
        public float MaxHeight => GameModel.GarbageCanHeight;

        public State State
        {
            get => _state;
            set
            {
                _renderer.material.color = value.GetColor();
                _timeSinceLastChange = 0f;
                _state = value;
            }
        }
        private State _state; //Note: Should never directly set

        private Renderer _renderer;
        private float _timeSinceLastChange = 0f;

        // Start is called before the first frame update
        void Start()
        {
            _renderer = GetComponentInChildren<Renderer>();
            
            Random.InitState(DateTime.Now.Millisecond);

            //Choose if the can starts full or empty
            State = (Random.value > 0.5f) ? State.Empty : State.Full;
        }

        // Update is called once per frame
        void Update()
        {
            _timeSinceLastChange += Time.deltaTime;

            if (_timeSinceLastChange >= 2f && State == State.Trap)
                State = State.Empty;
            else if (_timeSinceLastChange >= 10f)
                FlipState();

            // If least 10 sec have passed and not hasSquirrel then changeState
            // Note that when a squirrel leaves we reset _timeSinceLastChange to 0
            
            // else if (_timeSinceLastChange >= 10f && !_hasSquirrel) 
            // {
            //     FlipState();
            // }

        }

        public bool TryGetGarbage()
        {
            if (State == State.Empty)
            {
                State = State.Trap;
                return false;
            }

            // Should always be in Full state here
            State = State.Empty;
            return true;
        }
        
        private void FlipState()
        {
            State = State switch
            {
                State.Empty => State.Full,
                State.Full => State.Empty,
                State.Trap => throw new ArgumentOutOfRangeException(nameof(State), State, 
                    "Error: Cannot flip state while squirrel is stuck in Garbage Can!"),
                _ => throw new ArgumentOutOfRangeException(nameof(State), State, null) //Should never run
            };
        }
    }
}
