using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapTutorialController : Singleton<MapTutorialController>
{
    public bool showTutorial;

    public GameObject tutorialUiHolder;

    public TutorialPanel[] tutorialPanels;

    public bool allowSelectingVisions;
    // Start is called before the first frame update
    protected override void Awake()
    {
        base.Awake();
        //If playerPref hasn't been found, then show the tutorial
        showTutorial = PlayerPrefs.GetInt("TutorialComplete", 0) < 1;
        if (showTutorial)
        {
            tutorialUiHolder.SetActive(true);
            allowSelectingVisions = false;
            //If the game tutorial has been complete, show panel 3, otherwise show panel 0.
            ShowPanel(PlayerPrefs.GetInt("GameTutorialComplete", 0) > 0 ? 3 : 0);
        }
        else allowSelectingVisions = true;
    }

    //Shows at the start of the game if the tutorial has not been seen.
    public void ToggleTutorial(bool toggle)
    {
        showTutorial = toggle;
        tutorialUiHolder.SetActive(showTutorial);
        allowSelectingVisions = !showTutorial;
    }

    public void ShowPanel(int panelToShow)
    {
        tutorialPanels[panelToShow].Activate();
    }


    public void ClosePanel(int panelNumber)
    {
        
        tutorialPanels[panelNumber].Deactivate();
    }

    public void PanelClosed(int panelNumber)
    {

        switch (panelNumber)
        {
            case 0:
                allowSelectingVisions = true;
                tutorialPanels[panelNumber + 1].Activate();
                break;
            case 1:
                allowSelectingVisions = false;
                tutorialPanels[panelNumber + 1].Activate();
                break;
            default:
                break;
        }
    }

    public void TutorialComplete()
    {
        PlayerPrefs.SetInt("TutorialComplete", 1);
        showTutorial = false;
        tutorialUiHolder.SetActive(false);
        allowSelectingVisions = true;

    }
}
