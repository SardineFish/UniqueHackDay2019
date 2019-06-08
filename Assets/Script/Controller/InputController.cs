using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MovableEntity))]
public class InputController : MonoBehaviour
{
    public float InitialDirection = 1;
    [ReadOnly("Current Direction")]
    float CurrentDirection;

    // Start is called before the first frame update
    void Start()
    {
        CurrentDirection = InitialDirection;       
    }

    // Update is called once per frame
    void Update()
    {
        var controller = GetComponent<MovableEntity>();
        controller.Move(Vector2.right * CurrentDirection);
        if(Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.Mouse0))
        {
            DoJump();
        }
        if(Input.touchCount > 0)
        {
            DoJump();
        }
    }

    void DoJump()
    {
        var controller = GetComponent<MovableEntity>();
        controller.Jump();
    }
    
    private void OnCollisionEnter2D(Collision2D collision)
    {
        for (var i = 0; i < collision.contactCount; i++)
        {
            var contract = collision.GetContact(i);
            if (Mathf.Approximately(-1, Vector2.Dot(contract.normal, Vector2.right * CurrentDirection)))
            {
                CurrentDirection = -CurrentDirection;
                return;
            }
        }
    }
}
