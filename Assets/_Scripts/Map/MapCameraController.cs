using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using UnityEngine;

public class MapCameraController : MonoBehaviour
{
    private Camera mainCamera;
    private bool isMoving;
    private Vector2 moveDir;
    public float moveSpeed;
    public float zoomAmount;
    public LevelDetails levelDetails;
    [Header("Constraints")]
    public Vector2 fovRange;
    public Vector2 minFovPositionMin;
    public Vector2 minFovPositionMax;
    public Vector2 maxFovPositionMin;
    public Vector2 maxFovPositionMax;
    void OnEnable()
    {
        InputManager.OnPrimaryDownUpdated += StartMoveCamera;
        InputManager.OnPrimaryUpUpdated += EndMoveCamera;
        InputManager.OnLookUpdated += UpdateMoveDir;
    }

    void OnDisable()
    {
        InputManager.OnPrimaryDownUpdated -= StartMoveCamera;
        InputManager.OnPrimaryUpUpdated -= EndMoveCamera;
        InputManager.OnLookUpdated -= UpdateMoveDir;
    }

    void Awake()
    {
        mainCamera = GetComponent<Camera>();
        CameraZoom(0);
    }
    // Start is called before the first frame update
    void StartMoveCamera()
    {
        isMoving = true;
    }

    void EndMoveCamera()
    {
        isMoving = false;
    }

    void Update()
    {
        if (isMoving && !levelDetails.detailsShowing)
        {
            if (moveDir != Vector2.zero)
            {
                Vector3 moveTranslation = moveSpeed * Time.deltaTime * -moveDir;
                float posZ = transform.position.z;
                transform.Translate(moveTranslation);
                CheckCameraBounds(posZ);

            }
        }

        //Min FOV
        if (mainCamera.fieldOfView < fovRange.x)
        {
            mainCamera.fieldOfView = fovRange.x;
        }
        //Max FOV
        else if (mainCamera.fieldOfView > fovRange.y)
        {
            mainCamera.fieldOfView = fovRange.y;
        }
    }

    void UpdateMoveDir(Vector2 newMoveDir)
    {
        moveDir = newMoveDir;
    }

    void CheckCameraBounds(float posZ)
    {
        //Get the current FOV as 0-1
        float fovNormalized = Mathf.Clamp01((mainCamera.fieldOfView - fovRange.x) / (fovRange.y - fovRange.x));
        //Calculate the current FoV's Position Min/Max
        Vector2 currentFovPositionMin = Vector2.Lerp(minFovPositionMin, maxFovPositionMin, fovNormalized);
        Vector2 currentFovPositionMax = Vector2.Lerp(minFovPositionMax, maxFovPositionMax, fovNormalized);
        //Limit the position
        Vector3 position = transform.position;
        //Min
        if (position.x < currentFovPositionMin.x) position.x = currentFovPositionMin.x;
        if (position.y < currentFovPositionMin.y) position.y = currentFovPositionMin.y;
        //Max
        if (position.x > currentFovPositionMax.x) position.x = currentFovPositionMax.x;
        if (position.y > currentFovPositionMax.y) position.y = currentFovPositionMax.y;
        //Save the Z, this stops the camera also moving forward/backwards due to its tilt.
        position.z = posZ;
        //Update the position :D
        transform.position = position;
    }

    public void CameraZoom(float normalizedVal)
    {
        mainCamera.fieldOfView = Mathf.Lerp(fovRange.x, fovRange.y, normalizedVal);
        CheckCameraBounds(transform.position.z);
    }

}
