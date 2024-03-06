using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

public class VisionSelectPopup : MonoBehaviour
{
    public static event Action OnVisionSelectClosed;
    public Transform visionCarousel;
    public Campaign campaignSelected;
    public void ShowPopup(Campaign campaign)
    {
        gameObject.SetActive(true);
        campaignSelected = campaign;
        //Update information
        //Get visions for selected campaign
        Vision[] visions = campaign.visions;
        if (visionCarousel == null)
        {
            visionCarousel = transform.Find("ButtonCarousel");
        }
        //Go through each button, and assign a vision.
        for (int visionIndex = 0; visionIndex < visionCarousel.childCount; visionIndex++)
        {
            Button currentButton = visionCarousel.GetChild(visionIndex).GetComponent<Button>();
            //If there are no more visions to display, then hide the button.
            if (visionIndex > visions.Length)
            {
                currentButton.gameObject.SetActive(false);
            }
            else //Update the button
            {
                //Set the sprite
                currentButton.GetComponent<Image>().sprite = visions[visionIndex].image;
                var index = visionIndex;
                //Load the vision when clicked
                currentButton.onClick.AddListener(delegate { VisionSelected(index);});
            }
            
        }
        
    }

    void VisionSelected(int visionIndex)
    {
        campaignSelected.currentVision = visionIndex;
    }

    public void HidePopup()
    {
        //clear buttons
        for(int i = 0; i < visionCarousel.childCount; i++)
        {
            visionCarousel.GetChild(i).gameObject.SetActive(true);
            visionCarousel.GetChild(i).GetComponent<Button>().onClick.RemoveAllListeners();
        }

        campaignSelected = null;
        gameObject.SetActive(false);
        OnVisionSelectClosed?.Invoke();
    }
}
