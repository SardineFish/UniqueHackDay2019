using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class InputUtils
{
    public static bool GetMainKey()
    {
        return Input.GetKey(KeyCode.Mouse0) || Input.GetKey(KeyCode.Space);
    }
}

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    public Vector2 Velocity = new Vector2(1, 0);
    private Collider2D onGroundCollider = null;
    [Header("Horizontal")]
    public float XMaxSpeed = 2;
    public float XAcc = 4;
    [Header("Jump")]
    public float BaseGravity = 20;
    public float JumpTMin = 3;
    public float JumpTMax= 4;
    public float JumpSpeed = 8;
    public float JumpHoldAcc = 4;
    public float JumpUpGravityModulus = 10;
    public float JumpDownGravityModulus = 20;
    private IEnumerator JumpState;
    private event Action<Collision2D> CollisionEnterEvent;
    private IEnumerator currentState = null;
    [ReadOnly]
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
            collision.collider.GetComponent<MapItem>()?.OnPlayerTouch();
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
                        ChangeState(GroundState());
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
                ChangeState(AirState());
            }
        };
        ChangeState(AirState());
        while(true)
        {
            rigidbody.velocity = Velocity;
            yield return new WaitForFixedUpdate();
        }
    }
    public IEnumerator Jump()
    {
        float timeCount = 0;
        Velocity.y = JumpSpeed;
        while(true)
        {
            yield return new WaitForFixedUpdate();
            timeCount += Time.fixedDeltaTime;
            if(!InputUtils.GetMainKey())
            {
                JumpState = null;
                yield break;
            }
            if(timeCount < JumpTMax && timeCount >= JumpTMin)
            {
                Velocity.y += JumpHoldAcc * Time.fixedDeltaTime;
            }
            else if(timeCount >= JumpTMax) 
            {
                JumpState = null;
                yield break;
            }

        }
    }
    public IEnumerator GroundState()
    {
        StateName = "Ground";
        while(true)
        {
            if(InputUtils.GetMainKey())
            {
                if(JumpState == null)
                {
                    JumpState = Jump();
                    StartCoroutine(JumpState);
                }
            }
            if(Velocity.y < 0)
            {
                Velocity.y = 0;
            }
            float xSign = Mathf.Sign(Velocity.x);
            if (xSign == 0) xSign = 1;
            Velocity += xSign * Vector2.right * XAcc * Time.fixedDeltaTime;
            if(Mathf.Abs(Velocity.x) > XMaxSpeed)
            {
                Velocity.x = xSign * XMaxSpeed;
            }
            yield return new WaitForFixedUpdate();
        }
    }
    public IEnumerator AirState()
    {
        StateName = "Air";
        while(true)
        {
            var gravity = (Velocity.y >= 0 ? JumpUpGravityModulus : JumpDownGravityModulus) * BaseGravity;
            Velocity.y -=  gravity * Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }
    }
}
