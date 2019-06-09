using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class SceneDepth: MonoBehaviour
{
    public Vector2 InitialPosition;
    public bool UsePosZ = true;
    public float Depth = 0;

    private void Reset()
    {
        InitialPosition = transform.position;
    }

    private void OnEnable()
    {
    }

    private void Update()
    {
        if (UsePosZ)
            Depth = transform.position.z;
        var camera = GameObject.FindGameObjectWithTag("MainCamera");
        var cameraDelta = camera.GetComponent<DepthOfField>().ScaledDelta;
        var factor = camera.GetComponent<DepthOfField>().DepthFactor;
        transform.position = (cameraDelta * (Depth * factor) + InitialPosition).ToVector3(transform.position.z);
    }
}