using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class ItemControl : MonoBehaviour
{
    #region pos
    protected Vector3 v3_OriginalPos;
    /// <summary> 拖拉初始Pos </summary>
    protected Vector3 v3_DragStartPos;
    /// <summary> 拖拉終止Pos </summary>
    protected Vector3 v3_DragEndPos;
    /// <summary> 跟Mouse的Offset </summary>
    protected Vector3 v3_offsetToMouse;

    /// <summary> 暫時用V3 </summary>
    protected Vector3 tempV3;
    #endregion

    /// <summary> 拖拉移動 </summary>
    [Header("拖拉移動")]
    public bool isDragMove;

    /// <summary> 數值 </summary>
    [Header("數值")]
    public int value;

    [Header("移動區域")]
    public Transform m_MoveArea;

    /// <summary> 校正中心點的位置 </summary>
    [Header("校正中心點的位置")]
    public Vector2 offset;

    /// <summary> 移動限制 </summary>
    [Header("移動限制")]
    public bool isMovementRestriction;

    /// <summary> 移動限制距離 X </summary>
    [Header("移動限制距離 X")]
    public Vector2 moveRangeX;

    /// <summary> 移動限制距離 Y </summary>
    [Header("移動限制距離 Y")]
    public Vector2 moveRangeY;

    protected GameObject m_ColliderObj;

    /// <summary> 磁吸功能 </summary>
    [Header("磁吸功能")]
    public bool m_IsMagnetic;

    /// <summary> 無限拖拉 </summary>
    [Header("無限拖拉")]
    public bool m_IsCloneByDrag;

    /// <summary> 顯示原始透明圖片 </summary>
    [Header("顯示原始透明圖片")]
    public bool m_IsShowTransparentPicture;

    /// <summary> 不在感應區內就退回原位 </summary>
    [Header("不在感應區內就退回原位")]
    public bool m_IsNotPosItemBackToOriPos;

    /// <summary> 超過視窗回到上一步，false = 刪除自己 </summary>
    [Header("超過視窗 退到上一步")]
    public bool m_OutWindowBackToLastPos;

    /// <summary> 移動不夠退回原位 / 點一下退回原位 </summary>
    [Header("移動不夠退回原位 / 點一下退回原位")]
    public bool m_IsBackToOriPos;


    /// <summary> 碰撞到的GameObject </summary>
    protected List<GameObject> enterColliders = new List<GameObject>();

    /// <summary> 是否複製過 </summary>
    protected bool m_isCloen = false;

    /// <summary> 是否在垃圾桶 </summary>
    protected bool needRemove;

    protected bool isMoving;

    private GameObject cloneGo;

    int hightOrder = 10;
    int normalOrder;

    public Action m_SetPosForGridEvent;

    #region OnMouse

    private void Start()
    {
        if (GetComponent<MoveGird>())
            m_SetPosForGridEvent = delegate { GetComponent<MoveGird>().SetPosForGird(); };

        StartInit();
    }

    protected virtual void StartInit()
    {
        v3_OriginalPos = this.transform.position;

        if (m_IsShowTransparentPicture && !m_IsCloneByDrag)
        {
            GameObject go = new GameObject(this.gameObject.name);
            go.transform.position = v3_OriginalPos;
            var sp = go.AddComponent<SpriteRenderer>();
            sp.sprite = this.GetComponent<SpriteRenderer>().sprite;
            sp.sortingOrder = normalOrder;
            var color = sp.color;
            color.a *= 0.5f;
            sp.color = color;
        }
    }

    private void OnMouseDown()
    {
        if (!isMoving)
        {
            if (!m_isCloen && m_IsCloneByDrag)
            {
                cloneGo = CloneThis();
            }

            v3_DragStartPos = transform.position;
            v3_offsetToMouse = v3_DragStartPos - Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, transform.position.z));

            var spr = GetComponent<SpriteRenderer>();
            normalOrder = spr.sortingOrder;
            spr.sortingOrder = hightOrder;

            if (moveRangeX.x > moveRangeX.y || moveRangeY.x > moveRangeY.y)
                Debug.LogError("限制區域的最小值大於最大值");
        }

        if (isDragMove)
        {
            isMoving = true;
        }
        else
        {
            isMoving = !isMoving;
        }
    }

    private void OnMouseUp()
    {
        if (isDragMove)
        {
            isMoving = false;
        }

        GetComponent<SpriteRenderer>().sortingOrder = normalOrder;
        v3_DragEndPos = transform.position;

        var colliderCenter = GetComponent<Collider2D>().bounds.center;
        var posX = Camera.main.WorldToScreenPoint(colliderCenter).x;
        var posY = Camera.main.WorldToScreenPoint(colliderCenter).y;

        float moveDis = Vector3.Distance(v3_DragStartPos, v3_DragEndPos);

        if (moveDis < 0.1f)
        {
            if (isDragMove)
            {
                if (m_IsCloneByDrag)
                {
                    if (this.transform.position != v3_OriginalPos)
                        Destroy(cloneGo);
                }
                else if (m_IsBackToOriPos) //移動不夠回到原位
                {
                    Back2OriPos();
                }
            }
        }
        else if (posX < 0 || posY < 0 || posY > Screen.height || posX > Screen.width)
        {
            if (m_IsCloneByDrag)
            {
                if (this.transform.position != v3_OriginalPos)
                    Destroy(cloneGo);
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
                    if (enterColliders[i].GetComponent<MatchPosItemControl>().isMultpPair)
                    {
                        if (enterColliders[i].transform.childCount != 0 && enterColliders[i].transform.GetChild(0).gameObject != this.gameObject)
                        {
                            Debug.LogFormat("有子物件不比對{0}", enterColliders[i].name);
                            continue;
                        }
                    }

                    float dis = Vector2.Distance(this.transform.position, enterColliders[i].transform.position);
                    if (dis < lastdis)
                    {
                        lastdis = dis;
                        m_ColliderObj = enterColliders[i];
                    }
                }

                if (m_IsMagnetic)
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
                        transform.position = new Vector3(this.m_ColliderObj.transform.position.x + offset.x, this.m_ColliderObj.transform.position.y + offset.y, -2);
                        SetMatchPosParent(m_ColliderObj);
                    }
                }
                else
                {
                    //移動夠多，如果有碰到感應區，則移動到感應區位置
                    SetMatchPosParent(m_ColliderObj);
                }
            }
            else
            {
                Debug.Log("沒有在感應區");

                m_ColliderObj = null;
                this.transform.SetParent(m_MoveArea);
                if (m_IsNotPosItemBackToOriPos)
                {
                    if (m_IsCloneByDrag)
                    {
                        Destroy(this.gameObject);
                        return;
                    }
                    else
                        Back2OriPos();
                }
                else
                    m_SetPosForGridEvent?.Invoke();
            }
        }
    }
    #endregion

    private void FixedUpdate()
    {
        if (isMoving)
        {
            transform.position = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, transform.position.z)) + v3_offsetToMouse;

            if (isMovementRestriction)
            {
                tempV3 = transform.position;
                if (tempV3.x < moveRangeX.x)
                    tempV3.x = moveRangeX.x;
                else if (tempV3.x > moveRangeX.y)
                    tempV3.x = moveRangeX.y;

                if (tempV3.y < moveRangeY.x)
                    tempV3.y = moveRangeY.x;
                else if (tempV3.y > moveRangeY.y)
                    tempV3.y = moveRangeY.y;

                transform.position = tempV3;
            }
        }
    }

    private GameObject CloneThis()
    {
        GameObject go = Instantiate(this.gameObject, v3_OriginalPos, this.transform.rotation);
        go.name = this.gameObject.name;
        go.GetComponent<SpriteRenderer>().sortingOrder = normalOrder;
        m_isCloen = true;
        return go;
    }

    public void SetMatchPosParent(GameObject matchPosObj)
    {
        this.transform.SetParent(m_ColliderObj.transform);
        GetComponent<SpriteRenderer>().sortingOrder = normalOrder;

        if (m_ColliderObj.GetComponent<MatchPosItemControl>())
            m_ColliderObj.GetComponent<MatchPosItemControl>().CheckPair();
    }

    protected void Back2OriPos()
    {
        this.transform.SetParent(null);
        this.transform.position = v3_OriginalPos;
        //m_SetPosForGridEvent?.Invoke();
    }

    private void OnDrawGizmosSelected()
    {
        if (isMovementRestriction && (moveRangeX.x < moveRangeX.y || moveRangeY.x < moveRangeY.y))
        {
            Vector2 tL, tR, bL, bR;
            tL = new Vector2(moveRangeX.x, moveRangeY.y);
            bL = new Vector2(moveRangeX.x, moveRangeY.x);
            tR = new Vector2(moveRangeX.y, moveRangeY.y);
            bR = new Vector2(moveRangeX.y, moveRangeY.x);

            if (!moveRangeX.Equals(Vector2.zero))
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(bL, bR);
                Gizmos.DrawLine(tL, tR);
            }

            if (!moveRangeY.Equals(Vector2.zero))
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(bL, tL);
                Gizmos.DrawLine(bR, tR);
            }
        }
    }
}
