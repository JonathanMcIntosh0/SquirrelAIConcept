using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace GOAP
{
    [Serializable]
    public struct WorldState
    {
        public Vector2 location; // Might change to Vector3
        public float height;

        public bool isPlayerNear;
        
        public int nutsCarried;
        public int garbageCarried;
        public bool isInventoryFull;

        public bool hasStored; // Did we recently store inventory in home tree? (Does not actually get set. Only used for planning)
        
        public bool hasReplanned;
        public float distanceTravelled; // Distance travelled for current plan
    }
}