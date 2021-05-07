using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveGird : MonoBehaviour
{
    [Header("使用圖片的一半 做為網格大小")]
    public bool useSpriteHalfSize;

    [Header("自訂網格的寬")]
    [Range(0f, 10f)]
    public float width;

    [Header("自訂網格的寬")]
    [Range(0f, 10f)]
    public float height;

    private void Start()
    {
        if (useSpriteHalfSize)
        {
            var spr = GetComponent<SpriteRenderer>();
            float x = spr.bounds.size.x / 2;
            float y = spr.bounds.size.y / 2;
            SetGird(x, y);
        }
    }

    private void OnMouseUp()
    {
        SetPosForGird();
    }

    public void SetGird(float width, float height)
    {
        this.width = width;
        this.height = height;
    }

    [ContextMenu("Set Pos For Gird")]
    public void SetPosForGird()
    {
        float x = Mathf.Round(transform.position.x / width);
        float y = Mathf.Round(transform.position.y / height);

        Debug.Log(x + " / " + y);

        var pos = transform.position;
        pos.x = x * width;
        pos.y = y * height;
        transform.position = pos;
    }

#if UNITY_EDITOR
    [ContextMenu("Log bound")]
    private void BoundsLog()
    {
        var spr = GetComponent<SpriteRenderer>();
        Debug.Log(spr.bounds.size.x + " / " + spr.bounds.size.y);
    }
#endif

    private void OnDrawGizmosSelected()
    {
        if (useSpriteHalfSize)
        {
            Gizmos.color = Color.green;
            var pos = Vector3.zero;
            pos.x += width;
            Gizmos.DrawLine(Vector3.zero, pos);

            pos = Vector3.zero;
            pos.y += height;
            Gizmos.DrawLine(Vector3.zero, pos);
        }
        else
        {
            Gizmos.color = Color.black;
            var pos = transform.position;
            pos.x += width;
            Gizmos.DrawLine(transform.position, pos);

            pos = transform.position;
            pos.y += height;
            Gizmos.DrawLine(transform.position, pos);
        }

    }
}
