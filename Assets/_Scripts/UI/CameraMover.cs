using Cinemachine.Utility;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMover : MonoBehaviour
{
    public GameObject puzzleImage;
    Sprite puzzleSprite;

    public bool on = true;

    private void Awake()
    {
        puzzleSprite = puzzleImage.GetComponent<SpriteRenderer>().sprite;
    }

    public void UpdateCameraPosition()
    {
        if (!on) return;
        transform.position = puzzleImage.transform.position + puzzleSprite.bounds.center;
        transform.position = new Vector3(transform.position.x, transform.position.y, -1);
    }


}
