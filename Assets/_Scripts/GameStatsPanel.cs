using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameStatsPanel : MonoBehaviour
{
    [Header("Timer")]
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI timeTargetText;
    public bool timerActive;
    private int timerMins = 0;
    private float timerSecs = 0;
    private float playTime = 0;
    
    [Header("Moves")]
    public TextMeshProUGUI movesText;
    public TextMeshProUGUI movesTargetText;

    [Header("Misc")]
    public Animator animator;
    

    public void TogglePanel(bool toggle)
    {
        if (toggle) InitPanel();
        animator.SetTrigger(toggle ? "Show" : "Hide");
    }

    public void InitPanel()
    {
        Vision vision = LevelManager.Instance.campaign.GetCurrentVision();
        int targetMoves = vision.targetNumberOfMoves;
        movesTargetText.text = $"{targetMoves}";

        int time = vision.targetTimeInSeconds;
        int timeMin = time / 60;
        int timeSec = time % 60;
        timeTargetText.text = $"{timeMin.ToString("00")}:{timeSec.ToString("00")}";

        timerActive = true;
    }
    public void ResetPanel()
    {
        playTime = 0;
        timerMins = 0;
        timerSecs = 0;
        timeText.text = "00:00";
        timeTargetText.text = "00:00";
        movesText.text = "Moves: 0";
        movesTargetText.text = "0";
    }

    public void StopUpdating()
    {
        UpdateStats();
        timerActive = false;
    }

    public void UpdateStats()
    {
        if (!GameStateManager.Instance.isPuzzleActive || !timerActive) return;

        playTime += Time.deltaTime;

        if (timerSecs >= 60)
        {
            timerMins++;
            timerSecs -= 60;
        }

        timerSecs += Time.deltaTime;
        timeText.text = $"{timerMins.ToString("00")}:{timerSecs.ToString("00")}";
        movesText.text = $"Moves: {GameStateManager.Instance.numberOfMoves}";
    }

    public int GetPlayTime()
    {
        return (int)playTime;
    }
}
