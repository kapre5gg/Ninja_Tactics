using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine;

// 타깃을 바라보는 기능
public class LookAtTarget : MonoBehaviour
{
    public bool isReverseForward;   // 바라보는 방향 180도 회전 여부(반대방향)

    private void Update()
    {
        //if (isStart == false) return;

        // a. 반대방향인 경우
        if (isReverseForward)
        {
            transform.forward = Camera.main.transform.forward * -1;
        }
        // b. 정방향인 경우
        else
        {
            transform.forward = Camera.main.transform.forward;
        }
    }

    private void OnMouseEnter()
    {
        transform.GetChild(0).GetChild(4).GetComponent<CanvasGroup>().alpha = 1;
    }

    private void OnMouseExit()
    {
       transform.GetChild(0).GetChild(4).GetComponent<CanvasGroup>().alpha = 0;
    }
}
