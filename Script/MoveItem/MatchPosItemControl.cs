using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 物件感應區
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(BoxCollider2D))]
public class MatchPosItemControl : MonoBehaviour
{
    [Header("檢查obj名稱 判斷是否正確")]
    public bool isCheckObjName;

    [Header("正確的配對物件名字")]
    public string ansObjName;

    [Header("檢查obj index 判斷是否正確")]
    public bool isCheckObjIndex;

    [Header("正確的配對物件index")]
    public int ansObjIndex;

    /// <summary> 答對後要生成的物件 </summary>
    [Header("答對後要生成的物件")]
    public GameObject CorrectObj;

    /// <summary> 答對後生成物件的 LocalPos </summary>
    [Header("答對後生成物件的 LocalPos")]
    public Vector2 CorrectObjPos;

    /// <summary> BoxCollider縮放比例 </summary>
    [Header("BoxCollider碰撞區域縮放比例")]
    public Vector2 BoxColliderScale;

    /// <summary> 是否是答案 </summary>
    [Header("是否要放正確答案")]
    public bool isAnswer;

    private void Start()
    {
        //產生trigger
        BoxCollider2D BCollider = GetComponent<BoxCollider2D>();
        GetComponent<BoxCollider2D>().isTrigger = true;
        transform.tag = "matchPosItem";

        if (BoxColliderScale != Vector2.zero)
            BCollider.size = new Vector2(BCollider.size.x * BoxColliderScale.x, BCollider.size.y * BoxColliderScale.y);
        else
            BCollider.size = new Vector2(BCollider.size.x * 0.8f, BCollider.size.y * 0.8f);
    }

    public bool CheckAnsIsRight()
    {
        if (!isCheckObjName && !isCheckObjIndex)
        {
            Debug.LogError("isCheckObjName & isCheckObjIndex are false!!");
            return false;
        }

        for (int i = 0; i < transform.childCount; i++)
        {
            if (isCheckObjName)
            {
                if (transform.GetChild(i).name != ansObjName)
                    return false;
            }

            if (isCheckObjIndex)
            {
                if (transform.GetChild(i).GetComponent<ItemControl>().index != ansObjIndex)
                    return false;
            }
        }
        return true;
    }

    /// <summary>
    /// 碰撞到的物件名字，抓取子物件名稱
    /// </summary>
    /// <returns></returns>
    public List<GameObject> OnCollisionObjName()
    {
        List<GameObject> m_OnColliderObjName = new List<GameObject>();
        if (OnCollidionrObjCount() == 0)
        {
            m_OnColliderObjName.Add(null);
        }
        else
        {
            for (int i = 0; i < OnCollidionrObjCount(); i++)
            {
                m_OnColliderObjName.Add(this.transform.GetChild(i).gameObject);
            }
        }
        return m_OnColliderObjName;
    }

    /// <summary> 子物件數量 </summary>
    public int OnCollidionrObjCount()
    {
        int count = this.transform.childCount;
        return count;
    }

#if UNITY_EDITOR
    [ContextMenu("檢查內容物")]
    private void TestCheckAns()
    {
        Debug.Log(CheckAnsIsRight());
        //string str = CheckAnsIsRight() ? "正確" : "錯誤";
    }
#endif

    private void OnDrawGizmos()
    {
        if (isAnswer)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, 0.5f);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (CorrectObj != null)
        {
            Gizmos.color = Color.blue;
            Vector3 corpos = CorrectObjPos;
            Gizmos.DrawWireSphere(transform.position + corpos, 0.5f);
        }
    }
}
