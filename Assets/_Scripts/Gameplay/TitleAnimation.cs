using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TitleAnimation : MonoBehaviour
{
    public bool animating;
    public Material titleTangledMaterial;
    public Material titleVisionMaterial;

    public float tangledFaceSoftness;
    public float tangledFaceDilation;
    
    public float visionFaceSoftness;
    public float visionFaceDilation;


    // Update is called once per frame
    void Update()
    {
        if (animating)
        {
            titleTangledMaterial.SetFloat("_FaceDilate", tangledFaceDilation);
            titleTangledMaterial.SetFloat("_OutlineSoftness", tangledFaceSoftness);
            titleVisionMaterial.SetFloat("_FaceDilate", visionFaceDilation);
            titleVisionMaterial.SetFloat("_OutlineSoftness", visionFaceSoftness);
        }
    }

    public void PlayTitleIntroSound()
    {
        AudioManager.Instance.PlaySound("titleintro");
    }
}
