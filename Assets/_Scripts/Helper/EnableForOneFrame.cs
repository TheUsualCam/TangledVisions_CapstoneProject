using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnableForOneFrame : MonoBehaviour
{
    public Behaviour componentAffected;
    // Start is called before the first frame update
    public void EnableForFrame()
    {
        StartCoroutine(DisableCameraAtEndOfFrame());
    }

    private IEnumerator DisableCameraAtEndOfFrame()
    {
        yield return new WaitForEndOfFrame();
        componentAffected.enabled = false;
        yield return null;
    }
}
