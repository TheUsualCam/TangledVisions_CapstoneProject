using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class SelectedLevelCarrier : MonoBehaviour
{
    public Campaign selectedCampaign;

    void OnEnable()
    {
        SceneManager.sceneUnloaded += DestroyOnLeavingGame;
        LevelDetails.OnPlayLevel += UpdateCampaign;
    }

    void OnDisable()
    {
        SceneManager.sceneUnloaded -= DestroyOnLeavingGame;
        LevelDetails.OnPlayLevel -= UpdateCampaign;
    }

    public void DestroyOnLeavingGame(Scene scene)
    {
        if (scene.buildIndex == (int)Managers.SceneManager.Scenes.GameScene)
        {
            Destroy(gameObject);
        }
    }

    public void UpdateCampaign(MapLevel mapLevel)
    {
        selectedCampaign = mapLevel.campaign.campaign;
        selectedCampaign.currentVision = mapLevel.levelIndex;
    }

    public Campaign GetCampaign()
    {
        return selectedCampaign;
    }
}
