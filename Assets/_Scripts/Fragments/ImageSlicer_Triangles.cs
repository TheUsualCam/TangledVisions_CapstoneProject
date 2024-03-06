using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImageSlicer_Triangles : MonoBehaviour
{
    //Material that gets assigned to the new triangle slice objects
    [SerializeField] private Material newMaterial;

    //The list of triangle data that is obtained from the Triangulator script
    [SerializeField] private List<Triangle> triangles = new List<Triangle>();

    //Will store the newly created GameObjects after assigning triangle data
    [SerializeField] private List<GameObject> slices = new List<GameObject>();
    


    //Does all the calculations for Delaunay Triangulation
    private Triangulator triangulator;

    //Temporary storage for iteration over triangles
    private Vector3[] vertices;


    // Start is called before the first frame update
    void Start()
    {
        //Get the Triangulator and calculate then
        //store the triangles
        triangulator = GetComponent<Triangulator>();
        triangles = triangulator.Triangulate();

        //Iterate over every triangle, get their vertices,
        //create a new slice GameObject and assign the
        //triangle data to it's Mesh
        for(int i = 0; i < triangles.Count; i++)
        {
            Vector3[] vertices = new Vector3[3];
            vertices[0] = triangles[i].vertC.pos;
            vertices[1] = triangles[i].vertB.pos;
            vertices[2] = triangles[i].vertA.pos;

            int[] indices = new int[3];
            indices[0] = 0;
            indices[1] = 1;
            indices[2] = 2;

            Mesh mesh = new Mesh();
            mesh.vertices = vertices;
            mesh.triangles = indices;

            GameObject slice = new GameObject();
            slice.AddComponent<MeshFilter>();
            slice.AddComponent<MeshRenderer>();
            slice.GetComponent<MeshFilter>().mesh = mesh;
            slice.GetComponent<MeshRenderer>().material = newMaterial;

            slices.Add(slice);
        }
    }
}
