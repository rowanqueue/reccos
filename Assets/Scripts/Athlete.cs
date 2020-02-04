using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Athlete
{
    public bool playerControlled;
    public float movementSpeed = 5f;
    public float movementSmoothing = 0.05f;
    protected Vector2 targetVelocity;

    private Vector2 velocity = Vector2.zero;

    public int team;

    public GameObject gameObject;
    SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;
    GameObject playerTracker;
    public Athlete(GameObject gameObject){
        this.gameObject = gameObject;
        spriteRenderer = gameObject.transform.GetChild(0).GetComponent<SpriteRenderer>();
        rb = gameObject.GetComponent<Rigidbody2D>();
        playerTracker = gameObject.transform.GetChild(1).gameObject;
    }
    public Athlete SetTeam(int team){
        this.team = team;
        spriteRenderer.color = Services.GameController.teamColors[team];
        return this;
    }
    public Athlete SetPosition(float x, float y){
        gameObject.transform.position = new Vector3(x,y);
        return this;
    }
    public virtual void Update(){
        if(playerControlled){
            playerTracker.SetActive(true);
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");
            targetVelocity = new Vector2(horizontal,vertical).normalized;
        }else{
            playerTracker.SetActive(false);
            targetVelocity = (Services.Ball.transform.position - gameObject.transform.position).normalized;
        }
        if(targetVelocity != Vector2.zero){
            Move();
        }

    }
    public void Move(){
        rb.velocity = Vector2.SmoothDamp(rb.velocity, targetVelocity*movementSpeed,ref velocity,movementSmoothing);

    }
}
