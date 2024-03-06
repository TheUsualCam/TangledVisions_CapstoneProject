using System.Collections;
using System.Collections.Generic;
using Managers;
using UnityEngine;
using UnityEngine.Serialization;

public class LevelManager : Singleton<LevelManager>
{
    public FragmentCreator fragmentCreator;

    public Campaign campaign;

    public SelectedLevelCarrier carrier;

    Sprite currentSprite;
    public int currentLevel;
    int finalLevel;

    GameStateManager gameManager;

    protected override void Awake()
    {
        base.Awake();
        carrier = FindObjectOfType<SelectedLevelCarrier>();
        gameManager = FindObjectOfType<GameStateManager>();
    }

    void OnEnable()
    {
        LevelDetails.OnNextLevel += NextLevel;
        LevelDetails.OnReplayLevel += ReplayLevel;
    }

    void OnDisable()
    {
        LevelDetails.OnNextLevel -= NextLevel;
        LevelDetails.OnReplayLevel -= ReplayLevel;
    }

    private void Start()
    {
        StartFistLevel();
    }

    void StartFistLevel()
    {
        if(carrier)
            campaign = carrier.GetCampaign();
        currentLevel = campaign.currentVision;
        UpdateLevelImage(campaign.GetCurrentVision().image);
        finalLevel = campaign.visions.Length;
    }


    public void UpdateLevelImage(Sprite levelImage)
    {
        fragmentCreator.UpdateImage(levelImage, true);
    }

    public void ReplayLevel()
    {
        campaign.currentVision = currentLevel;
        UpdateLevelImage(campaign.GetCurrentVision().image);
        gameManager.StartGame();
    }

    public void NextLevel()
    {      
        if(currentLevel + 1 < finalLevel)
        {
            currentLevel++;
            campaign.currentVision = currentLevel;
            UpdateLevelImage(campaign.GetCurrentVision().image);
            gameManager.StartGame();
        }
        else
        {
            SceneManager.Instance.LoadScene(SceneManager.Scenes.LevelSelect);
        }

        
    }

    public Sprite GetCurrentSprite()
    {
        return currentSprite;
    }



}
