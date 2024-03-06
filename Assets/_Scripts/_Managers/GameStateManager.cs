using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using SceneManager = Managers.SceneManager;

[Serializable]
public struct GameState
{
    public string name;
    public float score;
    public bool isVictorious;
    public float playTime;
}
public class GameStateManager : Singleton<GameStateManager>
{
    [field: SerializeField]
    public bool IsPaused { get; private set; }

    [SerializeField]private GameState gameState;
    
    public static event Action<bool, int, int> OnGameOver;
    public static event Action OnGameBootup;
    public static event Action OnGameStart;

    public int numberOfMoves = 0;

    public LevelManager levelManager;
    public AudioManager audioManager;

    [FormerlySerializedAs("uiManager")] public GameUIManager gameUIManager;

    //If the puzzle is currently being completed.
    public bool isPuzzleActive;

    // objects to run updates on 
    public PlayerController playerController;

    private int starsEarned;

    private FragmentCreator fragCreator;

    protected override void Awake()
    {
        base.Awake();
        playerController = FindObjectOfType<PlayerController>();
    }

    private void Start()
    {
        fragCreator = FindObjectOfType<FragmentCreator>();
        audioManager = AudioManager.Instance;
    }

    void OnEnable()
    {
        SceneManager.OnSceneLoaded += StartGame;
    }

    void OnDisable()
    {
        SceneManager.OnSceneLoaded -= StartGame;
    }

    public void StartGame()
    {

        for(int visionNum = 0; visionNum < levelManager.campaign.visions.Length; visionNum++)
        {
            for(int starNum = 0; starNum < levelManager.campaign.visions[visionNum].starsToEarn.Length; starNum++)
            {
                levelManager.campaign.visions[visionNum].starsToEarn[starNum] = false;
            }
        }

        if (!PlayerPrefs.HasKey("TotalStars")) PlayerPrefs.SetInt("TotalStars", 0);

        //Gets the current vision and, if it exists in PlayerPrefs, checks how many stars
        //the player has already earned from this vision to avoid giving out duplicate stars
        Vision currentVision = levelManager.campaign.GetCurrentVision();
        if(PlayerPrefs.HasKey($"{currentVision.seed}"))
        {
            for(int i = 0; i < currentVision.starsToEarn.Length; i++)
            {
                Vision loadedVision = JsonUtility.FromJson<Vision>(PlayerPrefs.GetString($"{currentVision.seed}"));
                if (loadedVision.starsToEarn == null) continue;
                currentVision.starsToEarn[i] = loadedVision.starsToEarn[i];
            }
        }

        starsEarned = 0;
        if(IsPaused || Time.timeScale == 0) ResumeGame();
        numberOfMoves = 0;

        StartCoroutine(GameBootup());
    }

    IEnumerator GameBootup()
    {
        yield return new WaitForEndOfFrame();
        OnGameBootup?.Invoke();
        yield return null;
    }

    public void BootupComplete()
    {
        isPuzzleActive = true;
        OnGameStart?.Invoke();
        StartCoroutine(GameTutorialController.Instance.ShowPanel(0));
    }

    // Update is called once per frame
    void Update()
    {
        if (IsPaused) return;

        playerController.PlayerControllerUpdate();
        

        if (Time.frameCount % 20 == 0)
            audioManager.AudioUpdate();


    }

    public void PauseGame()
    {
        Time.timeScale = 0;
        IsPaused = true;
    }

    public void ResumeGame()
    {
        Time.timeScale = 1;
        IsPaused = false;
    }

    public void GameOver(bool isVictory)
    {
        if (isVictory)
        {
            UpdateGameState(null, 0, 0, true); 
        }

        AudioManager.Instance.PlaySound("LevelComplete");
        fragCreator.fragments.Clear();
        isPuzzleActive = false;


        (bool, int, int) stats = CalculateStars();
        OnGameOver?.Invoke(stats.Item1, stats.Item2, stats.Item3);
        if (PlayerPrefs.HasKey("TotalStars"))
        {
            PlayerPrefs.SetInt("TotalStars", PlayerPrefs.GetInt("TotalStars") + starsEarned);
        }
        else
        {
            PlayerPrefs.SetInt("TotalStars", starsEarned);
        }
        //Add to stars gained this session
        PlayerPrefs.SetInt("StarsEarnedThisSession", PlayerPrefs.GetInt("StarsEarnedThisSession", 0) + starsEarned);
    }

    public void UpdateGameState(string name=null, float scoreToAdd=0, float playTimeToAdd=0, bool isVictorious=false)
    {
        if (name != null)
        {
            gameState.name = name;
        }

        if (scoreToAdd > 0)
        {
            gameState.score += scoreToAdd;
        }

        if (playTimeToAdd > 0)
        {
            gameState.playTime += playTimeToAdd;
        }

        if (isVictorious)
        {
            gameState.isVictorious = true;
        }
    }

    public GameState GetGameState()
    {
        return gameState;
    }

    //Returns stars as a Vector3. X = Complete, Y = Under Time, Z = Under Moves.
    public (bool, int, int) CalculateStars()
    {
        Campaign campaign = levelManager.campaign;
        Vision currentVision = campaign.GetCurrentVision();
        if(gameState.isVictorious)
        {

            if (!currentVision.starsToEarn[0])
            {
                currentVision.starsToEarn[0] = true;
                starsEarned++;
                //Save this level in playerPrefs as complete.
                PlayerPrefs.SetInt($"{campaign.campaignName}_{campaign.currentVision}", 1);
            }
        }

        int timeInSeconds = gameUIManager.gameStats.GetPlayTime();
        if (timeInSeconds < currentVision.targetTimeInSeconds)
        {

            if (!currentVision.starsToEarn[2])
            {
                currentVision.starsToEarn[2] = true;
                starsEarned++;
            }
        }
        //Save the time taken in playerprefs
        PlayerPrefs.SetInt($"{campaign.campaignName}_{campaign.currentVision}_TimePrev", timeInSeconds);
        //Save Best Time
        if(PlayerPrefs.GetInt($"{campaign.campaignName}_{campaign.currentVision}_Time", 99999) > timeInSeconds)
            PlayerPrefs.SetInt($"{campaign.campaignName}_{campaign.currentVision}_Time", timeInSeconds);

        if (numberOfMoves < currentVision.targetNumberOfMoves)
        {

            if (!currentVision.starsToEarn[1])
            {
                currentVision.starsToEarn[1] = true;
                starsEarned++;
            }
        }
        //Save the number of moves in playerprefs
        PlayerPrefs.SetInt($"{campaign.campaignName}_{campaign.currentVision}_MovesPrev", numberOfMoves);
        //Only save BEST moves
        if (PlayerPrefs.GetInt($"{campaign.campaignName}_{campaign.currentVision}_Moves", 99999) > numberOfMoves)
            PlayerPrefs.SetInt($"{campaign.campaignName}_{campaign.currentVision}_Moves", numberOfMoves);

        //Saves the current vision object as a json string in PlayerPrefs, for the purpose
        //of checking the stars earned from this vision when the player loads it next time
        PlayerPrefs.SetString(currentVision.seed.ToString(), JsonUtility.ToJson(currentVision));

        return (gameState.isVictorious, numberOfMoves, timeInSeconds);
    }

    public void ResetGameLifetimeProgress()
    {
        PlayerPrefs.DeleteAll();
        SceneManager.Instance.LoadScene(0);
    }


}
