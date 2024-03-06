using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MapUIManager : MonoBehaviour
{
    public LevelDetails levelDetails;
    public GameObject hud;
    public GameObject menu;
    

    void OnEnable()
    {
        // CampaignButton.OnCampaignSelected += ShowVisionSelectPopup;
        MapLevel.OnLevelSelected += ShowLevelDetailsPopup;
    }

    void OnDisable()
    {
        // CampaignButton.OnCampaignSelected += ShowVisionSelectPopup;
        MapLevel.OnLevelSelected -= ShowLevelDetailsPopup;
    }

    // Update is called once per frame
    void ShowLevelDetailsPopup(MapLevel levelToShow)
    {
        levelDetails.ShowLevelDetails(levelToShow);
    }

    public bool IsUiOpen()
    {
        return levelDetails.detailsShowing || menu.activeSelf;
    }
}
