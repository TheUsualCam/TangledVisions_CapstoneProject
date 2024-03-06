using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class StarCounter : MonoBehaviour
{
    private TextMeshProUGUI starText;
    private int currentStars;

    public float textScaleIncrease;
    public AnimationCurve textScaleCurve;
    public float textScaleDuration;
    public Color starAddedHighlightColour;
    private Vector3 startSize;
    private Color startColour;

    private void Awake()
    {
        starText = GetComponent<TextMeshProUGUI>();
        currentStars = PlayerPrefs.GetInt("TotalStars", 0) - PlayerPrefs.GetInt("StarsEarnedThisSession", 0);
        starText.text = $"{currentStars}";
        startSize = starText.transform.localScale;
        startColour = starText.color;
    }

    public void AddToStarText()
    {
        currentStars += 1;
        starText.text = $"{currentStars}";
        StartCoroutine(StarAddedAnimation());
    }

    IEnumerator StarAddedAnimation()
    {
        float t = 0;
        while (t < textScaleDuration)
        {
            float currentCurvePos = textScaleCurve.Evaluate(t / textScaleDuration);
            starText.transform.localScale =
                Vector3.Lerp(startSize, startSize + Vector3.one * textScaleIncrease, currentCurvePos);
            starText.color = Color.Lerp(startColour, starAddedHighlightColour, currentCurvePos);
            t += Time.deltaTime;
            yield return null;
        }

        starText.transform.localScale = startSize;
        starText.color = startColour;

       yield return null;

    }
}
