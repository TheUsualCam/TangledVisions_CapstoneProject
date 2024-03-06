using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PolygonDrawTool : MonoBehaviour
{
    public float lineWidth = 0.1f;
    public int cornerVerts;
    public Material lineMat;
    private LineRenderer lineRenderer;

    public void DrawPolygonCollider(PolygonCollider2D collider)
    {
        lineRenderer = collider.gameObject.AddComponent<LineRenderer>();
        lineRenderer.material = lineMat;
        lineRenderer.numCornerVertices = cornerVerts;
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        lineRenderer.loop = true;
        lineRenderer.useWorldSpace = false;
        lineRenderer.positionCount = collider.points.Length;
        for (int i = 0; i < collider.points.Length; i++)
        {
            lineRenderer.SetPosition(i, new Vector3(collider.points[i].x, collider.points[i].y));
        }

        lineRenderer.enabled = false;
    }

    public void ToggleOutline(bool toggle)
    {
        lineRenderer.enabled = toggle;
    }
}
