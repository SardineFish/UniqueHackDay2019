using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform Target;

    public float SmoothTime;

    public float MaxSpeed;

    private readonly float zOffset = -100;
    private Vector2 currentVelocity;

    private void Start()
    {
        if (Target != null)
        {
            var position = Target.position;
            position.z = zOffset;
            position.x = transform.position.x;
            transform.position = position;
        }

        currentVelocity = Vector2.zero;
    }

    private void Update()
    {
        if (Target != null)
        {
            SmoothFollowTarget();
        }
    }

    private void SmoothFollowTarget()
    {
        Vector2 currentPosition = transform.position;
        Vector2 targetPosition = Target.position;

        Vector3 position = Vector2.SmoothDamp(currentPosition, targetPosition,
            ref currentVelocity, SmoothTime, MaxSpeed);
        position.z = zOffset;
        position.x = currentPosition.x;

        transform.position = position;
    }

}