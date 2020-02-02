using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Athlete : MonoBehaviour
{
    public float movementSpeed = 5f;
    public float movementSmoothing = 0.05f;
    protected Vector2 targetVelocity;

    private Vector2 velocity = Vector2.zero;
    private Rigidbody2D rb;
    // Start is called before the first frame update
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }
    void FixedUpdate(){
        if(targetVelocity != Vector2.zero){
            Move();
        }

    }
    public void Move(){
        rb.velocity = Vector2.SmoothDamp(rb.velocity, targetVelocity*movementSpeed,ref velocity,movementSmoothing);

    }
}
