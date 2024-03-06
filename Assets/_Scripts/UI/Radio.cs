using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Radio : MonoBehaviour
{
    [SerializeField] private RectTransform trackControlPanel;
    [SerializeField] private TextMeshProUGUI trackText;
    [SerializeField] private TextMeshProUGUI playbackText;
    [SerializeField] private bool isPlaying;
    private AudioManager audioManager;

    [Header("Widget Animations")]
    public bool widgetShowing;

    public bool expandWidth;
    public float rectWidth;
    public bool expandHeight;
    public float rectHeight;
    public float animationTimer;
    public float animationDuration;
    public AnimationCurve animationCurve;
    public float widgetShowDuration;

    private void OnEnable()
    {
        AudioManager.OnTrackChanged += TrackChanged;
    }

    private void OnDisable()
    {
        AudioManager.OnTrackChanged -= TrackChanged;
    }

    private void Start()
    {
        audioManager = AudioManager.Instance;
        ToggleRadioVisibility();
    }

    void TrackChanged(string track)
    {
        SetTrackText(track);
        if(!widgetShowing) ToggleRadioVisibility();
    }

    public void SetTrackText(string trackName)
    {
        trackText.text = trackName;
    }

    public void PreviousTrack()
    {
        audioManager.PreviousTrack();
    }

    public void NextTrack()
    {
        audioManager.NextTrack();
    }

    public void ToggleRadioPlayback()
    {
        isPlaying = !isPlaying;
        
        if (isPlaying)
        {
            audioManager.ResumeMusic();
            playbackText.text = "| |";
        }
        else
        {
            audioManager.PauseMusic();
            playbackText.text = ">";
        }
    }

    public void ToggleRadioVisibility(){ToggleRadioVisibility(false);}

    public void ToggleRadioVisibility(bool manuallyOpened)
    {
        widgetShowing = !widgetShowing;
        if(manuallyOpened) CancelInvoke("ToggleRadioVisibility");
        StopAllCoroutines();
        //Show Widget
        if (widgetShowing)
        {
            //Close automatically if not opened by the player
            if(!manuallyOpened) Invoke("ToggleRadioVisibility", widgetShowDuration);

            
            StartCoroutine(ShowAnimation());
        }
        else //Hide Widget
        {
            if (gameObject.activeSelf)
            {
                StartCoroutine(HideAnimation());
            }
            
            
        }
    }

    public IEnumerator ShowAnimation()
    {
        
        while (animationTimer < animationDuration)
        {
            animationTimer += Time.deltaTime;
            float width = expandWidth ? rectWidth * animationCurve.Evaluate(animationTimer / animationDuration) : rectWidth;
            float height = expandHeight ? rectHeight * animationCurve.Evaluate(animationTimer / animationDuration) : rectHeight;
            trackControlPanel.sizeDelta = new Vector2(width, height);
            yield return null;

        }
        animationTimer = animationDuration;
        trackControlPanel.sizeDelta = new Vector2(rectWidth, rectHeight);

        yield return null;
    }

    public IEnumerator HideAnimation()
    {
        while (animationTimer > 0)
        {
            animationTimer -= Time.deltaTime;
            float width = expandWidth ? rectWidth * animationCurve.Evaluate(animationTimer / animationDuration) : rectWidth;
            float height = expandHeight ? rectHeight * animationCurve.Evaluate(animationTimer / animationDuration) : rectHeight;
            trackControlPanel.sizeDelta = new Vector2(width, height);
            yield return null;
        }
        animationTimer = 0;
        trackControlPanel.sizeDelta = new Vector2(expandWidth ? 0 : rectWidth, expandHeight ? 0 : rectHeight);
        yield return null;
    }
}
