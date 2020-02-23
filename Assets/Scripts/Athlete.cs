using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorTree;

public class Athlete
{
    public Transform transform;
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
    private BehaviorTree.Tree<Athlete> tree;
    public Athlete(GameObject gameObject){
        this.gameObject = gameObject;
        this.transform = gameObject.transform;
        spriteRenderer = gameObject.transform.GetChild(0).GetComponent<SpriteRenderer>();
        rb = gameObject.GetComponent<Rigidbody2D>();
        playerTracker = gameObject.transform.GetChild(1).gameObject;
        athleteStateMachine = new FiniteStateMachine<Athlete>(this);
        athleteStateMachine.TransitionTo<InGame>();
        tree = new Tree<Athlete>(
            new Selector<Athlete>(
                new Sequence<Athlete>(
                    new StraightShot(),
                    new BallIntoGoal()
                ),
                new RepositionForBall()
                
            )
        );
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
    private class InGame : AthleteState
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
                Context.tree.Update(Context);
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
        public override void OnEnter(){
            Context.ready = false;
            Services.EventManager.Register<ReadyUp>(Context.GoToGame);
        }
        public override void Update(){
            Context.targetVelocity = (Context.spawnPosition - Context.gameObject.transform.position).normalized;
            if(!Context.ready){
                Context.Move();
            }
            
            if(Vector2.Distance(Context.spawnPosition,Context.gameObject.transform.position) < 0.5f){
                Context.ready = true;
            }
        }
        public override void OnExit(){
            Services.EventManager.Unregister<ReadyUp>(Context.GoToGame);
        }
    }
    public void GoToResetPosition(AGPEvent e){
        athleteStateMachine.TransitionTo<ResetPosition>();
    }
    public void GoToGame(AGPEvent e){
        Debug.Log("Suer");
        athleteStateMachine.TransitionTo<InGame>();
    }
    public class StraightShot : BehaviorTree.Node<Athlete>{
        public override bool Update(Athlete context){
            Vector2 toGoal = (Services.GameController.goals[context.team].transform.position - context.transform.position).normalized;
            Vector2 toBall = (Services.Ball.transform.position - context.transform.position).normalized;
            float difference = Vector2.Angle(toGoal,toBall);
            Debug.Log(difference);
            return difference <= 30f;
        }
    }
    public class BallIntoGoal : BehaviorTree.Node<Athlete>
    {
        public override bool Update(Athlete context){
            context.targetVelocity = (Services.Ball.transform.position - context.transform.position).normalized;
            return true;
        }
    }
    public class RepositionForBall : BehaviorTree.Node<Athlete>
    {
        public override bool Update(Athlete context){
            
            Vector2 toBall = (Services.Ball.transform.position - context.transform.position).normalized;

            toBall = new Vector2(toBall.y,-toBall.x);
            context.targetVelocity = toBall.normalized;
            return true;
        }
    }
}
