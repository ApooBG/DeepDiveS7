using UnityEngine;

/* 
    A class that's used to store properties. No gameObject attachment.
*/
public struct ProjectileProperties
{
    public Vector3 direction;
    public Vector3 initialPosition;
    public float initialSpeed;
    public float mass;
    public float drag;
}
