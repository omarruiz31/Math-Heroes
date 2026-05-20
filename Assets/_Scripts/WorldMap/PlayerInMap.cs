using UnityEngine;

public class playerInMap : MonoBehaviour
{
public float speed = 5f;
private RigidBody2D rb2D;
private Vector2 movementInput;

    void Start()
    {
        rb = GetComponentby<RigidBody2D>();
    }

    void Update()
    {

        movementInput.x = Input.GetAxisRaw("Horizontal");
        movementInput.y = Input.GetAxisRaw("Vertical");
        
        movementInput = movementInput.normalized;

        private void FixedUpdate(){
            rb2D.linearVelocity = movementInput * speed;
        }
    }
}