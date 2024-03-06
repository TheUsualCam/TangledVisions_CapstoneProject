using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RandomDivider : MonoBehaviour
{
    public static List<float> DivideIntoRandomPieces(int numPieces, float minSize, float maxSize, int modifySeed=0)
    {
        Random.InitState(SeedController.Instance.GetCurrentSeed() + modifySeed);
        // Ensure that numPieces is at least 1 and maxSize is greater than minSize
        numPieces = Mathf.Max(numPieces, 1);

        List<float> sizes = new List<float>();

        for (int i = 0; i < numPieces; i++)
        {
            float size = Random.Range(minSize, maxSize);
            sizes.Add(size);
        }

        // Normalize the sizes to ensure the sum adds up to 1
        float totalSize = sizes.Sum();
        for (int i = 0; i < numPieces; i++)
        {
            sizes[i] /= totalSize;
        }

        // Shuffle the list to randomize the order of pieces
        Shuffle(sizes);

        return sizes;
    }

    private static void Shuffle<T>(List<T> list)
    {
        Random.InitState(SeedController.Instance.GetCurrentSeed());
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = Random.Range(0, n + 1);
            (list[k], list[n]) = (list[n], list[k]);
        }
    }
}
