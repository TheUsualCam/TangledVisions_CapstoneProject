using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TutorialPanel : MonoBehaviour
{
    MapTutorialController mapTutorialController;
    GameTutorialController gameTutorialController;
    public int tutorialIndex;
    public GameObject[] objectsToActivate;
    public UnityEvent eventsOnActivation;
    private Animator animator;

    void Awake()
    {
        animator = GetComponent<Animator>();
        mapTutorialController = FindAnyObjectByType<MapTutorialController>();
        gameTutorialController = FindAnyObjectByType<GameTutorialController>();
    }
    public void Activate()
    {
        if (gameObject.activeSelf) return;
        foreach (var obj in objectsToActivate)
        {
            obj.SetActive(true);
        }
        eventsOnActivation?.Invoke();
        gameObject.SetActive(true);
    }

    public void Deactivate(bool skipNotify = false)
    {
        if (!gameObject.activeSelf) return;
        foreach (var obj in objectsToActivate)
        {
            obj.SetActive(false);
        }
        gameObject.SetActive(false);
        if (skipNotify) return;

        if (mapTutorialController != null)
            mapTutorialController.PanelClosed(tutorialIndex);
        else if (gameTutorialController != null)
            gameTutorialController.PanelClosed(tutorialIndex);
        
        
    }
}
