using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DumbAthlete : Athlete
{
    // Update is called once per frame
    public void TargetBall()
    {
        targetVelocity = Vector2.up;
    }
}
