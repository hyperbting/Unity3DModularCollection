using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DragnDropHelper : MonoBehaviour
{

    [Header("DragnDrop Setting")]
    public ScrollRect scrollRect;

    [Range(0.5f, 2f)]
    public float secondToDnDMode = 1f;

    public bool dragnDropMode = false;

    public void OnBtnPointerUp()
    {
        CancelInvoke("SetToDnDMode");
        if (dragnDropMode)
        {
            //scrollRect.enabled = true;
        }
    }

    public void OnBtnPointerDown()
    {
        dragnDropMode = false;

        Invoke("SetToDnDMode", secondToDnDMode);
    }

    public void CancelCountDownInvoke()
    {
        CancelInvoke("SetToDnDMode");
    }

    void SetToDnDMode()
    {
        dragnDropMode = true;
        //myScrollRect.enabled = false;

        //var holding = EventSystem.current.currentSelectedGameObject.GetComponent<MultiTouchDetector>();
        //if (holding != null)
        //    holding.interactable = false;
    }
}
