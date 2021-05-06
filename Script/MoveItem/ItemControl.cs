using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class ItemControl : MonoBehaviour
{
    protected Vector3 v3_OriginalPos;
    /// <summary> 拖拉初始Pos </summary>
    protected Vector3 v3_DragStartPos;
    /// <summary> 拖拉終止Pos </summary>
    protected Vector3 v3_DragEndPos;
    /// <summary> 跟Mouse的Offset </summary>
    protected Vector3 v3_offsetToMouse;

    [Header("對答案用")]
    /// <summary> 對答案用 </summary>
    public int index;
    [Header("對答案用")]
    public Transform m_MoveArea;

    public float offsetX;
    public float offsetY;

    protected GameObject m_ColliderObj;

    /// <summary> 磁吸功能 </summary>
    [Header("磁吸功能")]
    public bool m_IsMagnetic;

    /// <summary> 無限拖拉 </summary>
    [Header("無限拖拉")]
    public bool m_IsCloneByDrag;

    /// <summary> 超過視窗回到上一步，false = 刪除自己 </summary>
    [Header("超過視窗 退到上一步/刪除自己")]
    public bool m_OutWindowBackToLastPos;

    /// <summary> 關閉退回原位置 </summary>
    [Header("關閉退回原位置")]
    public bool m_IsCloseBackToOriPos;


    /// <summary> 碰撞到的GameObject </summary>
    protected List<GameObject> enterColliders = new List<GameObject>();

    /// <summary> 是否是被複製出來的 </summary>
    protected bool m_isCloen = false;

    /// <summary> 是否在垃圾桶 </summary>
    protected bool needRemove;

    #region OnMouse
    private void OnMouseDown()
    {
        v3_DragStartPos = transform.position;
        v3_offsetToMouse = v3_DragStartPos - Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, transform.position.z));
        GetComponent<SpriteRenderer>().sortingOrder = 10;
    }

    private void OnMouseDrag()
    {
        transform.position = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, transform.position.z)) + v3_offsetToMouse;

        //複製
        float moveDis = Vector3.Distance(v3_DragStartPos, transform.position);
        if (!m_isCloen && m_IsCloneByDrag && moveDis > 0)
        {
            m_isCloen = true;
            GameObject go = Instantiate(this.gameObject);
            go.name = this.gameObject.name;
            go.transform.position = v3_OriginalPos;
            go.GetComponent<SpriteRenderer>().sortingOrder = 3;
        }
    }


    private void OnMouseUp()
    {
        GetComponent<SpriteRenderer>().sortingOrder = 3;
        v3_DragEndPos = transform.position;

        var colliderCenter = GetComponent<Collider2D>().bounds.center;
        var posX = Camera.main.WorldToScreenPoint(colliderCenter).x;
        var posY = Camera.main.WorldToScreenPoint(colliderCenter).y;

        float moveDis = Vector3.Distance(v3_DragStartPos, v3_DragEndPos);

        if (moveDis < 0.2f)
        {
            if (m_IsCloneByDrag)
            {
                if (this.transform.position != v3_OriginalPos)
                    Destroy(this.gameObject);
            }
            else
            //移動不夠回到原位
            if (!m_IsCloseBackToOriPos)
            {
                Back2OriPos();
            }
        }
        else if (posX < 0 || posY < 0 || posY > Screen.height || posX > Screen.width)
        {
            if (m_IsCloneByDrag)
            {
                if (this.transform.position != v3_OriginalPos)
                    Destroy(this.gameObject);
            }
            else
            {
                if (m_OutWindowBackToLastPos)
                {
                    Debug.Log("Back to last time.");
                    this.transform.position = v3_DragStartPos;
                }
                else
                    Back2OriPos();
            }
        }
        else
        {
            if (needRemove)
            {
                Destroy(this.gameObject);
                return;
            }

            if (enterColliders.Count != 0)
            {
                float lastdis = float.MaxValue;
                for (int i = 0; i < enterColliders.Count; i++)
                {
                    if (enterColliders[i].transform.childCount != 0 && enterColliders[i].transform.GetChild(0).gameObject != this.gameObject)
                    {
                        Debug.LogFormat("有子物件不比對{0}", enterColliders[i].name);
                    }
                    else
                    {
                        float dis = Vector2.Distance(this.transform.position, enterColliders[i].transform.position);
                        if (dis < lastdis)
                        {
                            lastdis = dis;
                            m_ColliderObj = enterColliders[i];
                        }
                    }
                }

                if (!m_IsMagnetic)
                {
                    if (m_ColliderObj == null)
                    {
                        Debug.Log("剛拖曳出來 無法進入感應區");
                        if (m_IsCloneByDrag)
                            Destroy(this.gameObject);
                        else
                            Back2OriPos();
                    }
                    else
                    {
                        Debug.LogFormat("吸附到 {0}", m_ColliderObj.name);
                        transform.position = new Vector3(this.m_ColliderObj.transform.position.x + offsetX, this.m_ColliderObj.transform.position.y + offsetY, -2);
                        transform.SetParent(m_ColliderObj.transform);
                    }
                }
                else
                {
                    //移動夠多，如果有碰到感應區，則移動到感應區位置
                    this.transform.SetParent(m_ColliderObj.transform);
                }
            }
            else
            {
                Debug.Log("沒有在感應區");

                m_ColliderObj = null;
                this.transform.SetParent(m_MoveArea);
                GetComponent<SpriteRenderer>().sortingOrder = 4;
            }
        }
    }
    #endregion

    protected void Back2OriPos()
    {
        this.transform.SetParent(null);
        this.transform.position = v3_OriginalPos;
    }
}
