using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Goal
{
    public int team;
    public int score;
    GameObject pointDisplay;Vector2 nextPointPosition;
    public GameObject gameObject;
    public Collider2D collider;
    SpriteRenderer[] spriteRenderers;

    public Goal(GameObject gameObject){
        this.gameObject = gameObject;
        spriteRenderers = gameObject.transform.GetChild(0).GetComponentsInChildren<SpriteRenderer>();
        collider = gameObject.GetComponent<Collider2D>();
        Services.EventManager.Register<GoalScored>(OnGoalScored);
        pointDisplay = gameObject.transform.GetChild(1).GetChild(0).gameObject;
        //Destroy()
        nextPointPosition = pointDisplay.transform.position;
    }
    public Goal SetTeam(int team){
        this.team = team;
        foreach(SpriteRenderer sr in spriteRenderers){
            sr.color = Services.GameController.teamColors[team];
        }
        return this;
    }
    void OnGoalScored(AGPEvent e){
        var go = (GoalScored)e;
        if(go.whichTeam == team){
            score++;
            if(score == 1){
                pointDisplay.SetActive(true);
            }else{
                GameObject.Instantiate(pointDisplay,nextPointPosition,Quaternion.identity,pointDisplay.transform.parent);
            }
            nextPointPosition += Vector2.up*0.5f;
        }

    }

}
