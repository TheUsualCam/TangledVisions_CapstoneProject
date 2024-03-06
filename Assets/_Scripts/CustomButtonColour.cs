using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CustomButtonColour : MonoBehaviour
{
    public Color releasedColour;
    public Sprite releasedSprite;
    public Color pressedColour;
    public Sprite pressedSprite;


    public Image[] objectsToColour;

    public void ToggleHighlight(bool toggle)
    {
        foreach (var obj in objectsToColour)
        {
            obj.sprite = toggle ? pressedSprite : releasedSprite;
            obj.color = toggle ? pressedColour : releasedColour;
        }
    }
}
