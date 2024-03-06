using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class FragmentSwapper : MonoBehaviour
{
    private FragmentCreator maskCreator;
    public static event Action<Fragment, Fragment> FragmentsSwapped;
    
    void Start()
    {
        maskCreator = GetComponent<FragmentCreator>();
    }

    public void ChangeRandomMaterials()
    {
        Random.InitState(SeedController.Instance.GetCurrentSeed());
        int maskA = Random.Range(0, maskCreator.fragments.Count);
        int maskB = Random.Range(0, maskCreator.fragments.Count);
        ChangeMaterials(maskA, maskB);
    }
    public void ChangeMaterials(int maskA, int maskB)
    {
        ChangeMaterials(maskCreator.fragments[maskA], maskCreator.fragments[maskB]);
    }

    public void ChangeMaterials(Fragment frag1, Fragment frag2)
    {
        if (frag1 != null && frag2 != null)
        {
            
            ChangeMaterials(frag1.meshRenderer, frag2.meshRenderer);
            ChangeUVs(frag1.filter, frag2.filter);
            FragmentsSwapped?.Invoke(frag1, frag2);
            frag1.CalculateAndReorderUvCoordinates();
            frag2.CalculateAndReorderUvCoordinates();
            frag1.UpdateFragmentMaterial();
            frag2.UpdateFragmentMaterial();

        }
    }

    public void ChangeMaterials(MeshRenderer meshA, MeshRenderer meshB)
    {
        Material maskAMaterial = meshA.material;
        Material maskBMaterial = meshB.material;
        meshA.material = maskBMaterial;
        meshB.material = maskAMaterial;
    }

    public void ChangeUVs(MeshFilter meshA, MeshFilter meshB)
    {
        //Cache the UVs of each fragment
        Vector2[] meshAUVSet = meshA.mesh.uv;
        Vector2[] meshBUVSet = meshB.mesh.uv;

        

        //Set the UVs of the fragments.
        meshA.mesh.uv = meshBUVSet;
        meshB.mesh.uv = meshAUVSet;


        //DEBUG Showing vertex colours
        //Color[] vertexColourArray = new Color[] { Color.red, Color.green, Color.blue, Color.yellow };
        //meshA.mesh.colors = vertexColourArray;
        //meshB.mesh.colors = vertexColourArray;

    }

    public void ShuffleAllMaterials()
    {
        Random.InitState(SeedController.Instance.GetCurrentSeed());
        List<Fragment> unlockedFragments = maskCreator.fragments;
        List<Fragment> lockedFragments = new List<Fragment>();
        
        // Remove locked fragments from unlockedFragments, and keep them in their own list for updating later.
        for (int index = unlockedFragments.Count - 1; index >= 0; index--)
        {
            //Remove any fragments which are locked
            if (unlockedFragments[index].isLocked)
            {
                lockedFragments.Add(unlockedFragments[index]);
                unlockedFragments.RemoveAt(index);
            }
        }

        //Generate a list of indices for shuffling
        List<int> fragIndex = new List<int>();
        for (int i = 0; i < unlockedFragments.Count; i++)
        {
            fragIndex.Add(i);
        }
        
        // Fisher-Yates shuffle of fragIndex
        //Shuffle the list of indices
        for (int index = fragIndex.Count - 1; index > 0; index--)
        {
            int randomIndex = Random.Range(0, index + 1);
            //Repeat until it doesn't select itself.
            while (randomIndex == index)
            {
                randomIndex = Random.Range(0, index + 1);
            }
            //Rearrange indices.
            (fragIndex[index], fragIndex[randomIndex]) = (fragIndex[randomIndex], fragIndex[index]);
        }

        // Apply shuffled materials to the unlocked fragments
        for (int index = 0; index < unlockedFragments.Count; index++)
        {
            
            ChangeMaterials(unlockedFragments[index], unlockedFragments[fragIndex[index]]);
        }

        //Update the locked fragments
        foreach (var frag in lockedFragments)
        {
            frag.UpdateFragmentMaterial();
        }
    }
}
