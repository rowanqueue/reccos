using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AILifeCycleManager
{
    public List<Athlete> Athletes;

    public void Initialize(){
        Athletes = new List<Athlete>();
    }
    public void Update()
    {
        foreach(Athlete dumb in Athletes){
            dumb.Update();
        }
    }
    public void Destroy(){
        ClearAll();
    }

    public void CreateDumb(Vector2 position,int team){
        GameObject dumbObj = GameObject.Instantiate(Resources.Load("Athlete")) as GameObject;
        dumbObj.transform.parent = Services.GameController.game.transform;
        Athletes.Add(new Athlete(dumbObj).SetTeam(team).SetPosition(position.x,position.y));

    }

    public int NumberOfAthletes(){
        return Athletes.Count;
    }

    internal void DestroyEnemy(GameObject g){
        Athletes.Remove(g.GetComponent<Athlete>());
        GameObject.Destroy(g);
    }

    internal void ClearAll(){
        foreach(Athlete dumb in Athletes){
            Athletes.Remove(dumb);
            GameObject.Destroy(dumb.gameObject);
        }
    }
}
