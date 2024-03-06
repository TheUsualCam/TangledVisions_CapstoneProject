using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StarsGainedUI : MonoBehaviour
{
    public GameObject starAddedPrefab;
    public RectTransform target;
    public float initialTimeBetweenStars;
    public StarCounter starCounter;

    public void Start()
    {
        StartCoroutine(EStarsGained());
    }

    public IEnumerator EStarsGained()
    {
        //Get then clear stars earned.
        int starsEarned = PlayerPrefs.GetInt("StarsEarnedThisSession", 0);
        PlayerPrefs.SetInt("StarsEarnedThisSession", 0);
        float pitchUpPerStar = 1f / starsEarned;
        float pitch = 1f;

        //Generate that number of stars added ui elements
        for (int currentStar = 0; currentStar < starsEarned; currentStar++)
        {
            StarAddedAnimation star = Instantiate(starAddedPrefab, transform).GetComponent<StarAddedAnimation>();
            star.SetTarget(target, pitch);
            star.counter = starCounter;
            pitch += pitchUpPerStar;
            
            //1 - (1 / 3 * (2))
            yield return new WaitForSeconds(initialTimeBetweenStars - (initialTimeBetweenStars / starsEarned * (currentStar + 1)));
        }
        yield return null;
    }

}
