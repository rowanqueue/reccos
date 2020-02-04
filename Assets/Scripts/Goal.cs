using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Goal
{
    public int team;
    public GameObject gameObject;
    public Collider2D collider;
    SpriteRenderer[] spriteRenderers;

    public Goal(GameObject gameObject){
        this.gameObject = gameObject;
        spriteRenderers = gameObject.GetComponentsInChildren<SpriteRenderer>();
        collider = gameObject.GetComponent<Collider2D>();
    }
    public Goal SetTeam(int team){
        this.team = team;
        foreach(SpriteRenderer sr in spriteRenderers){
            sr.color = Services.GameController.teamColors[team];
        }
        return this;
    }
}
