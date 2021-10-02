using UnityEngine;

public class PlayerController : MonoBehaviour
{    
    private Rigidbody rigidBody;
    public float Speed = 1.0f;

    public void Start()
    {
        rigidBody = GetComponent<Rigidbody>();
    }

    public void FixedUpdate()
    {
        float leftRightMovement = Input.GetAxis("Horizontal");
        float forwardBackwardMovement = Input.GetAxis("Vertical");

        Vector3 movement = new Vector3(leftRightMovement, 0.0f, forwardBackwardMovement);
        rigidBody.AddForce(movement * Speed);
    }
}
