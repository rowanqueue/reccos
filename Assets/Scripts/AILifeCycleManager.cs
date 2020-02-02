using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AILifeCycleManager
{
    public List<DumbAthlete> dumbAthletes;

    public void Initialize(){
        dumbAthletes = new List<DumbAthlete>();
        DumbAthlete[] dumbs = GameObject.FindObjectsOfTypeAll(typeof(DumbAthlete)) as DumbAthlete[];
        foreach(DumbAthlete d in dumbs){
            dumbAthletes.Add(d);
        }
    }
    public void Update()
    {
        foreach(DumbAthlete dumb in dumbAthletes){
            dumb.TargetBall();
        }
    }
}
