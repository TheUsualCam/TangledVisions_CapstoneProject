using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class StarAddedAnimation : MonoBehaviour
{
    private Vector3 startPos = Vector3.zero;
    public RectTransform target;
    public float animationLerp;
    public Animator animator;
    private float soundPitch = 1;
    public StarCounter counter;
    

    public void SetTarget(RectTransform newTarget, float audioPitch)
    {
        target = newTarget;
        startPos = transform.position;
        animator.enabled = true;
        soundPitch = audioPitch;


    }

    public void Update()
    {
        transform.position = transform.position = Vector3.Lerp(startPos, target.position,
            animationLerp);
        
        //When complete, destroy.
        if (animationLerp >= 1)
        {
            AudioManager.Instance.PlaySound("StarAdded", soundPitch);
            counter.AddToStarText();
            Destroy(gameObject);
        }
        
    }
}
