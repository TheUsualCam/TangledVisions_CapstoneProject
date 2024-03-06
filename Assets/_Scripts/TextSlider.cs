using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextSlider : MonoBehaviour
{
    public RectTransform TrackNameObject;
    public RectTransform maskRect;

    public float scrollSpeed = 1f;

    // Update is called once per frame
    void Update()
    {
        Vector2 translation = scrollSpeed * Time.deltaTime * Vector2.left;
        TrackNameObject.anchoredPosition += translation;

        if (TrackNameObject.anchoredPosition.x < -TrackNameObject.rect.width)
            TrackNameObject.anchoredPosition = new Vector2(maskRect.rect.width, TrackNameObject.anchoredPosition.y);
    }
}
