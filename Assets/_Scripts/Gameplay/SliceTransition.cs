using System;
using System.Collections;
using System.Collections.Generic;
using _Scripts.Visual_Elements;
using UnityEngine;
using UnityEngine.UI;

public class SliceTransition : MonoBehaviour
{
    public FragmentCreator fragmentCreator;

    public Image image;

    public event Action OnVisionCovered;
    public event Action OnVisionRevealed;

    public Animator animator;


    void OnEnable()
    {
        fragmentCreator.fragmentsCreated += Reveal;
    }
    void OnDisable()
    {
        fragmentCreator.fragmentsCreated -= Reveal;
    }

    public void Cover()
    {
        animator.SetTrigger("Cover");
    }


    public void VisionCoverFinished()
    {
        OnVisionCovered?.Invoke();
    }
    public void Reveal()
    {
        animator.SetTrigger("Reveal");
        AudioManager.Instance.PlaySound("CloudTransition");
    }

    public void RevealNoPreview()
    {
        fragmentCreator.CreateFragments();
    }

    public void VisionRevealFinished()
    {
        OnVisionRevealed?.Invoke();
    }


}
