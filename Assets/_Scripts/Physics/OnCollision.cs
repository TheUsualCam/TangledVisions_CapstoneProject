using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class OnCollision : MonoBehaviour
{
    public UnityEvent onCollisionEnter;
    private void OnCollisionEnter(Collision other)
    {
        onCollisionEnter?.Invoke();
    }
    
    public UnityEvent onCollisionExit;
    private void OnCollisionExit(Collision other)
    {
        onCollisionExit?.Invoke();
    }

    private void OnCollisionStay(Collision other)
    {
        onCollisionExit?.Invoke();
        Physics2D.Raycast(Vector2.zero, Vector2.down);
    }

}

