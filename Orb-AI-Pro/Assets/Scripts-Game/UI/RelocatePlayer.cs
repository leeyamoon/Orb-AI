using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RelocatePlayer : MonoBehaviour
{
    public enum StartingLoc
    {
        Location1, Location2, Location3 ,Location4 ,Location5, Location6, Location7, Location8, Location9,Location10,
        Location11, Location12, Location13 ,Location14 ,Location15, Location16, Location17, Location18, Location19,
        Location20
    }

    public StartingLoc startingLocation;


    private void Start()
    {
        print(startingLocation);
        if(startingLocation == 0)
            return;
        transform.position = RewardBehavior.Shared().allGoalsTransform[(int) startingLocation-1].position;
        RewardBehavior.Shared().ChangeIndex((int) startingLocation-1);
    }
}
