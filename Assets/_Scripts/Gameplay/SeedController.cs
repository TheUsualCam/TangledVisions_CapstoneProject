using System;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class SeedController : Singleton<SeedController>
{
    private int currentSeed;
    public Campaign currentCampaign;
    public TMP_InputField seedInput;
    
    public int GetCurrentSeed()
    {
        currentCampaign = LevelManager.Instance.campaign;
        UpdateSeed();
        return currentSeed;
    }

    public void UpdateSeed()
    { 
        //Check if a custom seed has been added
        string input = seedInput.text;
        int number = 0;
        try
        { 
            number = int.Parse(input);
            Console.WriteLine("Converted '{0}' to {1}.", input, number);
        }
        catch (FormatException)
        {
            Console.WriteLine("Unable to convert '{0}'.", input);
        }
        catch (OverflowException)
        {
            Console.WriteLine("'{0}' is out of range of the Int32 type.", input);
        }
        
        Vision currenVision = LevelManager.Instance.campaign.visions[LevelManager.Instance.campaign.currentVision];
        //If a custom seed has been successfully input
        if (number > 0) 
            currentSeed = number;
        //No custom seed, use campaigns seed.
        else if (currenVision.seed > 0)
            currentSeed = currenVision.seed;
        //Else get a random seed
        else
            currentSeed = Random.Range(0, int.MaxValue);



    }
}
