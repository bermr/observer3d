using UnityEngine;
using System.Collections;

public class BallController : MonoBehaviour{

    private Rigidbody rb;
    public float speed;

    public void Start (){
        rb = GetComponent<Rigidbody>();
    }

    public void FixedUpdate(){
        //Move();
    }

	public void Move(){
        Vector3 randomPosition = new Vector3(Random.Range(0, 1000),0,Random.Range(0, 1000));
        //rb.AddForce(movement * speed);
        rb.position = randomPosition;
	}
}
