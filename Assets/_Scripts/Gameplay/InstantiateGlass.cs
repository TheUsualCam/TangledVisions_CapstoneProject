using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstantiateGlass : MonoBehaviour
{
    public GameObject brokenGlass;
    public float glassDestroyTime = 2.0f;
    private GameObject instantiatedGlass;

    public void InstantiateBrokenGlass()
    {
        instantiatedGlass = Instantiate(brokenGlass);
        instantiatedGlass.SetActive(true);

        StartCoroutine(DestroyGlassAfterTime());
    }

    IEnumerator DestroyGlassAfterTime()
    {
        yield return new WaitForSeconds(glassDestroyTime);
        Destroy(instantiatedGlass);
    }
}
