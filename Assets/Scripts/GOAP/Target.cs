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

        /**
         * <summary>Creates a new target pointing to <paramref name="location"/>.</summary>
         * <param name="location">the horizontal position of created target.</param>
         * <remarks>Remaining fields are initialised as:
         * <code>
         * type = TargetType.Location;
         * obj = null;
         * state = TargetState.Forgotten;
         * </code>
         * </remarks>
         */
        public Target(Vector2 location) :
            this(location, TargetType.Location, null, TargetState.Forgotten)
        {
        }

        /**
         * <summary>
         * Creates a new target pointing to <paramref name="obj"/>
         * where <c>TargetType</c> is inferred using <paramref name="obj"/><c>.tag</c>.
         * </summary>
         * <param name="obj">the GameObject used to create target.</param>
         * <param name="state">the initial state of target.</param>
         * <remarks>Remaining fields are initialised as:
         * <code>
         * location = obj.transform.position.GetHorizVector2();
         * type = GetTargetType(obj.tag);
         * </code>
         * (<c>GetTargetType()</c> represents a switch statement.)
         * </remarks>
         */
        public Target(GameObject obj, TargetState state) :
            this(obj.transform.position.GetHorizVector2(),
                obj.tag switch
                {
                    "Nut" => TargetType.Nut,
                    "Tree" => TargetType.Tree,
                    "Garbage Can" => TargetType.GarbageCan,
                    _ => throw new ArgumentOutOfRangeException()
                }, obj, state)
        {
        }

        /**
         * <summary>
         * Creates a new target pointing to <paramref name="obj"/>.
         * </summary>
         * <param name="obj">the GameObject used to create target.</param>
         * <param name="type">the TargetType for new target.</param>
         * <param name="state">the initial state of target.</param>
         * <remarks>Remaining fields are initialised as:
         * <code>
         * location = obj.transform.position.GetHorizVector2();
         * </code>
         * </remarks>
         */
        public Target(GameObject obj, TargetType type, TargetState state) :
            this(obj.transform.position.GetHorizVector2(), type, obj, state)
        {
        }

        public static bool IsDetectable(TargetType type)
        {
            return type == TargetType.Nut
                   || type == TargetType.Tree
                   || type == TargetType.GarbageCan;
        }

        // TODO somehow make getting IsOccupied quicker to avoid repeated GetComponent
        // TODO maybe store MonoBehaviour instead of GameObject so direct access to controllers
        public bool IsUsable()
        {
            // if (state == TargetState.Forgotten) return false;
            // if (state == TargetState.InMemory) return true; // May not actually be usable but can't tell

            switch (type)
            {
                case TargetType.Nut:
                    return obj != null; // Check if destroyed
                case TargetType.GarbageCan:
                case TargetType.Tree:
                case TargetType.HomeTree:
                    return !obj.GetComponent<IClimbable>().IsOccupied; // EXPENSIVE
                case TargetType.Location:
                    return false;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public override string ToString()
        {
            return $"(Type = {type}, Loc = {location}, Obj = {obj}, State = {state})";
        }
    }
}