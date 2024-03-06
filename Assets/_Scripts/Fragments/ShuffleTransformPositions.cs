using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Component = System.ComponentModel.Component;

public class ShuffleTransformPositions : MonoBehaviour
{
    public List<Transform> transforms;

    private List<Vector3> initialPositions;

    private void SaveInitialPositions()
    {
        // Store the initial positions of the transforms
        initialPositions = new List<Vector3>();
        foreach (Transform transformToShuffle in transforms)
        {
            initialPositions.Add(transformToShuffle.position);
        }
    }

    public void ShuffleTransforms(List<Transform> transformsToShuffle)
    {
        transforms = transformsToShuffle;
        SaveInitialPositions();
        ShuffleTransforms();
    }

    public void ShuffleTransforms()
    {
        Random.InitState(SeedController.Instance.GetCurrentSeed());
        int count = transforms.Count;

        // Fisher-Yates shuffle algorithm
        while (count > 1)
        {
            count--;
            int randomIndex = Random.Range(0, count + 1);
            //Swap positions
            (transforms[randomIndex], transforms[count]) = (transforms[count], transforms[randomIndex]);
        }

        // Apply shuffled positions to the transforms
        for (int i = 0; i < transforms.Count; i++)
        {
            transforms[i].position = initialPositions[i];
            transforms[i].gameObject.SetActive(true);
        }

        StartCoroutine(DisableCamerasAtEndOfFrame());

    }

    private IEnumerator DisableCamerasAtEndOfFrame()
    {
        yield return new WaitForEndOfFrame();
        for (int i = 0; i < transforms.Count; i++)
        {
            transforms[i].gameObject.SetActive(false);
        }
        yield return null;
    }
}
