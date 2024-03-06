using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CampaignButton : MonoBehaviour
{
    public Campaign campaign;

    public static event Action<Campaign> OnCampaignSelected;

    public GameObject lockObject;

    // Start is called before the first frame update
    void Start()
    {
        if(campaign == null)
        {
            gameObject.SetActive(false);
            return;
        }
    }

    public void SelectCampaign()
    {
        OnCampaignSelected?.Invoke(campaign);

    }

    

   

}
