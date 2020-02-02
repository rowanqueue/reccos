using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        InitializeServices();
    }
    void Update(){
        Services.AILifeCycleManager.Update();
    }


    private void InitializeServices(){
        Services.GameController = this;

        Services.AILifeCycleManager = new AILifeCycleManager();
        Services.AILifeCycleManager.Initialize();

        Services.Player = GameObject.FindObjectOfType<PlayerController>();
    }
}
