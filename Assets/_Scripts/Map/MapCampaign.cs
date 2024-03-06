using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapCampaign : MonoBehaviour
{
    public string campaignName;
    public Campaign campaign;
    public List<MapLevel> campaignLevels = new List<MapLevel>();

    // Start is called before the first frame update
    public void UpdateCampaign()
    {
        //Update stars on campaign to saved data.
        for(int i = 0; i < campaign.visions.Length; i++)
        {
            bool complete = PlayerPrefs.GetInt($"{campaign.campaignName}_{i}", 0) > 0;
            campaign.visions[i].starsToEarn[0] = complete;
            if (complete)
            {
                int moves = PlayerPrefs.GetInt($"{campaign.campaignName}_{i}_Moves", 0);
                bool moveStar = (moves > 0 && moves <= campaign.visions[i].targetNumberOfMoves);
                
                int time = PlayerPrefs.GetInt($"{campaign.campaignName}_{i}_Time", 0);
                bool timeStar = (time > 0 && time < campaign.visions[i].targetTimeInSeconds);

                campaign.visions[i].starsToEarn[1] = (moveStar || timeStar);
                campaign.visions[i].starsToEarn[2] = (moveStar && timeStar);
            }
            
        }
        //Go through each child (Game level)
        for (int index = 0; index < transform.childCount; index++)
        {
            MapLevel level = transform.GetChild(index).GetComponent<MapLevel>();
            //Set this as its campaign
            level.campaign = this;
            //Update the index to that of the child index.
            level.levelIndex = index;
            campaignLevels.Add(level);
        }
        
    }
}
