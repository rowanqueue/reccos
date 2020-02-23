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
    protected SpriteRenderer spriteRenderer;
    protected Rigidbody2D rb;
    GameObject playerTracker;

    Vector3 spawnPosition;
    public bool ready;

    private FiniteStateMachine<Athlete> athleteStateMachine;
    private BehaviorTree.Tree<Athlete> tree;
    float lastTreeUpdate = 0f;

    float anger = 0;
    float scared = 0;

    bool isReferee;
    public Athlete(GameObject gameObject){
        this.gameObject = gameObject;
        this.transform = gameObject.transform;
        spriteRenderer = gameObject.transform.GetChild(0).GetComponent<SpriteRenderer>();
        rb = gameObject.GetComponent<Rigidbody2D>();
        playerTracker = gameObject.transform.GetChild(1).gameObject;
        athleteStateMachine = new FiniteStateMachine<Athlete>(this);
        athleteStateMachine.TransitionTo<InGame>();
        BehaviorTree.Tree<Athlete> happyTree = new Tree<Athlete>(
            new Selector<Athlete>(
                new Sequence<Athlete>(
                    new NotStraightShot(),
                    new RepositionForBall()
                ),
                new Sequence<Athlete>(
                    new TooFar(),
                    new BallIntoGoal()
                ),
                new Sequence<Athlete>(
                    new SomethingBlockingBall(),
                    new RepositionForBlock()
                ),
                new BallIntoGoal()

            )
        );
        BehaviorTree.Tree<Athlete> angryTree = new Tree<Athlete>(
            new Selector<Athlete>(
                new Sequence<Athlete>(
                    new IsAngry(),
                    new Charge()
                ),
                new Sequence<Athlete>(
                    new NotStraightShot(),
                    new RepositionForBall()
                ),
                new Sequence<Athlete>(
                    new TooFar(),
                    new BallIntoGoal()
                ),
                new Sequence<Athlete>(
                    new SomethingBlockingBall(),
                    new RepositionForBlock()
                ),
                new BallIntoGoal()
            )
        );
        BehaviorTree.Tree<Athlete> scaredTree = new Tree<Athlete>(
            new Selector<Athlete>(
                new Sequence<Athlete>(
                    new IsScared(),
                    new RunAway()
                ),
                new Sequence<Athlete>(
                    new NotStraightShot(),
                    new RepositionForBall()
                ),
                new Sequence<Athlete>(
                    new TooFar(),
                    new BallIntoGoal()
                ),
                new Sequence<Athlete>(
                    new SomethingBlockingBall(),
                    new RepositionForBlock()
                ),
                new BallIntoGoal()
            )
        );
        float value = Random.value;
        if(value < 0.33f){
            tree = happyTree;
        }else if(value < 0.66f){
            tree = angryTree;
        }else{
            tree = scaredTree;
        }
    }
    public Athlete SetTeam(int team){
        this.team = team;
        spriteRenderer.color = Services.GameController.teamColors[team];
        isReferee = team == 2;
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

        anger+=Random.Range(0.001f,0.02f);
        anger = Mathf.Clamp(anger,0,1f);
        if(Vector2.Distance(transform.position,Services.Ball.transform.position) > 3f){
            scared -= 0.3f;
        }else{
            scared+= Random.Range(0.001f,0.02f);
        }
        
        scared = Mathf.Clamp(scared,0,1f);
        
        

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
                if(Context.isReferee){
                    if(Vector2.Distance(Context.transform.position,Services.Ball.transform.position) < 3f){
                        Context.targetVelocity = (Context.transform.position-Services.Ball.transform.position).normalized;
                    }else{
                        Context.targetVelocity = (Services.Ball.transform.position-Context.transform.position).normalized;
                    }
                }else{
                    if(Time.time >= Context.lastTreeUpdate+0.25f){
                        Context.tree.Update(Context);
                        Context.lastTreeUpdate = Time.time;
                    }
                }
                
               
            }
            if(Context.targetVelocity != Vector2.zero){
                Context.Move();
            }
        }
        public override void OnExit(){
            Services.EventManager.Unregister<GoalScored>(Context.GoToResetPosition);
        }
    }
    public float AngleBetweenObjects(Vector2 position,Vector2 middle, Vector2 end){
        Vector2 toGoal = (end - position).normalized;
        Vector2 toBall = (middle - position).normalized;
        float difference = Vector2.Angle(toGoal,toBall);
        return difference;
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
    public class NotStraightShot : BehaviorTree.Node<Athlete>{
        public override bool Update(Athlete context){
            float difference = context.AngleBetweenObjects(context.transform.position,Services.Ball.transform.position,Services.GameController.goals[context.team].transform.position);
            return !(difference <= 20f);
        }
    }
    public class SomethingBlockingBall : BehaviorTree.Node<Athlete>{
        public override bool Update(Athlete context){
            RaycastHit2D hit2D;
            Vector2 toBall = (Services.Ball.transform.position - context.transform.position).normalized;
            hit2D = Physics2D.Raycast(context.transform.position,toBall);
            if(hit2D.collider != null){
                if(hit2D.collider.CompareTag("Ball")){
                    return !true;
                }else{
                    return !false;
                }
            }else{
                return !true;
            }
        }
    }
    public class TooFar : BehaviorTree.Node<Athlete>{
        public override bool Update(Athlete context){
            return Vector2.Distance(context.transform.position,Services.Ball.transform.position) > 4f;
        }
    }
    public class IsAngry : BehaviorTree.Node<Athlete>{
        public override bool Update(Athlete context){
            return context.anger >= 1.0f;
        }
    }

    public class Charge : BehaviorTree.Node<Athlete>{
        public override bool Update(Athlete context){
            Vector2 target = context.transform.position;
            float distance = 10000000f;
            foreach(Athlete athlete in Services.AILifeCycleManager.Athletes){
                if(athlete == context){
                    continue;
                }
                if(athlete.team == context.team){
                    continue;
                }
                float thisDistance = Vector2.Distance(athlete.transform.position,context.transform.position);
                if(thisDistance < distance){
                    distance = thisDistance;
                    target = athlete.transform.position;
                }
            }
            context.targetVelocity = (target-(Vector2)context.transform.position).normalized;
            if(distance < 3f){
                context.anger = 0;
            }
            return true;
        }
    }
    public class IsScared : BehaviorTree.Node<Athlete>{
        public override bool Update(Athlete context){
            return context.scared >= 1.0f;
        }
    }
    public class RunAway : BehaviorTree.Node<Athlete>{
        public override bool Update(Athlete context){
            context.targetVelocity = (context.transform.position-Services.Ball.transform.position).normalized;
            return true;
        }
    }
    public class BallIntoGoal : BehaviorTree.Node<Athlete>
    {
        public override bool Update(Athlete context){
            float difference = context.AngleBetweenObjects(context.transform.position,Services.Ball.transform.position,Services.GameController.goals[context.team].transform.position);
            Vector2 velocity = (Services.Ball.transform.position - context.transform.position).normalized;
            velocity += new Vector2(Mathf.Cos(Mathf.Deg2Rad*difference)*0.5f,Mathf.Sin(Mathf.Deg2Rad*difference)*0.5f);
            context.targetVelocity = velocity.normalized;
            return true;
        }
    }
    public Vector2 Reposition(Vector2 self, Vector2 target, Vector2 middle){
        Vector2 toBall = (target - self).normalized;
        float difference = AngleBetweenObjects(self,middle,target);
        Vector2 toBallUp = new Vector2(toBall.y,-toBall.x);
        float differenceUp = AngleBetweenObjects(new Vector2(self.x+toBallUp.x,self.y+toBallUp.y),middle,target);
        Vector2 toBallDown = new Vector2(-toBall.y,toBall.x);
        float differenceDown = AngleBetweenObjects(new Vector2(self.x+toBallDown.x,self.y+toBallDown.y),middle,target);
        if(differenceUp < differenceDown){
            return toBallUp.normalized;
        }else{
            return toBallDown.normalized;
        }
    }
    public class RepositionForBall : BehaviorTree.Node<Athlete>
    {
        public override bool Update(Athlete context){
            context.targetVelocity = context.Reposition(context.transform.position,Services.Ball.transform.position,Services.GameController.goals[context.team].transform.position);
            return true;
        }
    }
    public class RepositionForBlock : BehaviorTree.Node<Athlete>
    {
        public override bool Update(Athlete context){
            RaycastHit2D hit2D;
            Vector2 toBall = (Services.Ball.transform.position - context.transform.position).normalized;
            hit2D = Physics2D.Raycast(context.transform.position,toBall);
            Vector2 obstaclePosition = Vector2.zero;
            if(hit2D.collider != null){
                if(hit2D.collider.CompareTag("Ball")){
                    return false;
                }else{
                    obstaclePosition = hit2D.collider.transform.position;
                }
            }else{
                return false;
            }
            context.targetVelocity = context.Reposition(context.transform.position,obstaclePosition,Services.Ball.transform.position);
            return true;
        }
    }
}