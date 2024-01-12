using UnityEngine;

public interface IClimbable
{
    public bool IsOccupied { get; set; }
    public float MaxHeight { get; }
    public Transform transform { get; }
}