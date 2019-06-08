using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    public Vector2 Velocity = new Vector2(1, 0);
    public bool OnGround = false;
    private Collider2D onGroundCollider = null;
    [Header("Idle")]
    public float IdleXSpeed = 2;
    public float IdleGravity = 3;
    public float IdleJumpYSpeed = 4;
    private event Action<Collision2D> CollisionEnterEvent;
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
            var mapItemType = collision.collider.GetComponent<MapItemType>();
            if(mapItemType != null)
            {
                bool reflectX = false;
                foreach(var contact in collision.contacts)
                {
                    var type = mapItemType.GetTypeFromContact(contact);
                    if(type == MapItemType.TypeEnum.StoneWall || type == MapItemType.TypeEnum.MapBorder)
                    {
                        reflectX = true;
                        break;
                    }
                }
                if(reflectX)
                {
                    Velocity.x = -Velocity.x;
                }
            }

        };
        CollisionStayEvent += (Collision2D collision) =>
        {
            foreach(var contact in collision.contacts)
            {
                Debug.DrawLine(contact.point, contact.point + contact.normal, Color.red);
            }
            if(!OnGround)
            {
                foreach(var contact in collision.contacts)
                {
                    if (Mathf.Abs(contact.point.y - FootY) < 0.01)
                    {
                        OnGround = true;
                        onGroundCollider = contact.collider;
                    }
                }
            }
        };
        CollisionExitEvent += (Collision2D collision) =>
        {
            if(OnGround && collision.collider == onGroundCollider)
            {
                OnGround = false;
                onGroundCollider = null;
            }
        };
        var idleState = Idle();
        StartCoroutine(idleState);
        while(true)
        {
            rigidbody.velocity = Velocity;
            yield return new WaitForFixedUpdate();
        }
    }
    public IEnumerator Idle()
    {
        while(true)
        {
            if(OnGround)
            {
                if(Input.GetKey(KeyCode.Mouse0) || Input.GetKey(KeyCode.Space))
                {
                    Velocity.y = IdleJumpYSpeed;
                }
                else
                {
                    Velocity.y = 0;
                }
            }
            else
            {
                Velocity.y -= IdleGravity * Time.fixedDeltaTime;
            }
            Velocity.x = Mathf.Sign(Velocity.x) * IdleXSpeed;
            yield return new WaitForFixedUpdate();
        }
    }
}
