using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour
{
    public void OnTriggerEnter2D(Collider2D other){
        if(other.CompareTag("Goal")){
            foreach(Goal goal in Services.GameController.goals){
                if(other == goal.collider){
                    Services.EventManager.Fire(new GoalScored(goal.team));
                    break;
                }
            }
        }
    }
}
