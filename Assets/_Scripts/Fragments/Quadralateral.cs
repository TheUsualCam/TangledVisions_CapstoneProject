using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Quadralateral
{

    public Vector3[] vertices { get; } = new Vector3[4];
    public Vector3 vertA { get { return vertices[0]; } }
    public Vector3 vertB { get { return vertices[1]; } }
    public Vector3 vertC { get { return vertices[2]; } }
    public Vector3 vertD { get { return vertices[3]; } }

    public void SetVertices(Vector3[] verts)
    {
        vertices[0] = verts[0];
        vertices[1] = verts[1];
        vertices[2] = verts[2];
        vertices[3] = verts[3];
    }
}
