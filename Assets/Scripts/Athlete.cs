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

    Vector3 spawnPosition;
    public bool ready;

    private FiniteStateMachine<Athlete> athleteStateMachine;
    public Athlete(GameObject gameObject){
        this.gameObject = gameObject;
        spriteRenderer = gameObject.transform.GetChild(0).GetComponent<SpriteRenderer>();
        rb = gameObject.GetComponent<Rigidbody2D>();
        playerTracker = gameObject.transform.GetChild(1).gameObject;
        athleteStateMachine = new FiniteStateMachine<Athlete>(this);
        athleteStateMachine.TransitionTo<ResetPosition>();
    }
    public Athlete SetTeam(int team){
        this.team = team;
        spriteRenderer.color = Services.GameController.teamColors[team];
        return this;
    }
    public Athlete SetPosition(float x, float y){
        gameObject.transform.position = new Vector3(x,y);
        spawnPosition = (Vector3)gameObject.transform.position;
        return this;
    }
    public virtual void Update(){
        Debug.Log(athleteStateMachine.CurrentState);
        /*if(athleteStateMachine.CurrentState==){
            Debug.Log("A");
        }*/
        athleteStateMachine.Update();
        

    }
    public void Move(){
        rb.velocity = Vector2.SmoothDamp(rb.velocity, targetVelocity*movementSpeed,ref velocity,movementSmoothing);

    }
    public abstract class AthleteState : FiniteStateMachine<Athlete>.State
    {

    }
    private class RunAtBall : AthleteState
    {
        public override void OnEnter(){
            Services.EventManager.Register<GoalScored>(Context.GoToResetPosition);
        }
        public override void Update(){
            if(Context.playerControlled){
                Context.playerTracker.SetActive(true);
                float horizontal = Input.GetAxis("Horizontal");
                float vertical = Input.GetAxis("Vertical");
                Context.targetVelocity = new Vector2(horizontal,vertical).normalized;
            }else{
                Context.playerTracker.SetActive(false);
                Context.targetVelocity = (Services.Ball.transform.position - Context.gameObject.transform.position).normalized;
            }
            if(Context.targetVelocity != Vector2.zero){
                Context.Move();
            }
        }
        public override void OnExit(){
            Services.EventManager.Unregister<GoalScored>(Context.GoToResetPosition);
        }
    }
    private class ResetPosition : AthleteState
    {
        public override void Update(){
            Context.ready = false;
            Context.targetVelocity = (Context.spawnPosition - Context.gameObject.transform.position).normalized;
            Context.Move();
            if(Vector2.Distance(Context.spawnPosition,Context.gameObject.transform.position) < 0.2f){
                TransitionTo<Ready>();
            }
        }
    }
    private class Ready : AthleteState
    {
        public override void Update(){
            Context.ready = true;
            if(Services.GameController.ready){
                TransitionTo<RunAtBall>();
            }
        }
    }
    public void GoToResetPosition(AGPEvent e){
        athleteStateMachine.TransitionTo<ResetPosition>();
    }
}
