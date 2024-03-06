using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FragmentHighlighter : MonoBehaviour
{
    private FragmentCreator fragmentCreator;
    public bool highlightActive;
    public static event Action OnHighlightActivate;

    void Awake()
    {
        fragmentCreator = GetComponent<FragmentCreator>();
    }

    public void ToggleHighlight(bool toggle)
    {
        highlightActive = toggle;
        List<Fragment> fragments = fragmentCreator.fragments;

        foreach (Fragment fragment in fragments)
        {
            fragment.ToggleOutline(toggle);
        }

        //Invoke if its toggled on.
        if(toggle) OnHighlightActivate?.Invoke();
    }
}