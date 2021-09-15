using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Box : MonoBehaviour
{
    private BoxData boxData = new BoxData();
    private BoxCollider2D boxCollider;
    private Vector2[] extents = new Vector2[4];

    private void Awake()
    {
        boxCollider = GetComponent<BoxCollider2D>();

        extents[0] = new Vector2(boxCollider.size.x, boxCollider.size.y) * 0.5f * transform.localScale;
        extents[1] = new Vector2(-boxCollider.size.x, boxCollider.size.y) * 0.5f * transform.localScale;
        extents[2] = new Vector2(-boxCollider.size.x, -boxCollider.size.y) * 0.5f * transform.localScale;
        extents[3] = new Vector2(boxCollider.size.x, -boxCollider.size.y) * 0.5f * transform.localScale;
    }

    public Vector2[] GetExtents()
    {
        return extents;
    }

    public BoxData GetBoxData()
    {
        boxData.center = SpaceConverter.WorldToTextureSpace(transform.position);
        boxData.extents = boxCollider.size * transform.localScale * 0.5f * SpaceConverter.WorldToTextureScaleFactor();
        return boxData;
    }
}
