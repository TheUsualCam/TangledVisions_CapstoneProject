using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class GameUIManager : MonoBehaviour
{
    
    public GameObject menu;
    public GameObject hud;
    public GameObject solvedText;
    public GameObject tapToContinueButton;
    public LevelDetails levelCompleteDetails;
    public GameStatsPanel gameStats;

    public float postCompletionWaitDuration;


    public GameObject previewCountdown;
    public TextMeshProUGUI previewCountdownText;
    public SliceTransition sliceTransition;

    private bool visionCompleted = false;
    private int totalMoves = 0;
    private int totalTimeInSeconds = 0;


    private void Update()
    {
        gameStats.UpdateStats();
    }
    
    private void OnEnable()
    {

        GameStateManager.OnGameOver += OnGameOver;
        GameStateManager.OnGameBootup += OnGameBootup;
    }
    
    private void OnDisable()
    {
        GameStateManager.OnGameOver -= OnGameOver;
        GameStateManager.OnGameBootup -= OnGameBootup;
    }


    void OnGameOver(bool complete, int moves, int timeSeconds)
    {
        gameStats.StopUpdating();
        visionCompleted = complete;
        solvedText.SetActive(true);
        
        StartCoroutine(DisplayCompletedVisionAfterDuration());
        
    }

    void OnGameBootup()
    {
        sliceTransition.OnVisionRevealed += StartComplete;
        ResetUIElements();
        StartCoroutine(VisionPreview());
    }

    IEnumerator VisionPreview()
    {
        Campaign campaign = LevelManager.Instance.campaign;
        float previewDuration = campaign.visions[campaign.currentVision].previewDuration;
        if (previewDuration > 0)
        {
            previewCountdown.SetActive(true);
            for (float i = previewDuration; i > 0; i--)
            {
                previewCountdownText.text = i.ToString("0");
                yield return new WaitForSeconds(1);
            }
            previewCountdown.SetActive(false);
            sliceTransition.Cover();
        }
        else
        {
            //No preview
            sliceTransition.RevealNoPreview();
        }
        
        yield return null;
    }

    void StartComplete()
    {
        gameStats.TogglePanel(true);
        sliceTransition.OnVisionRevealed -= StartComplete;
        GameStateManager.Instance.BootupComplete();
    }


    public void ResetUIElements()
    {
        tapToContinueButton.SetActive(false);
    }



    public void ShowScoreboard()
    {
        gameStats.TogglePanel(false);
        menu.SetActive(true);
        levelCompleteDetails.ShowLevelDetails(GameStateManager.Instance.levelManager.campaign, visionCompleted, totalMoves, totalTimeInSeconds);
        ResetUIElements();
    }

    IEnumerator DisplayCompletedVisionAfterDuration()
    {
        yield return new WaitForSeconds(postCompletionWaitDuration);
        tapToContinueButton.SetActive(true);
    }

    public bool IsUiActive()
    {
        return (menu.activeSelf);
    }

    
}
