using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameTutorialController : Singleton<GameTutorialController>
{
    public bool showTutorial;
    public GameObject tutorialUiHolder;

    public TutorialPanel[] tutorialPanels;

    private bool undoDone;
    public Image checkboxUndo;
    private bool highlightDone;
    public Image checkboxHighlight;
    public Sprite checkboxTrueSprite;

    protected override void Awake()
    {
        base.Awake();
        //If playerPref hasn't been found, then show the tutorial
        showTutorial = PlayerPrefs.GetInt("TutorialComplete", 0) < 1;
        tutorialUiHolder.SetActive(showTutorial);
    }

    public IEnumerator ShowPanel(int panelToShow)
    {
        yield return new WaitForEndOfFrame();
        if (showTutorial)
        {
            tutorialPanels[panelToShow].Activate();
            switch (panelToShow)
            {
                case 1:
                    Fragment.OnFragmentSwapped += FragmentSwapped;
                    break;
                case 2:
                    Fragment.OnFragmentSwapped += FragmentPickup;
                    break;
                case 4:
                    FragmentHighlighter.OnHighlightActivate += FragmentHighlight;
                    MoveUndo.OnMoveUndo += FragmentUndo;
                    break;
                case 5:
                    PlayerPrefs.SetInt("GameTutorialComplete", 1);
                    break;
            }
        }

        yield return null;
    }

    void FragmentHighlight()
    {
        FragmentHighlighter.OnHighlightActivate -= FragmentHighlight;
        highlightDone = true;
        checkboxHighlight.sprite = checkboxTrueSprite;
        if (undoDone && highlightDone) ClosePanel(4, 2);
    }

    void FragmentUndo()
    {
        MoveUndo.OnMoveUndo -= FragmentUndo;
        undoDone = true;
        checkboxUndo.sprite = checkboxTrueSprite;
        if (undoDone && highlightDone) ClosePanel(4, 2);
    }

    void FragmentSwapped()
    {
        ClosePanel(1);
        Fragment.OnFragmentSwapped -= FragmentSwapped;
    }

    void FragmentPickup()
    {
        ClosePanel(2);
        Fragment.OnFragmentSwapped -= FragmentPickup;
    }

    public void ClosePanel(int panelNumber)
    {
        ClosePanel(panelNumber, 0);
    }

    public void ClosePanel(int panelNumber, float delay = 0)
    {
        if (delay > 0) StartCoroutine(EClosePanel(panelNumber, delay));
        else tutorialPanels[panelNumber].Deactivate();
    }

    IEnumerator EClosePanel(int panelNumber, float delay)
    {
        yield return new WaitForSeconds(delay);
        tutorialPanels[panelNumber].Deactivate();
        yield return null;
    }

    public void PanelClosed(int panelNumber)
    {

        switch (panelNumber)
        {
            case 4: //Choice has been made on what action to use, wait for game to finish
                GameStateManager.OnGameOver += GameOverPanel;
                break;
            default:
                StartCoroutine(ShowPanel(panelNumber + 1));
                break;
        }
    }

    void GameOverPanel(bool b, int i, int j)
    {
        StartCoroutine(ShowPanel(5));
        GameStateManager.OnGameOver -= GameOverPanel;
    }

    void OnDisable()
    {
        Fragment.OnFragmentSwapped -= FragmentSwapped;
        Fragment.OnFragmentSwapped -= FragmentPickup;
        GameStateManager.OnGameOver -= GameOverPanel;
        FragmentHighlighter.OnHighlightActivate -= FragmentHighlight;
        MoveUndo.OnMoveUndo -= FragmentUndo;

    }
}
