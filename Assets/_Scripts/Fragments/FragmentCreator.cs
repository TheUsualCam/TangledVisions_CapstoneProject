using Cinemachine.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEngine;

using Random = UnityEngine.Random;

public class FragmentCreator : MonoBehaviour
{
    [Serializable] public enum FragmentShapes{Rectangles, Triangles, Quads, Null}
    [Header("FragmentShapes")]
    [SerializeField] private FragmentShapes fragmentShapesToBuild;

    [Header("Components")]
    public Sprite visionImage;
    public SpriteRenderer sourceSpriteRenderer;
    public SpriteRenderer renderImageSpriteRenderer;
    private FragmentSwapper fragmentChanger;
    [SerializeField] private TMP_Dropdown sliceSelectionDropDown;

    [Header("Generated Objects")]
    public GameObject fragmentPrefab; // Prefab of the hexagonal sprite to be placed.
    public List<Fragment> initFragments = new List<Fragment>();
    public List<Fragment> fragments = new List<Fragment>();
    private List<Camera> renderCameras = new List<Camera>();
    private int cameraCount;

    [Header("Modifiers")]
    [Tooltip("Should the Vision automatically shuffle when created?")]
    public bool shuffleOnStart;
    [Range(0f, 1f)]
    [Tooltip("Set desaturation to 0 to ignore.")]
    public float desaturationAmount;
    public Color selectionHighlightColour;
    public Color targetHighlightColour;

    [Header("Rectangles")]
    private List<Vector2> maskSizesInPixels = new List<Vector2>();

    [Header("Triangles & Quads")]
    //The list of triangle data that is obtained from the Triangulator script
    [SerializeField] private List<Triangle> triangles = new List<Triangle>();
    //The list of Quad data that is obtained from the modified Triangulator script
    [SerializeField] private List<Quadralateral> quads = new List<Quadralateral>();

    [Header("Delaunay Triangulation")]
    //Does all the calculations for Delaunay Triangulation
    private Triangulator triangulator; 
    //The number of attempts the script will make to reshuffle vertices that are too close
    [SerializeField] private int numberOfVertexReshuffleAttempts = 5;
    //Temporary storage for iteration over triangles
    private List<Vector2> vertices = new List<Vector2>();

    //Events
    public event Action fragmentsCreated;

    private Vision currentBuildingVision;

    void Start()
    {
        fragmentChanger = GetComponent<FragmentSwapper>();
        triangulator = GetComponent<Triangulator>();
    }

    //Clears all Fragments, ready for the next Vision.
    public void ClearFragments()
    {
        for (int i = renderCameras.Count - 1; i >= 0; i--)
        {

            Destroy(renderCameras[i].gameObject);
            renderCameras.RemoveAt(i);
        }

        //Remove any existing planes
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Destroy(transform.GetChild(i).gameObject);
        }

