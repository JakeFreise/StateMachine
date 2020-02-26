using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent((typeof(Collider)))]
public abstract class FlockBehavior : ScriptableObject
{
    public abstract Vector3 CalculateMove(FlockAgent agent, List<Transform> context, Flock flock);
    
}
