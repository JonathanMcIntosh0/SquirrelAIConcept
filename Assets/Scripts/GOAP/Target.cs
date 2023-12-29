using UnityEngine;

namespace GOAP
{
    public enum TargetTypes
    {
        Nut,
        GarbageCan,
        Tree,
        // HomeTree,
        Location
    }
    public class Target
    {
        public readonly Vector2 Location;
        public readonly TargetTypes Type;
        public readonly GameObject Obj;
        public readonly bool IsInFOV;

        public Target(Vector2 location, TargetTypes type, GameObject obj, bool isInFOV)
        {
            Location = location;
            Type = type;
            Obj = obj;
            IsInFOV = isInFOV;
        }
    }
}