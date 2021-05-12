using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class ConnectionLine : MonoBehaviour
{
    /// <summary> 顯示起點圖片 </summary>
    [Header("顯示起點圖片")]
    public bool isShowStartImg;

    /// <summary> 顯示目標圖片 </summary>
    [Header("顯示目標圖片")]
    public bool isShowTargetImg;

    AudioSource audioS;
    public AudioClip adioRight;
    public AudioClip adioFail;

    [Range(1, 15)]
    public float width;

    [Range(10, 30)]
    public float detectDis;

    Transform tsfStart;
    Transform tsfTarget;
    Transform tsfMove;
    RectTransform rtsfLine;
    bool isMoving;

    void Start()
    {
        tsfStart = transform.Find("Start");
        tsfTarget = transform.Find("Target");
        tsfMove = transform.Find("Move");
        rtsfLine = transform.Find("Line").GetComponent<RectTransform>();

        tsfStart.gameObject.SetActive(isShowStartImg);
        tsfTarget.gameObject.SetActive(isShowTargetImg);

        tsfMove.GetComponent<MoveDrag>().pointUpEvent += OnPointUpTheMove;
        tsfMove.GetComponent<MoveDrag>().pointDownEvent += OnPointDownTheMove;
        audioS = GetComponent<AudioSource>();
    }

    private void Update()
    {
        if (!isMoving) return;
        DrawLine();
    }

    private void OnDestroy()
    {
        tsfMove.GetComponent<MoveDrag>().pointUpEvent -= OnPointUpTheMove;
        tsfMove.GetComponent<MoveDrag>().pointDownEvent -= OnPointDownTheMove;
    }

    //private void MoveObj()
    //{
    //    var mousePos = Input.mousePosition;
    //    Debug.Log(mousePos);
    //    mousePos.z = transform.position.z;
    //    mousePos = Camera.main.ScreenToWorldPoint(mousePos);
    //    tsfMove.position = mousePos;
    //}

    private void DrawLine()
    {
        var dir = tsfMove.position - tsfStart.position;
        var dis = Vector3.Distance(tsfMove.position, tsfStart.position);

        Vector2 size = rtsfLine.sizeDelta;
        size.x = width;
        size.y = dis;
        rtsfLine.sizeDelta = size;
        rtsfLine.up = dir;
    }

    private bool CheckTargetPos()
    {
        var dis = Vector3.Distance(tsfMove.position, tsfTarget.position);
        return dis < detectDis;
    }

    public void OnPointDownTheMove()
    {
        isMoving = true;
    }

    public void OnPointUpTheMove()
    {
        isMoving = false;
        if (CheckTargetPos())
        {
            if (adioRight != null)
                audioS.PlayOneShot(adioRight);

            tsfMove.position = tsfTarget.position;
        }
        else
        {
            if (adioFail != null)
                audioS.PlayOneShot(adioFail);

            tsfMove.position = tsfStart.position;
        }
        DrawLine();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.black;
        Gizmos.DrawWireSphere(transform.Find("Target").position, detectDis);
    }
}
