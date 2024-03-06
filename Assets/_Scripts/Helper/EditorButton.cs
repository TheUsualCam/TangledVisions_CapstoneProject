using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EditorButton : MonoBehaviour
{
    public UnityEvent OnPressEvent;

    public void OnPress()
    {
        OnPressEvent?.Invoke();
    }
}
