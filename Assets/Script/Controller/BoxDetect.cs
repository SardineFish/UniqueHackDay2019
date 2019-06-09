using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class BoxDetect : MonoBehaviour
{
    private BoxCollider2D box;
    public float skinWidth = 0.001f;
    public LayerMask layer;
    public class DetectResult
    {
        public Vector2 normal;
        public Collider2D collider;

        public DetectResult(Vector2 normal, Collider2D collider)
        {
            this.normal = normal;
            this.collider = collider;
        }
    }
    public void Awake()
    {
        box = GetComponent<BoxCollider2D>();
    }

    private float boundDistance(Collider2D collider)
    {
        return (collider.bounds.center - box.bounds.center).magnitude;
    }
    public DetectResult DetectByDirection(Vector2 direction)
    {
        Vector2 normal = new Vector2(direction.y, -direction.x); //顺时针90
        Vector2 edgeCenter = new Vector2(box.bounds.center.x, box.bounds.center.y) + new Vector2(direction.x * box.bounds.extents.x, direction.y * box.bounds.extents.y);
        Vector2 normalDir = new Vector2(normal.x * (box.bounds.extents.x - 2 * skinWidth), normal.y * (box.bounds.extents.y - 2 * skinWidth));
        Vector2 left = edgeCenter - normalDir;
        Vector2 right = edgeCenter + normalDir;

        Vector2 leftStart = left - direction * skinWidth;
        Vector2 leftEnd = left + direction * skinWidth;
        Debug.DrawLine(leftStart, leftEnd, Color.yellow);

        Vector2 rightStart = right - direction * skinWidth;
        Vector2 rightEnd = right + direction * skinWidth;
        Debug.DrawLine(rightStart, rightEnd, Color.yellow);

        DetectResult r = null;

        var leftR = Physics2D.Raycast(leftStart, direction, skinWidth * 2, layer);
        if(leftR)
        {
            r = new DetectResult(leftR.normal, leftR.collider);
        }

        var rightR = Physics2D.Raycast(rightStart, direction, skinWidth * 2, layer);
        if(rightR)
        {

            if(r == null || boundDistance(r.collider) > boundDistance(rightR.collider))
            {
                r = new DetectResult(rightR.normal, rightR.collider);
            }
        }
        return r; 
    }

}
