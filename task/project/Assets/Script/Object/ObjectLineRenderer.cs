using System.Collections.Generic;
using UnityEngine;

public class ObjectLineRenderer : ObjectBase
{
    [SerializeField] LineRenderer lineRenderer = null;

    public void SetPositions(List<Vector2> positions)
    {
        lineRenderer.positionCount = positions.Count;
     
        for (int i = 0; i < positions.Count; ++i)
        {
            lineRenderer.SetPosition(i, positions[i]);
        }
    }
}
