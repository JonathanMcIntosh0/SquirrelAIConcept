using System;
using UnityEngine;

namespace GOAP
{
    public enum TargetType
    {
        Nut = 0,
        GarbageCan = 1,
        Tree = 2,
        Location,
        HomeTree
    }

    public enum TargetState
    {
        InFOV,
        InMemory,
        Forgotten // Not within targets in memory or FOV (Targeting system does not store nor update)
    }
    
    [Serializable]
    public class Target
    {
        public Vector2 location;
        public TargetType type;
        public GameObject obj;
        public TargetState state;
        
        public Target(Vector2 location, TargetType type, GameObject obj, TargetState state)
        {
            this.location = location;
            this.type = type;
            this.obj = obj;
            this.state = state;
        }
        
        public Target(Vector2 location)
        {
            this.location = location;
            type = TargetType.Location;
            obj = null;
            state = TargetState.Forgotten;
        }

        public Target(GameObject obj, TargetState state = TargetState.InFOV)
        {
            this.obj = obj;
            this.state = state;
            location = obj.transform.position.GetHorizVector2();
            type = obj.tag switch
            {
                "Nut" => TargetType.Nut,
                "Tree" => TargetType.Tree,
                "Garbage Can" => TargetType.GarbageCan,
                _ => throw new ArgumentOutOfRangeException()
            };
        }
        
        public Target(GameObject obj, TargetType type, TargetState state)
        {
            this.obj = obj;
            this.state = state;
            location = obj.transform.position.GetHorizVector2();
            this.type = type;
        }

        public static bool IsDetectable(TargetType type)
        {
            return type == TargetType.Nut
                   || type == TargetType.Tree
                   || type == TargetType.GarbageCan;
        }

        public override string ToString()
        {
            return $"(Type = {type}, Loc = {location}, Obj = {obj}, State = {state})";
        }
    }
}