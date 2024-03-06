using System.Collections;
using UnityEngine;

public class RenderTextureCreator : MonoBehaviour
{
    public Camera renderCam;

    public int textureWidth = 512;
    public int textureHeight = 512;

    private RenderTexture renderTexture;

    public string puzzleImageTag;

    public Camera CreateRenderTexture(GameObject newMask, int cameraCount, FragmentCreator.FragmentShapes fragmentShapes)
    {
        MeshRenderer meshRenderer;
        MeshFilter meshFilter;
        meshRenderer = newMask.transform.GetComponent<MeshRenderer>();
        meshFilter = newMask.transform.GetComponent<MeshFilter>();


        // Get the quad's bounds in world space
        Bounds quadBounds = meshRenderer.bounds;

        // Calculate the width and height of the quad in world space
        float quadWidth = quadBounds.size.x;
        float quadHeight = quadBounds.size.y;

        // Create the Render Texture with the calculated size
        renderTexture = new RenderTexture((int)(quadWidth * textureWidth), (int)(quadHeight * textureHeight), 24);
        //Debug.Log($"{(int)(quadWidth * textureWidth)}, {(int)(quadHeight * textureHeight)}");
        renderCam.orthographicSize = quadHeight / 2f;
        // Set the Render Texture to the Camera or Material as needed
        renderCam.targetTexture = renderTexture;
        meshRenderer.material.SetTexture("_MainTex", renderTexture);

        //Move the camera to the correct position
        renderCam.transform.parent = null;
        Vector2 puzzleImagePos = GameObject.FindGameObjectWithTag(puzzleImageTag).transform.localPosition;
        //For triangles, move the camera to the center of the Mesh
        if (fragmentShapes == FragmentCreator.FragmentShapes.Triangles || fragmentShapes == FragmentCreator.FragmentShapes.Quads)
        {
            renderCam.transform.position = quadBounds.center;
        }
        renderCam.transform.position += (Vector3)puzzleImagePos;

        //Rename the camera 
        renderCam.name = $"RenderCam{cameraCount}";

        //Disable at end of frame to ensure the render textures have updated.
        StartCoroutine(DisableCameraAtEndOfFrame());

        return renderCam;
    }

    private IEnumerator DisableCameraAtEndOfFrame()
    {
        yield return new WaitForEndOfFrame();
        renderCam.gameObject.SetActive(false);
        yield return null;
    }

    public void UpdateRenderTexture()
    {
        renderCam.gameObject.SetActive(true);

        StartCoroutine(DisableCameraAtEndOfFrame());
    }
}