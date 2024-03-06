using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawVertices : MonoBehaviour
{
    [SerializeField] private Campaign campaign;
    [SerializeField] private float vertexRadius = 1f;

#if UNITY_EDITOR

    private void OnDrawGizmos()
    {
        if(campaign)
        {
            for (int visionNum = 0; visionNum < campaign.visions.Length; visionNum++)
            {
                if (campaign.visions[visionNum].debugVertices)
                {
                    Gizmos.color = Color.black;

                    for (int vertexNum = 0; vertexNum < campaign.visions[visionNum].vertices.Count; vertexNum++)
                    {
                        Gizmos.DrawSphere(campaign.visions[visionNum].vertices[vertexNum], vertexRadius);
                    }
                }
            }
        }
    }
#endif
}
