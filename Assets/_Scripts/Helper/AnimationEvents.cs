using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class AnimationEvents : MonoBehaviour
{
    public UnityEvent onAnimationEnd;
    public UnityEvent animationEventA;
    public UnityEvent animationEventB;

    public void OnAnimationEnd()
    {
        onAnimationEnd.Invoke();
    }
    public void AnimationEventA()
    {
        animationEventA.Invoke();
    }

    public void AnimationEventB()
    {
        animationEventB.Invoke();
    }
}
