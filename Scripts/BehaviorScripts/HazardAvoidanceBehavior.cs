﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Flock/Behavior/HazardAvoidanceBehavior")]
public class HazardAvoidanceBehavior : FilteredFlockBehavior
{
    public override Vector3 CalculateMove(FlockAgent agent, List<Transform> context, Flock flock)
    {
        //if no neighbors, no adjustment
        if(context.Count == 0)
        {
            return Vector3.zero;
        }

        Vector3 avoidanceMove = Vector3.zero;
        int nAvoid = 0;

        List<Transform> filteredContext = (filter == null) ? context : filter.Filter(agent, context);
        
        foreach(Transform item in filteredContext)
        {
            nAvoid++;
            avoidanceMove += agent.transform.position - item.position;
        }

        if (nAvoid > 0)
        {
            avoidanceMove /= nAvoid;
        }

        return avoidanceMove;
    }
}

