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
    public void Destroy(){
        ClearAll();
    }

    public void CreateDumb(Vector2 position){
        GameObject dumbObj = GameObject.Instantiate(Resources.Load("DumbAthlete")) as GameObject;
        dumbObj.transform.position = position;
        DumbAthlete dumb = dumbObj.GetComponent<DumbAthlete>();
        dumbAthletes.Add(dumb);

    }

    public int NumberOfDumbAthletes(){
        return dumbAthletes.Count;
    }

    internal void DestroyEnemy(GameObject g){
        dumbAthletes.Remove(g.GetComponent<DumbAthlete>());
        GameObject.Destroy(g);
    }

    internal void ClearAll(){
        foreach(DumbAthlete dumb in dumbAthletes){
            dumbAthletes.Remove(dumb);
            GameObject.Destroy(dumb.gameObject);
        }
    }
}
