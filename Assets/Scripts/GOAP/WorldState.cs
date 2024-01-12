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
        
        public int nutsCarried;
        public int garbageCarried;
        public float inventoryFill; // Value between 0 and 1 indicating how full is inventory

        public bool isPlayerNear;
        public bool isInventoryFull;
        
        public bool hasStored; // Did we recently store inventory in home tree? (Does not actually get set. Only used for planning)
        public float distanceTravelled; // Distance travelled for current plan

        public void SetLocation(Vector3 pos)
        {
            location = pos.GetHorizVector2();
            height = pos.y;
        }

        public void ResetForPlanning()
        {
            distanceTravelled = 0;
            hasStored = false;
        }
    }
}