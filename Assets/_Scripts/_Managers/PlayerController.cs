using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class PlayerController : MonoBehaviour
{
    #region Variables
    [Header("Components")]
    public Transform draggingPoint;
    public InputManager InputManager;
    private Camera mainCam;
    public MeshFilter dragFragment;
    [SerializeField] private FragmentSwapper fragmentSwapper;
    public GameUIManager gameUiManager;
    
    [Header("Fragments")]
    public Fragment fragment1;
    public Fragment fragment2;
    public Fragment fragmentTargeted;
    
    [Header("Controls")]
    public GameOptions.ControlTypes controlType;
    public float pickupDuration;
    public AnimationCurve pickupSpeedCurve;
    public Vector3 pickupSize;
    public AnimationCurve pickupSizeCurve;
    public Vector3 fragmentPickupOffset;

    [SerializeField]
    private bool lockWhenCorrect;

    private FragmentCreator fragCreator;
    #endregion

    //Update Function
    public void PlayerControllerUpdate()
    {
        if (gameUiManager.IsUiActive()) return;
        //Get mouse position, move marker to that position
        Vector3 pointerPosition = mainCam.ViewportToWorldPoint(InputManager.GetPointerPosition());
        pointerPosition.z = -0.5f;
        draggingPoint.position = pointerPosition;

        //If a fragment is selected, begin highlighting targeted fragments
        if (fragment1 != null)
        {
            if (Time.frameCount % 10 != 0) return;

            Collider2D[] hits = Physics2D.OverlapPointAll(pointerPosition);

            foreach (Collider2D hit in hits)
            {
                //If it hit something
                if (hit != null)
                {
                    if (hit.transform.CompareTag("Piece"))
                    {
                        //Reset any previously targeted fragments
                        if (fragmentTargeted)
                        {
                            //Is the pointer over a new fragment?
                            if (fragmentTargeted.gameObject != hit.gameObject)
                            {
                                fragmentTargeted.isTargeted = false;
                                fragmentTargeted.UpdateFragmentMaterial();
                            }
                        }

                        //Update the targeted fragment
                        fragmentTargeted = hit.GetComponent<Fragment>();
                        if (fragmentTargeted != fragment1)
                        {
                            fragmentTargeted.isTargeted = true;
                            fragmentTargeted.UpdateFragmentMaterial();
                        }
                    }
                }
            }
        }
        else if (fragmentTargeted != null)
        {
            fragmentTargeted.isTargeted = false;
            fragmentTargeted.UpdateFragmentMaterial();
            fragmentTargeted = null;
        }

    }

    #region initialization
    private void Start()
    {
        mainCam = Camera.main;
        InputManager = InputManager.Instance;
        ChangeControl(GameOptions.GetGameSettings());
        fragCreator = GameObject.Find("FragmentCreator").GetComponent<FragmentCreator>();
    }

    private void OnEnable()
    {
        InputManager.OnPrimaryUpUpdated += PointerUp;
        InputManager.OnPrimaryDownUpdated += PointerDown;
        GameOptions.OnGameSettingsUpdated += ChangeControl;
    }

    private void OnDisable()
    {
        InputManager.OnPrimaryUpUpdated -= PointerUp;
        InputManager.OnPrimaryDownUpdated -= PointerDown;
        GameOptions.OnGameSettingsUpdated -= ChangeControl;

    }
    #endregion


    #region Input
    void PointerUp()
    {      
        if (!GameStateManager.Instance.isPuzzleActive || gameUiManager.IsUiActive()) return;
        switch (controlType)
        {
            case GameOptions.ControlTypes.Drag:
                FragmentDropped();
                break;
            default:
                break;
        }
    }
    
    void PointerDown()
    {
        if (!GameStateManager.Instance.isPuzzleActive || gameUiManager.IsUiActive()) return;
        switch (controlType)
        {
            case GameOptions.ControlTypes.Drag:
                FragmentDragged();
                break;
            case GameOptions.ControlTypes.Click:
                FragmentSelectedByTap();
                break;
            default:
                break;
        }
    }
    #endregion

    #region Tapping
    void FragmentSelectedByTap()
    {
        Vector2 position = mainCam.ViewportToWorldPoint(InputManager.GetPointerPosition());

        RaycastHit2D hit = Physics2D.Raycast(position, Vector2.zero);

        //if a click hit an object.
        if (hit.collider != null)
        {
            //if a piece was hit.
            if(hit.transform.CompareTag("Piece"))
            {
                //Does it need to register mesh1?
                if (fragment1 == null)
                {
                    AudioManager.Instance.PlaySound("FragmentPickup");
                    //Collect fragment one
                    fragment1 = hit.collider.GetComponent<Fragment>();
                    //Set the colour.
                    fragment1.isSelected = true;
                    fragment1.UpdateFragmentMaterial();
                    StartCoroutine(fragment1.FragmentSelectedAnim());
                }
                //Its already registered fragment1, so register fragment2 and swap.
                else
                {
                    //Collect mesh2's meshRenderer
                    fragment2 = hit.collider.transform.GetComponent<Fragment>();

                    //Remove mesh1's highlight
                    fragment1.isSelected = false;
                    fragment2.isTargeted = true;

                    if (fragment1.meshRenderer.material.mainTexture != fragment2.meshRenderer.material.mainTexture)
                    {
                        GameStateManager.Instance.numberOfMoves++;
                        fragment1.PlaySwapFragmentAnimation();
                        fragment2.PlaySwapFragmentAnimation();
                        fragmentSwapper.ChangeMaterials(fragment1, fragment2);
                        AudioManager.Instance.PlaySound("FragmentSwap");
                    }

                    fragment2.isTargeted = false;
                    StartCoroutine(fragment1.FragmentSelectedAnim(true));
                    fragment1.UpdateFragmentMaterial();
                    fragment2.UpdateFragmentMaterial();

                    fragCreator.CheckFragments();

                    //Clear the pieces.
                    (fragment1, fragment2) = (null,null);
                }
            }
            
        }
    }
    #endregion

    #region DragAndDrop

    void FragmentDragged()
    {
        Vector2 position = mainCam.ViewportToWorldPoint(InputManager.GetPointerPosition());

        Collider2D hit = Physics2D.OverlapPoint(position);

        if (hit != null && IsFragmentSelectable(hit))
        {
            if (fragment1 == null)
            {
                AudioManager.Instance.PlaySound("FragmentPickup");
                fragment1 = hit.transform.GetComponent<Fragment>();
                //Show the fragment on the cursor
                //Copy frag1 Mesh
                dragFragment.mesh = fragment1.filter.mesh;
                //Copy Frag1 Material
                dragFragment.GetComponent<MeshRenderer>().material.CopyPropertiesFromMaterial(fragment1.meshRenderer.material);
                //Copy Frag1 scale
                dragFragment.transform.localScale = fragment1.transform.localScale / 2;
                dragFragment.transform.position = fragment1.transform.position;
                //Enable the fragment copy
                dragFragment.gameObject.SetActive(true);
                
                //Update frag1 with selected highlight
                fragment1.isSelected = true;
                fragment1.UpdateFragmentMaterial();
                
                //Animation===
                StartCoroutine(fragment1.FragmentSelectedAnim());
                StartCoroutine(MoveFragmentToPointerOverTime(fragment1));
            }         
        }
    }
    
    void FragmentDropped()
    {
        //Clear hovering objects.
        dragFragment.gameObject.SetActive(false);
        
        Vector2 position = Camera.main.ViewportToWorldPoint(InputManager.GetPointerPosition());
        Collider2D hit = Physics2D.OverlapPoint(position);

        
        if (hit != null && IsFragmentSelectable(hit))
        {
            if (hit.transform.CompareTag("Piece")){
                fragment2 = hit.transform.GetComponent<Fragment>();
                if (fragment1 != null && fragment2 != null)
                {
                    if (fragment1.meshRenderer.material.mainTexture != fragment2.meshRenderer.material.mainTexture)
                    {
                        GameStateManager.Instance.numberOfMoves++;
                        fragment1.PlaySwapFragmentAnimation();
                        fragment2.PlaySwapFragmentAnimation();
                        fragmentSwapper.ChangeMaterials(fragment1, fragment2);
                        AudioManager.Instance.PlaySound("FragmentSwap");
                    }
                    fragment2.isTargeted = false;

                    
                }
            }
        }

        if (fragment1)
        {
            StartCoroutine(fragment1.FragmentSelectedAnim(true));
            fragment1.isSelected = false;
            fragment1.UpdateFragmentMaterial();
        }

        fragCreator.CheckFragments();

        fragment1 = null;
        fragment2 = null; 

    }

    #endregion
    
    
    private IEnumerator MoveFragmentToPointerOverTime(Fragment fragment)
    {
        //Keep bounds to calculate centre
        Vector3 fragmentCentreOffset = dragFragment.mesh.bounds.center;// * transform.localScale.magnitude;
        Vector3 startPosWorld = fragment.transform.position;
        startPosWorld.z = draggingPoint.transform.position.z;
        //Set to Start Pos
        //startPosWorld -= fragmentCentreOffset;
        dragFragment.transform.position = startPosWorld;
        Vector3 startSize = dragFragment.transform.localScale;
        Vector3 endSize = dragFragment.transform.localScale;
        //Move from start to end over X duration
        float t = 0;
        while (t < pickupDuration)
        {
            //Get the next position from start to end at t
            //Calculate End point ever frame to account for movement
            Vector3 nextPos = Vector3.Lerp(startPosWorld, draggingPoint.transform.position - fragmentCentreOffset,
                pickupSpeedCurve.Evaluate(t / pickupDuration));
            //Set position of dragging fragment
            dragFragment.transform.position = nextPos;
        
            Vector3 nextSize = Vector3.Lerp(startSize, endSize, pickupSizeCurve.Evaluate(t/pickupDuration));
            dragFragment.transform.localScale = nextSize;

            t += Time.deltaTime;
            yield return null;
        }

        //End Pos finish animation
        dragFragment.transform.position = draggingPoint.transform.position - fragmentCentreOffset;
        dragFragment.transform.localScale = endSize;
        yield return null;
    }

    

    bool IsFragmentSelectable(Collider2D hit)
    {
        //check if a object can be clicked, use this to lock fragments when placed in the right place 
        Fragment frag = hit.transform.gameObject.GetComponent<Fragment>();
        if(!frag)
            return false;
        if (lockWhenCorrect)
        {
            if (frag.isCorrectTexture)
                return false;
        }

        if (frag.isLocked)
        {
            frag.PlayIsLockedAnim();
            return false;
        }
        
        return true;
    }

    #region "Options"
    
    public void ChangeControl(GameOptions.GameSettings settings)
    {
        controlType = settings.controlType;
    }
    #endregion

}
