using UnityEngine;

public static class TransformExtension
{
    public static float GetAngleOfSight(this Transform tf, Vector3 p)
    {
        return Vector3.Angle(tf.forward, p - tf.position);
    }

    public static float GetHorizDistance(this Transform tf, Vector3 p)
    {
        var pos = tf.position;
        var a = new Vector2(pos.x, pos.z);
        var b = new Vector2(p.x, p.z);
        return Vector2.Distance(a, b);
    } 
}