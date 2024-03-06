using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GameMap : MonoBehaviour
{
    [SerializeField] private List<MapLevel> levelsOnMap;

    [SerializeField] private List<MapCampaign> campaigns = new List<MapCampaign>();
    
    // Start, This is the entry point for the Map Scene.
    void Start()
    {
        //Get all campaigns & Update
        foreach (MapCampaign campaign in transform.GetComponentsInChildren<MapCampaign>())
        {
            campaigns.Add(campaign);
            campaign.UpdateCampaign();
            for (int level = 0; level < campaign.campaignLevels.Count; level++)
            {
                levelsOnMap.Add(campaign.campaignLevels[level]);
            }
        }

        UpdateMap();
        
        
    }

    private void UpdateMap()
    {
        for (int level = 0; level < levelsOnMap.Count; level++)
        {
            MapLevel levelScript = levelsOnMap[level];
            
            bool showUnlocked = false;
            //if its the first level, or the previous level is complete, then its unlocked.
            if (level <= 0) showUnlocked = true;
            else if (levelsOnMap[level - 1].levelState == MapLevel.levelStatus.Complete) showUnlocked = true;

            levelScript.UpdateLevel(showUnlocked);
        }
    }

}
