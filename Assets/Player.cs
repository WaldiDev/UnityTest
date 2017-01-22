using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour
{

    public float speed = 400.0f;
    public float maxSpeed = 20.0f;

    Rigidbody rb;
    
    // Use this for initialization
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        Move();
    }

    void Move()
    {
        Vector3 force = Vector3.zero;
        if (Input.GetKey(KeyCode.W))
        {
            force += transform.forward.normalized * speed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.S))
        {
            force -= transform.forward.normalized * speed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.A))
        {
            force -= transform.right.normalized * speed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.D))
        {
            force += transform.right.normalized * speed * Time.deltaTime;
        }
        rb.AddForce(force);
        if (rb.velocity.magnitude > maxSpeed)
        {
            rb.velocity = rb.velocity.normalized * maxSpeed;
        }
    }

}
