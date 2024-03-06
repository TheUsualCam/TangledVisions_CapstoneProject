using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fragment : MonoBehaviour
{
    public static event Action OnFragmentSwapped; 
    public static event Action OnFragmentPickup; 
    [Header("Components")]
    public MeshRenderer meshRenderer;
    public MeshFilter filter;
    public PolygonCollider2D polygonCollider;
    public RenderTextureCreator renderTextureCreator;
    public Camera renderCamera;
    public ParticleSystem correctParticles;
    private PolygonDrawTool polygonDrawTool;

    [Header("Status")] 
    public FragmentCreator.FragmentShapes fragmentShape;
    public bool isLocked;
    public bool isCorrectTexture;
    public Texture correctTexture;
    public Texture currentTexture;
    Material currentMaterial;

    [Header("Colour Information")] 
    public bool isSelected;
    public bool isTargeted;
    public Color selectedHighlightColour;
    public Color targetedHighlightColour;
    public float desaturationAmount;
    public List<Vector3> defaultTriangleUVDirection = new List<Vector3>();

    [Header("Selection Animation")] 
    public Vector3 selectionAnimationOffset;
    public AnimationCurve selectAnimationCurve;
    public float selectAnimDuration;
    public Vector3 originLocalPosition;

    [Header("Swap Animation")] 
    public float swapAnimDuration;
    public AnimationCurve swapAnimScaleCurve;
    public ParticleSystem[] swapParticleSystems;

    [Header("Is Locked Animation")]
    public ParticleSystem lockOutlineParticleSystem;
    public ParticleSystem lockOnTapParticleSystem;
    public float lockShakeDuration;
    public AnimationCurve lockShakeCurveX;
    public AnimationCurve lockShakeCurveY;
    public float lockShakeIntensity;
    private bool playingLockShake;
    




    private void Awake()
    {
        if (!renderCamera)
            renderCamera = GetComponentInChildren<Camera>();
        
        if(!meshRenderer)
            meshRenderer = gameObject.GetComponent<MeshRenderer>();

        if(!filter)
            filter = gameObject.GetComponent<MeshFilter>();

        if(!polygonCollider)
            polygonCollider = gameObject.GetComponent<PolygonCollider2D>();

        if (!renderTextureCreator)
            renderTextureCreator = gameObject.GetComponent<RenderTextureCreator>();
        
        if (!polygonDrawTool)
            polygonDrawTool = gameObject.GetComponent<PolygonDrawTool>();
    }

    void Start()
    {
        polygonDrawTool.DrawPolygonCollider(polygonCollider);
    }

    public void ToggleOutline(bool toggle)
    {
        polygonDrawTool.ToggleOutline(toggle);
    }
    
    public void UpdateFragmentMaterial()
    {

        
        currentMaterial = meshRenderer.material;
        currentTexture = currentMaterial.GetTexture("_MainTex");

        if(isSelected)
            meshRenderer.material.SetColor("_HighlightColour", selectedHighlightColour);
        else if (isTargeted)
            meshRenderer.material.SetColor("_HighlightColour", targetedHighlightColour);
        else
            meshRenderer.material.SetColor("_HighlightColour", Color.white);

        meshRenderer.material.SetFloat("_DesaturationAmount", desaturationAmount);

        if (currentTexture == correctTexture && !isCorrectTexture)
        {
            isCorrectTexture = true;
            currentMaterial.SetInt("_IsCorrectPosition", 1);
            correctParticles.Play();
        }
        else if(currentTexture != correctTexture && isCorrectTexture)
        {
            isCorrectTexture = false;
            currentMaterial.SetInt("_IsCorrectPosition", 0);
            correctParticles.Stop();
        }
    }

    public IEnumerator FragmentSelectedAnim(bool isDropped = false)
    {
        if(!isDropped)
            OnFragmentPickup?.Invoke();
        Vector3 startPos = transform.localPosition;
        //Set the end Position to the offset, or if dropped, the origin of the fragment.
        Vector3 endPos = (isDropped ? originLocalPosition : transform.localPosition + selectionAnimationOffset);

        //Iteration over duration
        float t = 0;
        while (t < selectAnimDuration)
        {
            //Move position from start to end
            transform.localPosition = Vector3.Lerp(startPos, endPos, selectAnimationCurve.Evaluate(t/selectAnimDuration));
            //Increment time and loop
            t += Time.deltaTime;
            yield return null;
        }

        //Ensure the finish position is exactly the end position.
        transform.localPosition = endPos;
        yield return null;
    }

    public void PlaySwapFragmentAnimation()
    {
        OnFragmentSwapped?.Invoke();
        foreach (var particle in swapParticleSystems)
        {
            particle.Play();
        }
        StartCoroutine(ESwapFragmentAnimation());
    }

    public IEnumerator ESwapFragmentAnimation()
    {
        
        Vector3 startScale = transform.localScale;
        float t = 0;
        
        while (t < swapAnimDuration)
        {
            transform.localScale =
                Vector3.Lerp(Vector3.zero, startScale, swapAnimScaleCurve.Evaluate(t / swapAnimDuration));
            t += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        transform.localScale = startScale;
        yield return null;
    }

    public void LockFragment()
    {
        isLocked = true;
        lockOutlineParticleSystem.Play();
    }

    public void PlayIsLockedAnim()
    {
        
        if(!playingLockShake) lockOnTapParticleSystem.Play();
        StartCoroutine(EIsLockAnim());
    }

    public IEnumerator EIsLockAnim()
    {
        if (!playingLockShake)
        {
            playingLockShake = true;
            Vector3 origin = transform.localPosition;
            float t = 0;
            while (t < lockShakeDuration)
            {
                float xShake = lockShakeCurveX.Evaluate(t / lockShakeDuration) * lockShakeIntensity;
                float yShake = lockShakeCurveY.Evaluate(t / lockShakeDuration) * lockShakeIntensity;
                transform.localPosition = new Vector3(origin.x + xShake, origin.y + yShake, origin.z);
                t += Time.deltaTime;
                yield return null;
            }
            transform.localPosition = origin;
            playingLockShake = false;
        }
        yield return null;
    }

    public void CalculateAndReorderUvCoordinates()
    {
        if (fragmentShape == FragmentCreator.FragmentShapes.Rectangles) return;
        float highestDot = -1;
        int highestDotIndex = 0;
        foreach (var defaultUvDirection in defaultTriangleUVDirection)
        {
            for (int i = 0; i < filter.mesh.uv.Length; i++)
            {
                float dot = Vector2.Dot(defaultUvDirection.normalized,( filter.mesh.uv[i] - filter.mesh.uv[(i + 1) % filter.mesh.uv.Length]).normalized);
                
                if (dot > highestDot)
                {
                    highestDot = dot;
                    highestDotIndex = i;
                }
            }
        }
        Vector2[] reorderedUVs = filter.mesh.uv;
        //Start at highestDotIndex, and cycle through all vertices and reorder.
        for (int i = 0; i < filter.mesh.vertices.Length; i++)
        {
            reorderedUVs[i] = filter.mesh.uv[(highestDotIndex + i) % filter.mesh.uv.Length];
        }
        filter.mesh.uv = reorderedUVs;
        

    }

    public void SaveUvDefaultDirection()
    {
        switch (fragmentShape)
        {
            case FragmentCreator.FragmentShapes.Quads:
                Vector3 uv1 = filter.mesh.uv[filter.mesh.triangles[0]];
                Vector3 uv2 = filter.mesh.uv[filter.mesh.triangles[1]];

                Vector3 uv3 = filter.mesh.uv[filter.mesh.triangles[5]];
                Vector3 uv4 = filter.mesh.uv[filter.mesh.triangles[4]];

                defaultTriangleUVDirection.Add(uv1 - uv2);
                //defaultTriangleUVDirection.Add(uv3 - uv4);
                break;
            case FragmentCreator.FragmentShapes.Triangles:
                defaultTriangleUVDirection.Add(filter.mesh.uv[0] - filter.mesh.uv[1]);
                break;
        }
        
    }

    private int lastUvOffset = 0; 
    public  int uvOffset = 0;
    private Vector2[] origUvs;
    private bool hasInitUV = false;
    
    public void Update()
    {
            if (uvOffset != lastUvOffset)
            {
                if (!hasInitUV)
                {
                    origUvs = filter.mesh.uv;
                    hasInitUV = true;
                }

                Vector2[] newUVSet = new Vector2[origUvs.Length];
                for (int i = 0; i < origUvs.Length; i++)
                {
                    newUVSet[i] = origUvs[(i+uvOffset) %(origUvs.Length)];
                }
                filter.mesh.uv = newUVSet;
                lastUvOffset = uvOffset;
            }
    }



}
