using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class MoveItem : ItemControl
{
    public int score;

    public EcolliderType ecolliderType;
    public enum EcolliderType
    {
        Box,
        Circle,
        Polygon,
    }

    void Start()
    {
        v3_OriginalPos = this.transform.position;
        if (m_MoveArea == null)
        {
            if (GameObject.Find("MoveArea"))
            {
                m_MoveArea = GameObject.Find("MoveArea").transform;
            }
            else
            {
                var go = new GameObject("MoveArea");
                m_MoveArea = go.transform;
            }
        }

        if (GetComponent<Collider2D>() == null)
        {
            if (ecolliderType == EcolliderType.Box)
            {
                BoxCollider2D BCollider = this.gameObject.AddComponent<BoxCollider2D>();
                BCollider.isTrigger = true;
                Vector2 OriColliderSize = BCollider.size;
                BCollider.size = OriColliderSize * 0.8f;
            }
            else if (ecolliderType == EcolliderType.Polygon)
            {
                PolygonCollider2D collider = this.gameObject.AddComponent<PolygonCollider2D>();
                collider.isTrigger = true;
            }
            else
            {
                CircleCollider2D collider = this.gameObject.AddComponent<CircleCollider2D>();
                collider.isTrigger = true;
            }
        }
    }


    #region OnTrigger
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "matchPosItem")
        {
            //Debug.Log("Enter " + collision.name);
            enterColliders.Add(collision.gameObject);
        }
        else if (collision.tag == "removeItem")
        {
            needRemove = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag == "matchPosItem")
        {
            enterColliders.Remove(collision.gameObject);
        }
        else if (collision.tag == "removeItem")
        {
            needRemove = false;
        }
    }
    #endregion

    /// <summary>
    /// 從外部設定MatchPosObj，只有初始化時才會呼叫
    /// </summary>
    /// <param name="matchPosObj"></param>
    public void SetMatchPosParent(GameObject matchPosObj)
    {
        if (matchPosObj.tag.Equals("matchPosItem"))
        {
            m_ColliderObj = matchPosObj;
            this.transform.SetParent(m_ColliderObj.transform);
            transform.position = new Vector3(this.m_ColliderObj.transform.position.x + offsetX, this.m_ColliderObj.transform.position.y + offsetY, -2);
            GetComponent<SpriteRenderer>().sortingOrder = 2;
        }
    }
}
