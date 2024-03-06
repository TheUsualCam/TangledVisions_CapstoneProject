using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Splines;
using static Cinemachine.DocumentationSortingAttribute;

public class MapLevel : MonoBehaviour
{
    [Header("Level Details")]
    public int levelIndex;
    public enum levelStatus { Hidden, Locked, Unlocked, Complete }
    public levelStatus levelState;

    [Header("Sprites")]
    [Tooltip("0 = Incomplete, 1 = Bronze, 2 = Silver, 3 = Gold.")]
    public Sprite[] buttonSprites;
    private SpriteRenderer buttonSpriteRenderer;
    public TextMeshPro requiredStarsText;
    public GameObject starsRequiredGO;
    public Animator vineAnimator;
    public GameObject levelHighlight;

    [Header("Campaign References")]
    public MapCampaign campaign;

    public static event Action<MapLevel> OnLevelSelected;


    [Header("Path Reveal")] 
    public SplineExtrude splinePath;
    public float pathShowDuration;

    void Awake()
    {
        buttonSpriteRenderer = GetComponent<SpriteRenderer>();
        MeshFilter filter = splinePath.GetComponent<MeshFilter>();
        filter.mesh = filter.mesh;
    }
    public void SelectLevel()
    {
        OnLevelSelected?.Invoke(this);

    }

    public void UpdateLevel(bool previousLevelComplete)
    {
        requiredStarsText.text = $"x{campaign.campaign.visions[levelIndex].requiredStars}";

        //If the previous level isn't complete, hide level.
        if (!previousLevelComplete)
        {
            levelState = levelStatus.Hidden;
        }
        //If not enough stars, then lock level.
        else if (PlayerPrefs.GetInt("TotalStars") < campaign.campaign.visions[levelIndex].requiredStars)
        {
            levelState = levelStatus.Locked;
        }
        else //Level is unlocked
        {
            levelState = levelStatus.Unlocked;
        }
        

        //Get level Goals.
        int targetMoves = campaign.campaign.visions[levelIndex].targetNumberOfMoves;
        int targetTimeInSeconds = campaign.campaign.visions[levelIndex].targetTimeInSeconds;
        bool moveStar = false;
        bool timeStar = false;

        //If player prefs for this level is > 0, then the level has been complete.
        if (PlayerPrefs.GetInt($"{campaign.campaignName}_{levelIndex}") > 0)
        {
            levelState = levelStatus.Complete;

            //If player prefs for this level's move counter is < target, then the level has the move star.
            int levelMoves = PlayerPrefs.GetInt($"{campaign.campaignName}_{levelIndex}_Moves");
            if (levelMoves > 0 && levelMoves < targetMoves)
            {
                moveStar = true;
            }
            //If player prefs for this level's time counter is < target, then the level has the time star.
            int levelTimeInSeconds = PlayerPrefs.GetInt($"{campaign.campaignName}_{levelIndex}_Time");
            if (levelTimeInSeconds > 0 && levelTimeInSeconds < targetTimeInSeconds)
            {
                timeStar = true;
            }
        }

        //State Animations
        switch (levelState)
        {
            case levelStatus.Hidden:
                gameObject.SetActive(false);
                break;
            case levelStatus.Locked:
                starsRequiredGO.SetActive(true);
                buttonSpriteRenderer.sprite = buttonSprites[0];
                break;
            case levelStatus.Unlocked:
                buttonSpriteRenderer.sprite = buttonSprites[0];
                if (splinePath)
                    StartCoroutine(RevealPath());
                vineAnimator.SetBool("Unlocked", true);
                break;
            case levelStatus.Complete:

                buttonSpriteRenderer.sprite = buttonSprites[1 + (timeStar ? 1 : 0) + (moveStar ? 1 : 0)];
                vineAnimator.gameObject.SetActive(false);
                if (splinePath)
                { 
                    ShowSpline();
                }
                break;
        }
        
        //Unlocked but not completed levels have the level highlight
        if (levelState == levelStatus.Unlocked)
        {
            levelHighlight.SetActive(true);
        }
        else levelHighlight.SetActive(false);
    }

    IEnumerator RevealPath()
    {
        float t = 0;
        bool hasShown = PlayerPrefs.GetInt($"{campaign.campaignName}_{levelIndex}_PathShown", 0) > 0;
        if(!hasShown)
        {
            PlayerPrefs.SetInt($"{campaign.campaignName}_{levelIndex}_PathShown", 1);
            while (t < pathShowDuration)
            {
                t += Time.deltaTime;

                splinePath.Range = new Vector2(0, t / pathShowDuration);
                splinePath.Rebuild();
                yield return null;
            }
        }
        
        ShowSpline();

        yield return null;
    }

    public void ShowSpline()
    {
        splinePath.Range = new Vector2(0, 1);
        splinePath.Rebuild();
    }

    public void ShowHighlight()
    {
        levelHighlight.SetActive(true);
    }
}
