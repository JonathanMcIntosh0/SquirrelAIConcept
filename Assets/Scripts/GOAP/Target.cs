using System;
using GarbageCan;
using Nut;
using Tree;
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
        // Using, // Allows us to keep track of target we are currently using (e.g. tree we are in)
        InFOV,
        InMemory,
        Forgotten // Not within targets in memory or FOV (Targeting system does not store nor update)
    }

    [Serializable]
    public class Target
    {
        public Vector2 location; // TODO see if should add height
        public TargetType type; // TODO maybe remove this and use is operator (might need an isHomeTree and isLocation bool)
        public MonoBehaviour objController;
        public TargetState state;
        public float memoryTimer = 0f;

        public Target(Vector2 location, TargetType type, MonoBehaviour objController, TargetState state)
        {
            this.location = location;
            this.type = type;
            this.objController = objController;
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
            this(location, TargetType.Location, null, TargetState.InMemory)
        {
        }
        
        /// <summary>
        /// Creates a new target pointing to <paramref name="objController"/>
        /// where <c>TargetType</c> is inferred using <paramref name="objController"/><c>.tag</c>.
        /// </summary>
        /// <param name="objController">the controller for the GameObject used to create target.</param>
        /// <param name="state">the initial state of target.</param>
        /// <remarks>
        /// Remaining fields are initialised as:
        /// <code>
        /// location = objController.transform.position.GetHorizVector2();
        /// type = GetTargetType(objController.tag);
        /// </code>
        /// </remarks>
        public Target(MonoBehaviour objController, TargetState state) :
            this(objController.transform.position.GetHorizVector2(),
                objController switch
                {
                    NutController _ => TargetType.Nut,
                    TreeController _ => TargetType.Tree,
                    GarbageCanController _ => TargetType.GarbageCan,
                    _ => throw new ArgumentOutOfRangeException()
                }, objController, state)
        {
        }

        /**
         * <summary>
         * Creates a new target pointing to <paramref name="objController"/>.
         * </summary>
         * <param name="objController">the controller for the GameObject used to create target.</param>
         * <param name="type">the TargetType for new target.</param>
         * <param name="state">the initial state of target.</param>
         * <remarks>Remaining fields are initialised as:
         * <code>
         * location = objController.transform.position.GetHorizVector2();
         * </code>
         * </remarks>
         */
        public Target(MonoBehaviour objController, TargetType type, TargetState state) :
            this(objController.transform.position.GetHorizVector2(), type, objController, state)
        {
        }

        public static bool IsDetectable(TargetType type)
        {
            return type == TargetType.Nut
                   || type == TargetType.Tree
                   || type == TargetType.GarbageCan;
        }

        public bool IsUsable()
        {
            return objController switch
            {
                NutController _ => objController != null,
                GarbageCanController controller => !controller.IsOccupied,
                TreeController controller => !controller.IsOccupied,
                null => true, // Location type (always "usable")
                _ => throw new ArgumentOutOfRangeException()
            };
            
            // switch (type)
            // {
            //     case TargetType.Nut:
            //         return objController != null; // Check if destroyed
            //     case TargetType.GarbageCan:
            //         return !((GarbageCanController) objController).IsOccupied;
            //     case TargetType.Tree:
            //     case TargetType.HomeTree:
            //         return !((TreeController) objController).IsOccupied;
            //     case TargetType.Location:
            //         return false;
            //     default:
            //         throw new ArgumentOutOfRangeException();
            // }
        }

        public override string ToString()
        {
            return $"(Type = {type}, Loc = {location}, State = {state}, Usable = {IsUsable()})";
        }
    }
}