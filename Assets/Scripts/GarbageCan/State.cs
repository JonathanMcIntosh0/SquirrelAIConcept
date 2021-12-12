using System;
using UnityEngine;

namespace GarbageCan
{
    public enum State
    {
        Empty, Full, Trap 
    }

    public static class StateExtension
    {
        private static readonly Color EmptyColor = Color.grey;
        private static readonly Color FullColor = Color.black;
        private static readonly Color TrapColor = Color.red;
        
        public static Color GetColor(this State s) => s switch
        {
            State.Empty => EmptyColor,
            State.Full => FullColor,
            State.Trap => TrapColor,
            _ => throw new ArgumentOutOfRangeException(nameof(s), s, null) //Should never run
        };
    }
}