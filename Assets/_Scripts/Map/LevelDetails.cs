using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelDetails : MonoBehaviour
{
    public bool detailsShowing = false;
    public static event Action<MapLevel> OnPlayLevel;
    public static event Action OnNextLevel;
    public static event Action OnReplayLevel;

    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI playButtonText;
    [SerializeField] private Button playButton;
    [SerializeField] private GameObject playButtonLockText;
    [SerializeField] private GameObject replayButton;
    [SerializeField] private GameObject mapButton;
    [Header("Stars")]
    public float timeBetweenStars;
    [SerializeField] private Animator star1;
    [SerializeField] private Animator starMiddle;
    [SerializeField] private Animator star2;
    [Header("Stat Elements")]
    [Header("Previous")]
    [SerializeField] private TextMeshProUGUI previousMovesText;
    [SerializeField] private TextMeshProUGUI previousTimeText;
    [Header("Best")]
    [SerializeField] private TextMeshProUGUI bestMovesText;
    [SerializeField] private TextMeshProUGUI bestTimeText;
    [Header("Target")]
    [SerializeField] private TextMeshProUGUI targetTimeText;
    [SerializeField] private TextMeshProUGUI targetMovesText;



    [Header("Temporary")]
    public MapLevel currentLevelShowing;

    public Campaign currentCampaign;

    public void ShowLevelDetails()
    {
        playButton.onClick.RemoveAllListeners();
        // Activate level details panel 
        gameObject.SetActive(true);
        // Start coroutine to let panel be closed
        StartCoroutine(SetClosable());
    }

    // FOR MAP VIEW -- Show details relevant to selected level
    public void ShowLevelDetails(MapLevel levelToShow)
    {
        currentLevelShowing = levelToShow;

        Vision vision = currentLevelShowing.campaign.campaign.visions[currentLevelShowing.levelIndex];

        int prevTime = PlayerPrefs.GetInt($"{currentLevelShowing.campaign.campaignName}_{currentLevelShowing.levelIndex}_TimePrev", 0); 
        int prevMoves = PlayerPrefs.GetInt($"{currentLevelShowing.campaign.campaignName}_{currentLevelShowing.levelIndex}_MovesPrev", 0);
        int bestTime = PlayerPrefs.GetInt($"{currentLevelShowing.campaign.campaignName}_{currentLevelShowing.levelIndex}_Time", 0);
        int bestMoves = PlayerPrefs.GetInt($"{currentLevelShowing.campaign.campaignName}_{currentLevelShowing.levelIndex}_Moves", 0);

        SetStats(vision.name, prevTime, prevMoves, bestTime, bestMoves, vision.targetTimeInSeconds, vision.targetNumberOfMoves);



        ShowLevelDetails();

        // Set buttons based on if level is complete or not
        if (currentLevelShowing.levelState == MapLevel.levelStatus.Complete)
        {
            playButton.gameObject.SetActive(false);
            replayButton.SetActive(true);
            mapButton.SetActive(true);
        }
        else
        {
            playButton.gameObject.SetActive(true);
            playButtonText.text = "Play Level";
            replayButton.SetActive(false);
            mapButton.SetActive(false);
            playButton.onClick.AddListener(PlayLevel);
        }

        bool complete = currentLevelShowing.levelState == MapLevel.levelStatus.Complete;
        if (complete)
            StartCoroutine(ShowStars(timeBetweenStars, complete, vision.starsToEarn[1], vision.starsToEarn[2]));

    }

    //FOR GAME VIEW
    public void ShowLevelDetails(Campaign campaignToShow, bool complete, int moves, int timeSeconds)
    {
        currentCampaign = campaignToShow;
        Vision vision = campaignToShow.visions[campaignToShow.currentVision];

        // Set Stats
        int prevTime = PlayerPrefs.GetInt($"{campaignToShow.campaignName}_{campaignToShow.currentVision}_TimePrev", 0);
        int prevMoves = PlayerPrefs.GetInt($"{campaignToShow.campaignName}_{campaignToShow.currentVision}_MovesPrev", 0);
        int bestTime = PlayerPrefs.GetInt($"{campaignToShow.campaignName}_{campaignToShow.currentVision}_Time", 0);
        int bestMoves = PlayerPrefs.GetInt($"{campaignToShow.campaignName}_{campaignToShow.currentVision}_Moves", 0);

        SetStats(vision.name, prevTime, prevMoves, bestTime, bestMoves, vision.targetTimeInSeconds, vision.targetNumberOfMoves);


        bool moveStar = moves < campaignToShow.GetCurrentVision().targetNumberOfMoves;
        bool timeStar = timeSeconds < campaignToShow.GetCurrentVision().targetTimeInSeconds;


        ShowLevelDetails();
        // Set buttons based on if level is complete or not, and that its not the last level
        if (complete && currentCampaign.currentVision < currentCampaign.visions.Length)
        {
            playButtonText.text = "Next Level";
            //Is there a next level?
            if (currentCampaign.currentVision < currentCampaign.visions.Length - 1)
            {
                //If there is, is it locked?
                if (PlayerPrefs.GetInt("TotalStars") <
                    currentCampaign.visions[currentCampaign.currentVision + 1].requiredStars)
                {
                    //Disable play button and notify the player
                    playButton.interactable = false;
                    playButtonLockText.SetActive(true);
                }
                else
                {
                    playButton.interactable = true;
                    playButtonLockText.SetActive(false);
                }
            }
            
            replayButton.SetActive(true);
            mapButton.SetActive(true);
            playButton.onClick.AddListener(PlayNextLevel);
        }
        else
        {
            playButtonText.text = "Play Level";
            replayButton.SetActive(false);
            mapButton.SetActive(false);
            playButton.onClick.AddListener(PlayLevel);
        }


        StartCoroutine(ShowStars(timeBetweenStars, complete, moveStar || timeStar, moveStar && timeStar));


    }

    public void PlayLevel()
    {
        if (currentLevelShowing)
            OnPlayLevel?.Invoke(currentLevelShowing);
        else if (currentCampaign)
        {
            OnReplayLevel?.Invoke();
            CloseLevelDetails();
            transform.parent.gameObject.SetActive(false);
        }
    }

    public void PlayNextLevel()
    {
        Debug.Log("Play");
        //Play Level From Map
        if (currentLevelShowing)
        {
            if (currentLevelShowing.levelIndex + 1 < currentLevelShowing.campaign.campaignLevels.Count)
            {
                OnPlayLevel?.Invoke(currentLevelShowing.campaign.campaignLevels[currentLevelShowing.levelIndex + 1]);
            }
            else
            {
                OnPlayLevel?.Invoke(currentLevelShowing);
            }
        }
        //Play level from Game
        else if (currentCampaign)
        {
            CloseLevelDetails();
            OnNextLevel?.Invoke();
            transform.parent.gameObject.SetActive(false);
        }
    }
    

    private string TimeString(int time)
    {
        // Calculate time in minutes and seconds
        int minutes = Mathf.FloorToInt(time / 60);
        int seconds = time % 60;

        // Return time string
        return ($"{minutes:00}m {seconds:00}s");
    }


    private IEnumerator SetClosable()
    {
        // Wait for half a second
        yield return new WaitForSeconds(0.5f);
        // Set detailsShowing bool to true
        if (MapTutorialController.Instance)
        {
            if (!MapTutorialController.Instance.showTutorial)
                detailsShowing = true;
        }
        else detailsShowing = true;

    }

    public void CloseLevelDetails()
    {
        // If panel is open
        if (detailsShowing)
        {
            currentLevelShowing = null;
            // Close panel
            detailsShowing = false;
            gameObject.SetActive(false);
            starMiddle.SetTrigger("Reset");
            star1.SetTrigger("Reset");
            star2.SetTrigger("Reset");
        }
    }

    IEnumerator ShowStars(float durationBetweenStars, bool starMiddleActive, bool starLeft, bool starRight)
    {
        starMiddle.SetTrigger(starMiddleActive ? "Unlock" : "Locked");
        AudioManager.Instance.PlaySound(starMiddleActive ? "StarSuccess" : "StarFail");
        yield return new WaitForSeconds(durationBetweenStars);
        AudioManager.Instance.PlaySound(starLeft ? "StarSuccess" : "StarFail");
        star1.SetTrigger(starLeft ? "Unlock" : "Locked");
        yield return new WaitForSeconds(durationBetweenStars);
        AudioManager.Instance.PlaySound(starRight ? "StarSuccess" : "StarFail");
        star2.SetTrigger(starRight ? "Unlock" : "Locked");
        yield return null;
    }

    public void SetStats(string title, int prevTime, int prevMoves, int bestTime, int bestMoves, int targetTime, int targetMoves)
    {
        titleText.text = title;

        //Set Previous
        previousTimeText.text = prevTime > 0 ? TimeString(prevTime) : "---";
        previousMovesText.text = prevMoves > 0 ? prevMoves.ToString("00") : "---";

        //Set Best
        bestTimeText.text = bestTime > 0 ? TimeString(bestTime) : "---";
        bestMovesText.text = bestTime > 0 ? bestMoves.ToString("00") : "---";

        //Set Targets
        targetTimeText.text = targetTime > 0 ? TimeString(targetTime) : "---";
        targetMovesText.text = targetTime > 0 ? targetMoves.ToString("00") : "---";

    }
}
