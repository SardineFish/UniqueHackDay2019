using System;
using System.Collections;
using System.Text;
using System.Collections.Generic;
using UnityEngine;


public static class InputUtils
{
    public static bool GetMainKey()
    {
        return Input.GetKey(KeyCode.Mouse0) || Input.GetKey(KeyCode.Space);
    }
    public static bool GetMainKeyDown()
    {
        return Input.GetKeyDown(KeyCode.Mouse0) || Input.GetKeyDown(KeyCode.Space);
    }
}


[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    public BoxDetect boxDetect;
    public Animator animator;
    public Vector2 Velocity = new Vector2(1, 0);
    public AudioClip TransportClip;
    [Header("Horizontal")]
    public int XDirection = 1;
    public float XMaxSpeed = 2;
    public float XAcc = 4;
    [Header("Jump")]
    public AudioClip JumpClip;
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
    [Header("Grass")]
    public Vector2 outGrassVelocity = new Vector2(7, 10);
    private IEnumerator JumpState;
    private event Action<Collision2D> CollisionEnterEvent;
    private IEnumerator currentState = null;
    [ReadOnly]
    public string StateName = "Null";
    private Queue<bool> inputBuffer = new Queue<bool>();
    private int torchSkipEnterTime = 0;
    public void ChangeState(IEnumerator state)
    {
        if(currentState != null)
        {
            StopCoroutine(currentState);
        }
        StartCoroutine(state);
        currentState = state;
    }
    public void PlaySound(AudioClip clip)
    {
        var audioSource = GetComponentInChildren<AudioSource>();
        audioSource.clip = clip;
        audioSource.Play();
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
    private event Action<Collider2D> TriggerEnterEvent;
    public void OnTriggerEnter2D(Collider2D collider)
    {
        TriggerEnterEvent?.Invoke(collider);
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

        CollisionStayEvent += (Collision2D collision) =>
        {
            collision.collider.GetComponent<MapItem>()?.OnPlayerTouch();
            foreach(var contact in collision.contacts)
            {
                Debug.DrawLine(contact.point, contact.point + contact.normal, Color.red);
                if(StateName == "Ground" &&  FootY - contact.point.y >= -0.01f && Mathf.Approximately(contact.normal.y, 0))
                {
                    rigidbody.position += Vector2.up * (FootY - contact.point.y + 0.01f);
                    break;
                }
            }
        };
        TriggerEnterEvent += (Collider2D collider) => {
            collider.GetComponent<MapItem>()?.OnPlayerTouch();
            var grass = collider.GetComponent<GrassItem>();
            if(grass != null && !grass.burnt)
            {
                ChangeState(InGrassState(grass));
                return;
            }
            var torch = collider.GetComponent<Torch>();
            if(torch != null)
            {
                if(torchSkipEnterTime > 0)
                {
                    torchSkipEnterTime--;
                }
                else
                {
                    ChangeState(TorchState(torch));
                    return;
                }
            }
            var gamePassTorch = collider.GetComponent<GamePassTorch>();
            if(gamePassTorch != null)
            {
                ChangeState(GamePassTorchState(gamePassTorch));
            }
        };
        ChangeState(AirState());
        while(true)
        {
            rigidbody.velocity = Velocity;
            {
                var detectR = boxDetect.DetectByDirection(Vector2.down);
                if(detectR == null && StateName == "Ground")
                {
                    ChangeState(AirState());
                }
                else if(detectR != null)
                {
                    if(StateName == "Air")
                    {
                        ChangeState(GroundState());
                    }
                    else if(StateName == "StickOnWall")
                    {
                        if(Velocity.y < 0)
                        {
                            XDirection = -XDirection;
                        }
                        ChangeState(GroundState());
                    }
                }
            }

            {
                var detectR = boxDetect.DetectByDirection(new Vector2(XDirection, 0));
                if(detectR != null)
                {
                    if(StateName == "Air")
                    {
                        ChangeState(StickOnWallState());
                    }
                    else if(StateName == "Ground")
                    {
                        XDirection = -XDirection;
                    }
                }
                else if(detectR == null && StateName == "StickOnWall")
                {
                    ChangeState(AirState());
                }
            }

            {
                var detectR = boxDetect.DetectByDirection(Vector2.up);
                if(detectR != null && StateName == "Air")
                {
                    if(JumpState != null)
                    {
                        StopCoroutine(JumpState);
                        JumpState = null;
                    }
                    Velocity.y = 0;
                }
            }
            yield return new WaitForFixedUpdate();
        }
    }
    public void Update()
    {
        inputBuffer.Enqueue(InputUtils.GetMainKeyDown());
        while(inputBuffer.Count > 5)
        {
            inputBuffer.Dequeue();
        }
    }
    public bool RecentlyMainKeyDown()
    {
        foreach(bool down in inputBuffer)
        {
            if(down)
            {
                return true;
            }
        }
        return false;
    }
    private string addDirectionSuffix(string prefix)
    {
        string suffix = XDirection < 0 ? "Left" : "Right";
        StringBuilder sb = new StringBuilder();
        sb.Append(prefix);
        sb.Append(suffix);
        return sb.ToString();
    }
    private bool HorizontalIsReflect(Collision2D collision)
    {
        var mapItemType = collision.collider.GetComponent<MapItemType>();
        if(mapItemType != null)
        {
            foreach(var contact in collision.contacts)
            {
                if(Mathf.Sign(contact.normal.x) * XDirection == -1 && contact.point.y - FootY > 0.01 && Mathf.Approximately(contact.normal.y, 0))
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
        PlaySound(JumpClip);
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
    public IEnumerator GroundState()
    {
        if(StateName == "Air")
        {
            animator.Play(addDirectionSuffix("FallToGround"));
        }
        StateName = "Ground";
        while(true)
        {
            if(RecentlyMainKeyDown())
            {
                StartJump();
            }
            if(Velocity.y < 0)
            {
                Velocity.y = 0;
            }
            XSpeedProcess();
            yield return new WaitForFixedUpdate();
            var clipInfo = animator.GetCurrentAnimatorClipInfo(0)[0];
            var clipName = clipInfo.clip.name;
            if(clipName != addDirectionSuffix("FallToGround"))
            {
                animator.Play(addDirectionSuffix("Move"));
            }
        }
    }
    public IEnumerator StickOnWallState()
    {
        StateName = "StickOnWall";
        if(JumpState != null)
        {
            StopCoroutine(JumpState);
            JumpState = null;
        }
        animator.Play(addDirectionSuffix("StickOnWall"));
        Velocity.x = 0;
        while(true)
        {
            Velocity.y -= StickOnWallGravity * Time.fixedDeltaTime;
            Velocity.y = Mathf.Max(-StickOnWallMaxSpeed, Velocity.y);
            
            if(RecentlyMainKeyDown())
            {
                XDirection = -XDirection;
                Velocity.x = XDirection * XMaxSpeed;
                StartJump();
                yield break;
            }
            
            yield return new WaitForFixedUpdate();
        }
        
    }
    public IEnumerator AirState()
    {
        if(StateName == "StickOnWall")
        {
            animator.Play(addDirectionSuffix("WallStartJump"));
        }
        StateName = "Air";
        while(true)
        {
            yield return new WaitForFixedUpdate();
            if(Velocity.y > 0)
            {
                if (animator.GetCurrentAnimatorClipInfo(0)[0].clip.name != addDirectionSuffix("WallStartJump")) {
                    animator.Play(addDirectionSuffix("JumpUp"));
                }
            }
            else
            {
                animator.Play(addDirectionSuffix("JumpDown"));
            }
            var gravity = (Velocity.y >= 0 ? JumpUpGravityModulus : JumpDownGravityModulus) * BaseGravity;
            Velocity.y -=  gravity * Time.fixedDeltaTime;
            XSpeedProcess();
        }
    }
    private string inGrassAnimationName(Transform trans)
    {

        StringBuilder stringBuilder = new StringBuilder();
        stringBuilder.Append("InGrass");

        var dir3 = trans.position - transform.position;
        var dir = new Vector2(dir3.x, dir3.y);

        bool horizontal = Mathf.Abs(dir.x) < Mathf.Abs(dir.y);

        stringBuilder.Append(horizontal ? "Horizontal" : "Vertical");

        return stringBuilder.ToString();
    }
    public IEnumerator InGrassState(GrassItem grass)
    {
        StateName = "InGrass";

        var rigidbody = GetComponent<Rigidbody2D>();
        animator.Play(addDirectionSuffix(inGrassAnimationName(grass.transform)));

        Velocity = Vector2.zero;
        Vector2 finalPos = Vector2.zero;
        var grassProgress = grass.GrassProcess((pos) => { finalPos = pos; });
        StartCoroutine(grassProgress);
        yield return grassProgress;

        finalPos += Vector2.up;

        float xDir = Mathf.Sign((finalPos - rigidbody.position).x);
        XDirection = (int)xDir;
        rigidbody.position = finalPos;
        Velocity.x = xDir * outGrassVelocity.x;
        Velocity.y = outGrassVelocity.y;
        PlaySound(TransportClip);
        ChangeState(AirState());
        
    }
    public IEnumerator TorchState(Torch torch)
    {
        StateName = "Torch";
        torchSkipEnterTime++;
        animator.Play(addDirectionSuffix(inGrassAnimationName(torch.transform)));
        var rigidbody = GetComponent<Rigidbody2D>();
        var lastVelocity = Velocity;
        Velocity = Vector2.zero;
        yield return new WaitForSeconds(0.5f);
        rigidbody.position = torch.Opposite.transform.position;
        Velocity = lastVelocity;
        PlaySound(TransportClip);
        ChangeState(AirState());
    }
    public IEnumerator GamePassTorchState(GamePassTorch gamePassTorch)
    {
        StateName = "GamePassTorch";
        animator.Play(addDirectionSuffix(inGrassAnimationName(gamePassTorch.transform)));
        Velocity = Vector2.zero;
        while(true)
        {
            yield return new WaitForFixedUpdate();
        }
    }
}

