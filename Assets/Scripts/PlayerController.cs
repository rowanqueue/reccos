using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : Athlete
{
    // Update is called once per frame
    void Update()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        targetVelocity = new Vector2(horizontal,vertical);
    }
}
