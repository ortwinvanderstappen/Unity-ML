using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class Platform : MonoBehaviour
{
    private BoxCollider _boxCollider;

    void Awake()
    {
        _boxCollider = GetComponent<BoxCollider>();
    }

    public bool IsPositionAbovePlatform(Vector3 pos)
    {
        Bounds b = _boxCollider.bounds;
        Vector3 extents = b.extents;

        const float errorMargin = .1f;

        Vector3 topLeft = new Vector3(pos.x - extents.x + errorMargin, pos.y, pos.z + extents.z - errorMargin);
        Vector3 topRight = new Vector3(pos.x + extents.x - errorMargin, pos.y, pos.z + extents.z - errorMargin);
        Vector3 bottomLeft = new Vector3(pos.x - extents.x + errorMargin, pos.y, pos.z - extents.z + errorMargin);
        Vector3 bottomRight = new Vector3(pos.x + extents.x - errorMargin, pos.y, pos.z - extents.z + errorMargin);

        Ray rayTopleft = new Ray(topLeft, Vector3.down);
        Ray rayTopRight = new Ray(topRight, Vector3.down);
        Ray rayBottomLeft = new Ray(bottomLeft, Vector3.down);
        Ray rayBottomRight = new Ray(bottomRight, Vector3.down);

        //Debug.DrawRay(topLeft, Vector3.down, Color.red);
        //Debug.DrawRay(topRight, Vector3.down, Color.red);
        //Debug.DrawRay(bottomLeft, Vector3.down, Color.red);
        //Debug.DrawRay(bottomRight, Vector3.down, Color.red);

        return b.IntersectRay(rayTopleft) || b.IntersectRay(rayTopRight)
            || b.IntersectRay(rayBottomLeft) || b.IntersectRay(rayBottomRight);
    }
}
