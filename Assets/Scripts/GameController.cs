using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    public int scoreToWin;
    public int timeLimit;
    float passedTime;
    public TextMeshPro timeLimitText;
    /*public int numberOfTeams;
    public int numberPerTeam;*/
    public List<Color> teamColors;
    public Transform goalParent;
    public Transform athleteParent;
    public List<Goal> goals;
    public List<int> scores;


    private FiniteStateMachine<GameController> gameStateMachine;
    public GameObject startMenu;
    public GameObject game;
    public GameObject gameOver;
    public bool ready;
    // Start is called before the first frame update
    void Awake()
    {
        

        InitializeServices();
        for(int j=0;j<athleteParent.childCount;j++){
            Transform teamParent = athleteParent.GetChild(j);
            for(int i = 0; i < teamParent.childCount;i++){
                Transform spawnPoint = teamParent.GetChild(i);
                if(spawnPoint.gameObject.activeSelf){
                    Services.AILifeCycleManager.CreateDumb(new Vector2(spawnPoint.position.x,spawnPoint.position.y),j);
                }
            }
        }
        
        Services.AILifeCycleManager.Athletes[0].playerControlled = true;
        
    }
    private void Start(){
        passedTime = 0;
        goals = new List<Goal>();
        for(int i =0; i < goalParent.childCount;i++){
            goals.Add(new Goal(goalParent.GetChild(i).gameObject).SetTeam(i));
        }
        Services.EventManager.Register<GoalScored>(OnGoalScored);
        scores = new List<int>();
        for(int i = 0; i < goals.Count;i++){
            scores.Add(0);
        }
        Services.EventManager.Register<TimeOut>(OnTimeOut);
    }
    private void OnDestroy(){
        Services.EventManager.Unregister<GoalScored>(OnGoalScored);
        Services.EventManager.Unregister<TimeOut>(OnTimeOut);
    }
    void OnGoalScored(AGPEvent e){
        var goal = (GoalScored)e;
        scores[goal.whichTeam]++;
    }
    void OnTimeOut(AGPEvent e){
        gameStateMachine.TransitionTo<GameOver>();
    }
    void Update(){
        
        timeLimitText.text = ((timeLimit-(int)passedTime)/60)+":";
        if((int)(timeLimit-(int)passedTime)%60 < 10){
            timeLimitText.text+="0";
        }
        timeLimitText.text += (timeLimit-(int)passedTime)%60;
        gameStateMachine.Update();
    }


    private void InitializeServices(){
        Services.GameController = this;

        Services.EventManager = new EventManager();

        Services.AILifeCycleManager = new AILifeCycleManager();
        Services.AILifeCycleManager.Initialize();

        Services.Ball = GameObject.FindObjectOfType<Ball>();
        gameStateMachine = new FiniteStateMachine<GameController>(this);
        gameStateMachine.TransitionTo<StartMenu>();
    }
    private abstract class GameState : FiniteStateMachine<GameController>.State
    {

    }
    private class StartMenu : GameState
    {
        public override void OnEnter(){
            Context.game.SetActive(false);
            Context.startMenu.SetActive(true);
        }
        public override void Update(){
            if(Input.anyKeyDown){
                TransitionTo<ResetGame>();
            }
        }
        public override void OnExit(){
            Context.startMenu.SetActive(false);
        }
    }
    private  class InGame : GameState
    {
        public override void OnEnter(){
            Context.game.SetActive(true);
        }
        public override void Update(){
            Context.passedTime += Time.deltaTime;
            Services.AILifeCycleManager.Update();
            if(Context.scores[0] > Context.scoreToWin || Context.scores[1] > Context.scoreToWin){
                Context.gameStateMachine.TransitionTo<GameOver>();
            }
            if(Context.passedTime>Context.timeLimit){
                Services.EventManager.Fire(new TimeOut());
            }
        }
    }
    private class ResetGame : GameState
    {
        public override void OnEnter(){
            Context.game.SetActive(true);
        }
        public override void Update(){
            Context.passedTime += Time.deltaTime;
            Services.AILifeCycleManager.Update();
            Context.ready = true;
            foreach(Athlete athlete in Services.AILifeCycleManager.Athletes){
                if(athlete.ready == false){
                    Context.ready = false;
                    break;
                }
            }
            if(Context.ready){
                Context.gameStateMachine.TransitionTo<InGame>();
            }
        }

    }
    private class GameOver : GameState{
        public override void OnEnter(){
            Context.gameOver.SetActive(true);
            Context.game.SetActive(false);
        }
        public override void Update(){
            if(Input.anyKeyDown){
                SceneManager.LoadScene(0);
            }
        }
        public override void OnExit(){
            Context.gameOver.SetActive(false);
        }
    }
}

