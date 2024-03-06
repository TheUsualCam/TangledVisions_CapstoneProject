using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MoveUndo : MonoBehaviour
{
    public FragmentSwapper fragmentSwapper;
    public Fragment lastMoveFragmentA;
    public Fragment lastMoveFragmentB;

    public Button undoButton;

    public static event Action OnMoveUndo;

    void OnEnable()
    {
        GameStateManager.OnGameStart += StartTrackingMoves;
    }

    void OnDisable()
    {
        FragmentSwapper.FragmentsSwapped -= RegisterNewMove;
        GameStateManager.OnGameStart -= StartTrackingMoves;
    }

    void OnDestroy()
    {
        FragmentSwapper.FragmentsSwapped -= RegisterNewMove;
    }

    void StartTrackingMoves()
    {
        FragmentSwapper.FragmentsSwapped += RegisterNewMove;
        undoButton.interactable = false;
    }

    void RegisterNewMove(Fragment frag1, Fragment frag2)
    {
        lastMoveFragmentA = frag1;
        lastMoveFragmentB = frag2;
        undoButton.interactable = true;
    }

    public void Undo()
    {
        if (lastMoveFragmentA && lastMoveFragmentB)
        {
            fragmentSwapper.ChangeMaterials(lastMoveFragmentA, lastMoveFragmentB);
            lastMoveFragmentA.PlaySwapFragmentAnimation();
            lastMoveFragmentB.PlaySwapFragmentAnimation();
            lastMoveFragmentA = null;
            lastMoveFragmentB = null;
            undoButton.interactable = false;
            OnMoveUndo?.Invoke();
        }
    }
    
}
