using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public int dumbAthleteNumber;
    public Bounds spawnArea;
    // Start is called before the first frame update
    void Start()
    {
        InitializeServices();
        for(int i = 0; i < dumbAthleteNumber;i++){
            float x = Random.Range(spawnArea.min.x,spawnArea.max.x);
            float y = Random.Range(spawnArea.min.y,spawnArea.max.y);
            Debug.Log(x);
            Services.AILifeCycleManager.CreateDumb(new Vector2(x,y));
        }
    }
    void Update(){
        Services.AILifeCycleManager.Update();
    }


    private void InitializeServices(){
        Services.GameController = this;

        Services.AILifeCycleManager = new AILifeCycleManager();
        Services.AILifeCycleManager.Initialize();

        Services.Player = GameObject.FindObjectOfType<PlayerController>();

        Services.Ball = GameObject.FindObjectOfType<Ball>();
    }
}
