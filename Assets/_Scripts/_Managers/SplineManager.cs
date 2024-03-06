using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Splines;

public class SplineManager : MonoBehaviour
{

    public float forwardPercentage = 0;
    public float backPercentage = 0;
    public float SpeedModifier = 8;

    public bool start;
    public bool end;

    public List<SplineExtrude> splines;

    private void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (start)
        {
            forwardPercentage += Time.deltaTime / SpeedModifier;


            foreach(SplineExtrude spline in splines)
            {
                spline.Range = new Vector2(0, forwardPercentage);
                spline.Rebuild();
            }

            
        }

        if (end)
        {
            backPercentage += Time.deltaTime / SpeedModifier;


            foreach (SplineExtrude spline in splines)
            {
                spline.Range = new Vector2(backPercentage, 1);
                spline.Rebuild();
            }

            if(backPercentage < 0)
            {
                end = false;
            }
        }
    }

    public void resetForwardPercentage()
    {
        forwardPercentage = 0;
    }

    public void resestBackPercentage()
    {
        backPercentage = 0;
    }
}