        initFragments = new List<Fragment>();
        sourceSpriteRenderer.gameObject.SetActive(true);
    }

    public void CreateFragments()
    {
        Campaign campaign = LevelManager.Instance.campaign;
        CreateFragments(campaign.visions[campaign.currentVision]);
    }

    public void CreateFragments(Vision visionToCreate)
    {
        Random.InitState(SeedController.Instance.GetCurrentSeed());
        currentBuildingVision = visionToCreate;
        if (currentBuildingVision.fragmentShapes == FragmentShapes.Null)
            fragmentShapesToBuild = (FragmentShapes)sliceSelectionDropDown.value;
        else
        {
            fragmentShapesToBuild = currentBuildingVision.fragmentShapes;
        }

        //Remove any existing cameras/fragments.
        ClearFragments();

        //Disable the initial viewing image
        sourceSpriteRenderer.gameObject.SetActive(false);

        //Build Rectangles
        if (fragmentShapesToBuild == FragmentShapes.Rectangles)
        {
            //The number of cameras used to create Render Textures
            cameraCount = 0;
            StartCoroutine(GenerateRectangles());
        }
        //Build Triangles
        else if (fragmentShapesToBuild == FragmentShapes.Triangles)
        {
            StartCoroutine(GenerateTriangles());
        }
        //Build Quads
        else if (fragmentShapesToBuild == FragmentShapes.Quads)
        {
            StartCoroutine(GenerateQuads());
        }
        else
        {
            Debug.LogWarning($"Cannot Generate FragmentShapes.Null");
        }
        
    }

    private void OnFragmentsGenerated()
    {
        //Count the number of cameras used.
        //This is used for naming the Cameras.
        int count = 0;
        //Iterate over each mask, and add fragments.
        foreach (Fragment fragment in initFragments)
        {
            //Update the colour information
            fragment.selectedHighlightColour = selectionHighlightColour;
            fragment.targetedHighlightColour = targetHighlightColour;
            fragment.desaturationAmount = currentBuildingVision.useSaturation ? desaturationAmount : 0;
            fragment.UpdateFragmentMaterial();
            //Update the correct fragment
            fragment.correctTexture = renderCameras[count].targetTexture;

            AddFragment(fragment);
            count++;
        }

        triangulator.enabled = false;
        //Shuffle all fragments into random positions.
        if (shuffleOnStart)
            fragmentChanger.ShuffleAllMaterials();

        //Completed generation
        fragmentsCreated?.Invoke();
    }

    public void AddFragment(Fragment frag)
    {
        fragments.Add(frag);
    }

    private Fragment GenerateFragment(Vector3 meshCentrePosition, Vector3 fragmentScale, int fragmentNumber, Mesh meshGeometry, bool startLocked=false)
    {

        //Create new mask
        Transform fragmentTransform = Instantiate(fragmentPrefab, transform).transform;
        Fragment fragment = fragmentTransform.GetComponent<Fragment>();
        initFragments.Add(fragment);

        fragment.name = $"Fragment{fragmentNumber}";
        fragment.tag = "Piece";
        fragment.fragmentShape = fragmentShapesToBuild;
        
        fragmentTransform.localScale = fragmentScale;
        if(fragmentShapesToBuild == FragmentShapes.Rectangles)
            fragmentTransform.localPosition = meshCentrePosition;
        else
            fragmentTransform.position = meshCentrePosition;

        //Zero the local position Z.
        Vector3 localPos = fragmentTransform.localPosition;
        localPos.z = 0;
        fragmentTransform.localPosition = localPos;
        fragment.originLocalPosition = localPos;


        MeshFilter maskFilter = fragment.filter;
        if(meshGeometry != null)
            maskFilter.mesh = meshGeometry;
        

        //Generate the 2D Polygon Collider.
        PolygonCollider2D collider = fragment.polygonCollider;
        Vector3[] verts = maskFilter.mesh.vertices;
        //Convert to 2D points
        Vector2[] verts2D = new Vector2[verts.Length];
        if(fragmentShapesToBuild == FragmentShapes.Rectangles)
        {
            (verts[2], verts[3]) = (verts[3], verts[2]);
        }
        //Assign vertices to the Collider. Reordered so it creates a QUAD collider.
        for (int i = 0; i < verts.Length; i++)
        {
            verts2D[i] = verts[i];
        }

        //Update the collider's points
        collider.points = verts2D;

        Camera cam = fragment.GetComponent<RenderTextureCreator>()
            .CreateRenderTexture(fragment.gameObject, cameraCount++, fragmentShapesToBuild);

        //Save the Cameras reference
        renderCameras.Add(cam);
        if(fragmentShapesToBuild != FragmentShapes.Rectangles)
            UpdateUVs(maskFilter, cam);
        fragment.SaveUvDefaultDirection();
        if (startLocked) fragment.LockFragment();
        return fragment;
    }

    public void CheckFragments()
    {
        GameStateManager manager = GameStateManager.Instance;

        if (initFragments.Count > 0 && manager.isPuzzleActive)
        {

            foreach (Fragment frag in initFragments)
            {
                if (!frag.isCorrectTexture)
                {
                    return;
                }
            }
            manager.GameOver(true);
        }
    }

    private IEnumerator GenerateRectangles()
    {
        List<float> maskWidthPercents = RandomDivider.DivideIntoRandomPieces(currentBuildingVision.rectangleColumns, currentBuildingVision.minPercentSize, currentBuildingVision.maxPercentSize);
        List<float> maskHeightPercents = RandomDivider.DivideIntoRandomPieces(currentBuildingVision.rectangleRows, currentBuildingVision.minPercentSize, currentBuildingVision.maxPercentSize, 1);

        float totalWidth = 0;
        float currentWidth = 0;
        //Columns
        for (int x = 0; x < currentBuildingVision.rectangleColumns; x++)
        {
            float totalHeight = 0;
            //Rows
            for (int y = 0; y < currentBuildingVision.rectangleRows; y++)
            {
                Vector2 spriteSize = sourceSpriteRenderer.bounds.size * 2;
                Vector2 maskSize = new Vector2(maskWidthPercents[x] * spriteSize.x, maskHeightPercents[y] * spriteSize.y);
                currentWidth = maskSize.x;
                maskSizesInPixels.Add(maskSize);
                GenerateRectangle(totalWidth, totalHeight, maskSize, Vector2.zero);
                totalHeight += maskSize.y;
                yield return new WaitForEndOfFrame();
            }
            totalWidth += currentWidth;
        }
        OnFragmentsGenerated();
        yield return null;

    }

    //Creates a Fragment, and can recursively be split into smaller fragments.
    private Fragment GenerateRectangle(float totalX, float totalY, Vector2 maskSize, Vector2 offset, bool hasSplitHorizontal = false, bool hasSplitVertical = false)
    {

        bool splitHorizontal = Random.Range(0f, 1f) < currentBuildingVision.rectangleChanceToSplit;
        if (splitHorizontal && !hasSplitHorizontal)
        {

            if (maskSize.x / 2 > currentBuildingVision.minWorldSize)
            {
                maskSize.x /= 2;
                GenerateRectangle(totalX, totalY, maskSize, offset + new Vector2(maskSize.x, 0), true);
            }
        }
        bool splitVertical = Random.Range(0f, 1f) < currentBuildingVision.rectangleChanceToSplit;
        if (splitVertical && !hasSplitVertical)
        {
            if (maskSize.y / 2 > currentBuildingVision.minWorldSize)
            {
                maskSize.y /= 2;
                GenerateRectangle(totalX, totalY, maskSize, offset + new Vector2(0, maskSize.y), splitHorizontal, true);
            }


        }

        //Move it to the isCorrectTexture position on the board
        //Add half the sprite size
        Vector2 imagePivot = new Vector2(maskSize.x / 2, maskSize.y / 2);

        Vector2 pos = new Vector2(totalX + imagePivot.x,totalY + imagePivot.y) + offset;

        return GenerateFragment(pos, maskSize, 0, null);
    }


    /// <summary>
    /// Generates Quadrilateral Fragments from a modified Delaughny Triangulation
    /// </summary>
    private IEnumerator GenerateQuads()
    {
        if(!triangulator)
            triangulator = GetComponent<Triangulator>();

        SetTriangulationPoints();

        //Check if there are shared edges, and if so, combine them.
        List<Triangle> combinedTris = new List<Triangle>();
        List<Quadralateral> quads = new List<Quadralateral>();

        //Triple for loop nesting! :D I love this /s

        //Cycle all triangles to find possible quads
        foreach (Triangle tri in triangles)
        {
            //Only check Tris that haven't been combined already.
            if (combinedTris.Contains(tri)) continue;

            Edge[] edges = tri.GetEdges();

            //Check all edges of this tri
            for (int index = 0; index < edges.Length; index++)
            {
                //Check the currentTexture edge against all triangles
                foreach (Triangle comparisonTri in triangles)
                {
                    // Don't check against itself, or all edges will match!
                    if (comparisonTri == tri) continue;
                    //If the tri has already been combined, skip this iteration.
                    if (combinedTris.Contains(comparisonTri)) continue;

                    //Check if the comparison triangle contains any of the edges
                    if (comparisonTri.ContainsEdge(edges[index]))
                    {
                        //Mark tri as combined.
                        combinedTris.Add(comparisonTri);
                        combinedTris.Add(tri);

                        //Create a new quad!
                        quads.Add(CombineTrianglesToQuads(comparisonTri, tri, edges[index]));
                        //Break, as to not check for other edges.
                        break;
                    }
                }
                if (combinedTris.Contains(tri)) break; 
            }
        }

        //Generate remaining triangles
        int triNum = 0;
        foreach (Triangle tri in triangles)
        {
            //if the triangle hasn't been combined, then process it as a triangle
            if (!combinedTris.Contains(tri))
            {
                //because these are generated as Tris, lock them so they cannot be shuffled/moved
                triNum = GenerateTriangle(tri, triNum, true, true);
                yield return new WaitForEndOfFrame();
            }
        }

        //Iterate over every Quad, and create a Fragment
        for (int i = 0; i < quads.Count; i++)
        {
            // MESH GENERATION
            //Create Mesh
            Mesh mesh = new Mesh();

            //Create Vertices
            Vector3[] vertices = new Vector3[4];

            //Convert vertices to Local Coordinates
            var bounds = new Bounds(quads[i].vertA, Vector3.zero);
            bounds.Encapsulate(quads[i].vertB);
            bounds.Encapsulate(quads[i].vertC);
            bounds.Encapsulate(quads[i].vertD);
            Vector3 meshPos = bounds.min;
            Vector3 localVertA = (Vector3)quads[i].vertA - meshPos;
            Vector3 localVertB = (Vector3)quads[i].vertB - meshPos;
            Vector3 localVertC = (Vector3)quads[i].vertC - meshPos;
            Vector3 localVertD = (Vector3)quads[i].vertD - meshPos;

            // Find the bottom-left vertex
            Vector3 bottomLeft = localVertA;
            int bottomLeftIndex = 0;

            if (localVertB.y < bottomLeft.y || (localVertB.y == bottomLeft.y && localVertB.x < bottomLeft.x))
            {
                bottomLeft = localVertB;
                bottomLeftIndex = 1;
            }

            if (localVertC.y < bottomLeft.y || (localVertC.y == bottomLeft.y && localVertC.x < bottomLeft.x))
            {
                bottomLeft = localVertC;
                bottomLeftIndex = 2;
            }

            if (localVertD.y < bottomLeft.y || (localVertD.y == bottomLeft.y && localVertD.x < bottomLeft.x))
            {
                bottomLeft = localVertD;
                bottomLeftIndex = 3;
            }

            // Sort the vertices in clockwise order starting from the bottom-left
            vertices[0] = bottomLeft;
            vertices[1] = bottomLeftIndex == 0 ? localVertB : (bottomLeftIndex == 1 ? localVertC : (bottomLeftIndex == 2 ? localVertD : localVertA));
            vertices[2] = bottomLeftIndex == 0 ? localVertC : (bottomLeftIndex == 1 ? localVertD : (bottomLeftIndex == 2 ? localVertA : localVertB));
            vertices[3] = bottomLeftIndex == 0 ? localVertD : (bottomLeftIndex == 1 ? localVertA : (bottomLeftIndex == 2 ? localVertB : localVertC));
            mesh.vertices = vertices;

            //Create Quad Indices
            int[] indices = new int[6];
            indices[0] = 0;
            indices[1] = 1;
            indices[2] = 3;

            indices[3] = 2;
            indices[4] = 3;
            indices[5] = 1;
            mesh.triangles = indices;

            //GAME OBJECT GENERATION
            GenerateFragment(meshPos, Vector3.one * 2, i, mesh);
            yield return new WaitForEndOfFrame();
        }
        OnFragmentsGenerated();
        yield return null;
    }


    private int GenerateTriangle(Triangle tri, int triNum, bool addFourthVert=false, bool startLocked=false)
    {
        //Create Mesh
        Mesh mesh = new Mesh();

        //Create Vertices
        Vector3[] vertices = new Vector3[addFourthVert ? 4 : 3];

        //Convert vertices to Local Coordinates
        var bounds = new Bounds(tri.vertC.pos, Vector3.zero);
        bounds.Encapsulate(tri.vertB.pos);
        bounds.Encapsulate(tri.vertA.pos);
        Vector3 meshPos = bounds.min;
        Vector3 localVertA = (Vector3)tri.vertA.pos - meshPos;
        Vector3 localVertB = (Vector3)tri.vertB.pos - meshPos;
        Vector3 localVertC = (Vector3)tri.vertC.pos - meshPos;

        // Find the bottom-left vertex
        Vector2 bottomLeft = localVertA;
        int bottomLeftIndex = 0;

        if (localVertB.y < bottomLeft.y || (localVertB.y == bottomLeft.y && localVertB.x < bottomLeft.x))
        {
            bottomLeft = localVertB;
            bottomLeftIndex = 1;
        }

        if (localVertC.y < bottomLeft.y || (localVertC.y == bottomLeft.y && localVertC.x < bottomLeft.x))
        {
            bottomLeft = localVertC;
            bottomLeftIndex = 2;
        }

        // Sort the vertices in clockwise order starting from the bottom-left
        vertices[0] = bottomLeft;
        vertices[1] = bottomLeftIndex == 0 ? localVertB : (bottomLeftIndex == 1 ? localVertC : localVertA);
        vertices[2] = bottomLeftIndex == 0 ? localVertC : (bottomLeftIndex == 1 ? localVertA : localVertB);
        
        //Add an extra vertex for "fake" quads.
        if (addFourthVert) vertices[3] = Vector3.Lerp(vertices[2], vertices[0], .5f);

        mesh.vertices = vertices;
        //Create Triangle Indices
        int[] indices = new int[addFourthVert ? 6 : 3];
        indices[0] = 0;
        indices[1] = 1;
        indices[2] = 2;
        if (addFourthVert)
        {
            indices[3] = 2;
            indices[4] = 3;
            indices[5] = 0;
        }
        mesh.triangles = indices;

        //GAME OBJECT GENERATION
        GenerateFragment(meshPos, Vector3.one * 2, triNum, mesh, startLocked);
        return triNum;
    }

    private IEnumerator GenerateTriangles()
    {
        //Add code for generating triangles here.

        //Get the Triangulator and calculate then
        //store the triangles
        if(triangulator == null)
            triangulator = GetComponent<Triangulator>();

        SetTriangulationPoints();
        
        //Iterate over every triangle, get their vertices,
        //create a new slice GameObject and assign the
        //triangle data to it's Mesh
        for (int i = 0; i < triangles.Count; i++)
        {
            // MESH GENERATION, lock the first and last fragments
            GenerateTriangle(triangles[i], i);
            yield return new WaitForEndOfFrame();
        }
        OnFragmentsGenerated();
        yield return null;
    }

    //
    //HELPER FUNCTIONS
    //
    private void SetTriangulationPoints()
    {
        //Empty vertex buffer
        vertices.Clear();

        Bounds spriteBounds = sourceSpriteRenderer.sprite.bounds;
        Vector2 topLeft = new Vector2(spriteBounds.min.x, spriteBounds.max.y);
        Vector2 bottomRight = new Vector2(spriteBounds.max.x, spriteBounds.min.y);
        
        //GENERATE BORDER VERTICES
        //Top Right
        vertices.Add(sourceSpriteRenderer.transform.TransformPoint(spriteBounds.max));
        vertices.Add(sourceSpriteRenderer.transform.TransformPoint(Vector2.Lerp(topLeft, spriteBounds.max, .66f)));
        vertices.Add(sourceSpriteRenderer.transform.TransformPoint(Vector2.Lerp(topLeft, spriteBounds.max, .33f)));
        //Top Left
        vertices.Add(sourceSpriteRenderer.transform.TransformPoint(topLeft));
        vertices.Add(sourceSpriteRenderer.transform.TransformPoint(Vector2.Lerp(topLeft, spriteBounds.min, .75f)));
        //vertices.Add(sourceSpriteRenderer.transform.TransformPoint(Vector2.Lerp(topLeft, spriteBounds.min, .5f)));
        vertices.Add(sourceSpriteRenderer.transform.TransformPoint(Vector2.Lerp(topLeft, spriteBounds.min, .25f)));
        //Bottom Left
        vertices.Add(sourceSpriteRenderer.transform.TransformPoint(spriteBounds.min));
        vertices.Add(sourceSpriteRenderer.transform.TransformPoint(Vector2.Lerp(spriteBounds.min, bottomRight, .33f)));
        vertices.Add(sourceSpriteRenderer.transform.TransformPoint(Vector2.Lerp(spriteBounds.min, bottomRight, .66f)));
        //Bottom Right
        vertices.Add(sourceSpriteRenderer.transform.TransformPoint(bottomRight));
        vertices.Add(sourceSpriteRenderer.transform.TransformPoint(Vector2.Lerp(bottomRight, spriteBounds.max, .25f)));
        //vertices.Add(sourceSpriteRenderer.transform.TransformPoint(Vector2.Lerp(bottomRight, spriteBounds.max, .5f)));
        vertices.Add(sourceSpriteRenderer.transform.TransformPoint(Vector2.Lerp(bottomRight, spriteBounds.max, .75f)));

        Campaign campaign = LevelManager.Instance.campaign;
        //Randomize Delaunay Triangulation Points
        if (campaign.visions[campaign.currentVision].randomizePoints)
        {
            //Add extra points at random positions, accounting for border padding
            for (int numVertices = 0; numVertices < campaign.visions[campaign.currentVision].vertices.Count; numVertices++)
            {
                //Create the new vertex
                Vector2 newVert = new Vector2(0, 0);

                //Keeps tracks of the amount of times a vertex position needs to be shuffled
                int numReshuffleAttempts = 0;

                //Complete the first shuffle
                do
                {
                    newVert.Set(Random.Range(spriteBounds.min.x + currentBuildingVision.minVertexDistanceFromBounds,
                                    spriteBounds.max.x - currentBuildingVision.minVertexDistanceFromBounds),
                                Random.Range(spriteBounds.min.y + currentBuildingVision.minVertexDistanceFromBounds,
                                    spriteBounds.max.y - currentBuildingVision.minVertexDistanceFromBounds));
                    if (GetDistanceToClosestPoint(newVert) > currentBuildingVision.vertexRadius)
                    {
                        vertices.Add(newVert);
                        campaign.visions[campaign.currentVision].vertices[numVertices] = newVert;
                        break;
                    }

                    numReshuffleAttempts++;
                }
                //Reshuffle until either the vertex is a good distance from the others,
                //or reshuffles have exceeded the specified number of attempts
                while (numReshuffleAttempts < numberOfVertexReshuffleAttempts);
            }
        }
        else
        {
            for(int i = 0; i < campaign.visions[campaign.currentVision].vertices.Count; i++)
            {
                vertices.Add(campaign.visions[campaign.currentVision].vertices[i]);
            }
        }

        //Convert vertices to Point objects for triangulation
        List<Point> points = new List<Point>();

        for (int numPoints = 0; numPoints < vertices.Count; numPoints++)
        {
            Point point = new Point(vertices[numPoints].x, vertices[numPoints].y);
            points.Add(point);
        }

        //Set the points in the Triangulator and generate the triangle data
        triangulator.SetPoints(points);

        //else use the existing triangulation points set in editor.
        triangles = triangulator.Triangulate();
    }

    //Iterates over the list of vertices and returns the distance to
    //the vertex that is closest to the input vertex
    private float GetDistanceToClosestPoint(Vector2 currentVertex)
    {
        float distanceToClosestPoint = 0f;

        if(vertices.Count > 0)
        {
            distanceToClosestPoint = Vector2.Distance(currentVertex, vertices[0]);
        }

        for(int i = 0; i < vertices.Count; i++)
        {
            float distance = Vector2.Distance(currentVertex, vertices[i]);
            if (distance < distanceToClosestPoint)
            {
                distanceToClosestPoint = distance;
            }
        }

        return distanceToClosestPoint;

    }

    private Quadralateral CombineTrianglesToQuads(Triangle tri1, Triangle tri2, Edge sharedEdge)
    {
        Vector3[] verts = new Vector3[4];

        // vert0 is shared edge A
        verts[0] = sharedEdge.vertexA.pos;
        // vert1 is the third vertice of tri1 (Check against sharedEdge A/B, it's the one that doesn't match these)
        verts[1] = GetThirdVertice(sharedEdge, tri1);
        // vert2 is shared edge B
        verts[2] = sharedEdge.vertexB.pos;
        // vert3 is the third vertice of tri2 (Check against sharedEdge A/B, it's the one that doesn't match these)
        verts[3] = GetThirdVertice(sharedEdge, tri2);

        // Ensure the vertices are in clockwise order
        if (IsClockwise(verts))
        {
            // If the vertices are not in clockwise order, swap two vertices to reverse the order.
            Swap(ref verts[1], ref verts[3]);
        }

        // Replace "new" with instantiation.
        Quadralateral newQuad = new Quadralateral();
        newQuad.SetVertices(verts);

        return newQuad;
    }

    // Helper function to check if vertices are in clockwise order
    private bool IsClockwise(Vector3[] vertices)
    {
        float sum = 0f;
        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3 current = vertices[i];
            Vector3 next = vertices[(i + 1) % vertices.Length];
            sum += (next.x - current.x) * (next.y + current.y);
        }
        return sum < 0f;
    }

    // Helper function to swap two vertices
    private void Swap(ref Vector3 a, ref Vector3 b)
    {
        (a, b) = (b, a);
    }

    private Vector3 GetThirdVertice(Edge sharedEdge, Triangle tri)
    {
        Vector3 thirdVertice = new Vector3();

        //Go through each vertice
        foreach (Point vertice in tri.vertices)
        {
            //If the vertice doesn't match Edge Point A and B, then it must be the third vertice.
            if (sharedEdge.vertexA.pos != vertice.pos && sharedEdge.vertexB.pos != vertice.pos)
            {
                thirdVertice = vertice.pos;
                break;
            }
        }

        return thirdVertice;
    }

public void UpdateUVs(MeshFilter meshFilter, Camera cam)
{
    Vector3 camTopRight = cam.ViewportToWorldPoint(new Vector3(1, 1, 0));
    camTopRight -= renderImageSpriteRenderer.transform.localPosition; // Account for the offset of the cameras
    camTopRight -= meshFilter.transform.position;

    //Create UV array of length of vertices
    Vector2[] uvs = new Vector2[meshFilter.mesh.vertices.Length];

    // Initialize UVs
    for (int index = 0; index < uvs.Length; index++)
    {
        // Generate UV (0.0-1.0)
        uvs[index] = new Vector2(meshFilter.mesh.vertices[index].x / camTopRight.x,
            meshFilter.mesh.vertices[index].y / camTopRight.y);
    }
    meshFilter.mesh.uv = uvs;
}

    public void UpdateImage(Sprite newImage, bool updateVision = false)
    {
        //ClearFragments();
        if (updateVision)
        {
            visionImage = newImage;
        }
        sourceSpriteRenderer.sprite = visionImage;
        renderImageSpriteRenderer.sprite = visionImage;
    }

}