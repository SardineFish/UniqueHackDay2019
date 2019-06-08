using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    public Vector2 Velocity = new Vector2(1, 0);
    private Collider2D onGroundCollider = null;
    [Header("Idle")]
    public float IdleXSpeed = 2;
    public float IdleGravity = 3;
    public float IdleJumpYSpeed = 4;
    private event Action<Collision2D> CollisionEnterEvent;
    private IEnumerator currentState = null;
    public string StateName = "Null";
    public void ChangeState(IEnumerator state)
    {
        if(currentState != null)
        {
            StopCoroutine(currentState);
        }
        StartCoroutine(state);
        currentState = state;
    }
    public void OnCollisionEnter2D(Collision2D collision)
    {
        CollisionEnterEvent?.Invoke(collision);
    }
    private event Action<Collision2D> CollisionStayEvent;
    public void OnCollisionStay2D(Collision2D collision)
    {
        CollisionStayEvent?.Invoke(collision);
    }
    private event Action<Collision2D> CollisionExitEvent;
    public void OnCollisionExit2D(Collision2D collision)
    {
        CollisionExitEvent?.Invoke(collision);
    }
    private float FootY
    {
        get
        {
            var box = GetComponentInChildren<BoxCollider2D>();
            return box.bounds.min.y;
        }
    }
    public IEnumerator Main()
    {
        Rigidbody2D rigidbody = GetComponent<Rigidbody2D>();

        CollisionEnterEvent += (Collision2D collision) =>
        {

        };
        CollisionStayEvent += (Collision2D collision) =>
        {
            var mapItemType = collision.collider.GetComponent<MapItemType>();
            if(mapItemType != null)
            {
                bool reflectX = false;
                foreach(var contact in collision.contacts)
                {
                    if(Mathf.Sign(contact.normal.x) * Mathf.Sign(Velocity.x) == -1)
                    {
                        var type = mapItemType.GetTypeFromContact(contact);
                        if(type == MapItemType.TypeEnum.StoneWall || type == MapItemType.TypeEnum.MapBorder)
                        {
                            reflectX = true;
                            break;
                        }
                    }
                }
                if(reflectX)
                {
                    Velocity.x = -Velocity.x;
                }
            }
            foreach(var contact in collision.contacts)
            {
                Debug.DrawLine(contact.point, contact.point + contact.normal, Color.red);
            }
            if(StateName == "Air")
            {
                foreach(var contact in collision.contacts)
                {
                    if (FootY - contact.point.y >= 0f && Mathf.Approximately(contact.normal.x, 0))
                    {
                        ChangeState(Ground());
                        onGroundCollider = contact.collider;
                    }
                }
            }
            else if(StateName == "Ground")
            {
                foreach(var contact in collision.contacts)
                {
                    if (FootY - contact.point.y >= 0f && Mathf.Approximately(contact.normal.y, 0))
                    {
                        rigidbody.position += Vector2.up * (FootY - contact.point.y + 0.01f);
                    }
                }

            }
        };
        CollisionExitEvent += (Collision2D collision) =>
        {
            if(StateName == "Ground" && collision.collider == onGroundCollider)
            {
                onGroundCollider = null;
                ChangeState(Air());
            }
        };
        ChangeState(Air());
        while(true)
        {
            rigidbody.velocity = Velocity;
            yield return new WaitForFixedUpdate();
        }
    }
    public IEnumerator Ground()
    {
        StateName = "Ground";
        while(true)
        {
            if(Input.GetKey(KeyCode.Mouse0) || Input.GetKey(KeyCode.Space))
            {
                Velocity.y = IdleJumpYSpeed;
            }
            else
            {
                Velocity.y = 0;
            }
            Velocity.x = Mathf.Sign(Velocity.x) * IdleXSpeed;
            yield return new WaitForFixedUpdate();
        }
    }
    public IEnumerator Air()
    {
        StateName = "Air";
        while(true)
        {
            Velocity.y -= IdleGravity * Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }
    }
}
