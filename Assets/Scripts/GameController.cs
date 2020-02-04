using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public int numberOfTeams;
    public int numberPerTeam;
    public List<Color> teamColors;
    public List<Goal> goals;
    public List<int> scores;
    public Bounds spawnArea;
    // Start is called before the first frame update
    void Awake()
    {
        InitializeServices();
        for(int j=0;j<numberOfTeams;j++){
            for(int i = 0; i < numberPerTeam;i++){
                float x = Random.Range(spawnArea.min.x,spawnArea.max.x);
                float y = Random.Range(spawnArea.min.y,spawnArea.max.y);
                Services.AILifeCycleManager.CreateDumb(new Vector2(x,y),j);
            }
        }
        
        Services.AILifeCycleManager.Athletes[0].playerControlled = true;
        
    }
    private void Start(){
        goals.Add(new Goal(transform.GetChild(1).gameObject).SetTeam(1));
        goals.Add(new Goal(transform.GetChild(0).gameObject).SetTeam(0));
        //goals.Add(new Goal(transform.GetChild(1).gameObject).SetTeam(1));
        Services.EventManager.Register<GoalScored>(OnGoalScored);
        scores = new List<int>();
        for(int i = 0; i < goals.Count;i++){
            scores.Add(0);
        }
    }
    private void OnDestroy(){
        Services.EventManager.Unregister<GoalScored>(OnGoalScored);
    }
    void OnGoalScored(AGPEvent e){
        var goal = (GoalScored)e;
        scores[goal.whichTeam]++;
    }
    void Update(){
        Services.AILifeCycleManager.Update();
    }


    private void InitializeServices(){
        Services.GameController = this;

        Services.EventManager = new EventManager();

        Services.AILifeCycleManager = new AILifeCycleManager();
        Services.AILifeCycleManager.Initialize();

        Services.Ball = GameObject.FindObjectOfType<Ball>();
    }
}

