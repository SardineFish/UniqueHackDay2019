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
    [Header("Horizontal")]
    public int XDirection = 1;
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
    [Header("StickOnWall")]
    public float StickOnWallGravity = 30;
    public float StickOnWallMaxSpeed = 10;
    private bool lastMainKey = false;
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
            var collider = GetComponentInChildren<Collider2D>();
            return collider.bounds.min.y;
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
                        break;
                    }
                    if(HorizontalIsReflect(collision))
                    {
                        ChangeState(StickOnWallState());
                        stickOnWallCollider = contact.collider;
                        break;
                    }
                }
            }
            else if(StateName == "Ground")
            {
                foreach(var contact in collision.contacts)
                {
                    bool digInGround = FootY - contact.point.y >= -0.01f;
                    bool normalHorizontal = Mathf.Approximately(contact.normal.y, 0);
                    if (digInGround && normalHorizontal)
                    {
                        rigidbody.position += Vector2.up * (FootY - contact.point.y + 0.01f);
                    }
                    if (HorizontalIsReflect(collision))
                    {
                        XDirection = -XDirection;
                    }
                }

            }
            else if(StateName == "StickOnWall")
            {
                foreach(var contact in collision.contacts)
                {
                    if (FootY - contact.point.y >= 0f && Mathf.Approximately(contact.normal.x, 0))
                    {
                        if(Velocity.y < 0)
                        {
                            XDirection = -XDirection;
                        }
                        ChangeState(GroundState());
                        onGroundCollider = contact.collider;
                        break;
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

            if(StateName == "StickOnWall" && collision.collider == stickOnWallCollider)
            {
                stickOnWallCollider = null;
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
    private bool HorizontalIsReflect(Collision2D collision)
    {
        var mapItemType = collision.collider.GetComponent<MapItemType>();
        if(mapItemType != null)
        {
            foreach(var contact in collision.contacts)
            {
                if(Mathf.Sign(contact.normal.x) * XDirection == -1 && contact.point.y - FootY > 0.01)
                {
                    var type = mapItemType.GetTypeFromContact(contact);
                    if(type == MapItemType.TypeEnum.StoneWall || type == MapItemType.TypeEnum.MapBorder)
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }
    public void StartJump()
    {
        if(JumpState != null)
        {
            StopCoroutine(JumpState);
        }
        JumpState = Jump();
        StartCoroutine(JumpState);
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
                yield break;
            }
            if(timeCount < JumpTMax && timeCount >= JumpTMin)
            {
                Velocity.y += JumpHoldAcc * Time.fixedDeltaTime;
            }
            else if(timeCount >= JumpTMax) 
            {
                yield break;
            }

        }
    }
    private void XSpeedProcess()
    {
        float xSign = Mathf.Sign(Velocity.x);
        if(xSign * XDirection == -1)
        {
            Velocity.x = 0;
        }
        Velocity += XDirection * Vector2.right * XAcc * Time.fixedDeltaTime;
        if(Mathf.Abs(Velocity.x) > XMaxSpeed)
        {
            Velocity.x = Mathf.Sign(Velocity.x) * XMaxSpeed;
        }
    }
    private Collider2D onGroundCollider = null;
    public IEnumerator GroundState()
    {
        StateName = "Ground";
        while(true)
        {
            if(!lastMainKey && InputUtils.GetMainKey())
            {
                StartJump();
            }
            lastMainKey = InputUtils.GetMainKey();
            if(Velocity.y < 0)
            {
                Velocity.y = 0;
            }
            XSpeedProcess();
            yield return new WaitForFixedUpdate();
        }
    }
    private Collider2D stickOnWallCollider = null;
    public IEnumerator StickOnWallState()
    {
        StateName = "StickOnWall";
        if(JumpState != null)
        {
            StopCoroutine(JumpState);
            JumpState = null;
        }
        Velocity.x = 0;
        while(true)
        {
            Velocity.y -= StickOnWallGravity * Time.fixedDeltaTime;
            Velocity.y = Mathf.Max(-StickOnWallMaxSpeed, Velocity.y);
            
            if(!lastMainKey && InputUtils.GetMainKey())
            {
                XDirection = -XDirection;
                Velocity.x = XDirection * XMaxSpeed;
                StartJump();
                yield break;
            }
            lastMainKey = InputUtils.GetMainKey();
            
            yield return new WaitForFixedUpdate();
        }
        
    }
    public IEnumerator AirState()
    {
        StateName = "Air";
        while(true)
        {
            yield return new WaitForFixedUpdate();
            var gravity = (Velocity.y >= 0 ? JumpUpGravityModulus : JumpDownGravityModulus) * BaseGravity;
            Velocity.y -=  gravity * Time.fixedDeltaTime;
            XSpeedProcess();
        }
    }
}
