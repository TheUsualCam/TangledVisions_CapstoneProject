using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;
using UnityEngine.Serialization;

public class AspectRatioCameraFitter : MonoBehaviour
{
    [FormerlySerializedAs("camera")] public Camera mainCamera;
    [Tooltip("The desired width the screen should be able to view.")]
    public float desiredUnitWidth;
    private void OnValidate()
    {
        mainCamera ??= Camera.main;
        UpdateCamera();
    }

    private void Start()
    {
        UpdateCamera();
    }

    void UpdateCamera()
    {
        float orthoSize = desiredUnitWidth * ((float)Screen.height / Screen.width) * 0.5f;
        mainCamera.orthographicSize = orthoSize;
    }
}
