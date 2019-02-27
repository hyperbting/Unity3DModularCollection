using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MultiTouchDetector : Selectable, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public RectTransform BtnHolder;

    [Header("Single/Swipe Setting")]
    [Tooltip("min pixel to open side tab")]
    [Range(6f, 60f)]
    public float minPixelToOpenTab = 6f;

    [Header("Swipe Setting")]
    [Range(0.1f, 0.9f)]
    public float ratioToOpen = 0.4f;
    public float AnimDuration = 0.5f;
    [Range(10f, 100f)]
    public float sideTabMovementPerFrame = 10f;

    public DragnDropHelper myDnDHelper;

    #region Debug
    [Header("Debug Purpose")]
    [SerializeField]
    [Tooltip("As long as it shows one pixel, it is marked as OPENED")]
    private bool swipeOpened = false;
    [Space]
    [SerializeField]
    private float btnHolderWidth = -1;
    [Space]
    [SerializeField]
    private float widthToOpen = -1;
    [Space]
    [SerializeField]
    private Vector2 acuumlatedSwipePixel = Vector2.zero;
    [Space]
    [SerializeField]
    private ScrollRect myScrollRect;
    private System.Action myOnClicked;
    #endregion

    public void Init(ScrollRect _scrollRect, System.Action _onClicked)
    {
        //Debug.Log("Init");
        BtnHolder.localPosition = Vector3.zero; //Vector3.right * btnHolderWidth;        

        myScrollRect = _scrollRect;

        myOnClicked = _onClicked;
    }

    void ResetData()
    {
        acuumlatedSwipePixel = Vector2.zero;
    }

    #region Pointer
    public override void OnPointerDown(PointerEventData eventData)
    {
        btnHolderWidth = BtnHolder.sizeDelta.x;
        widthToOpen = -btnHolderWidth * ratioToOpen;

        myDnDHelper.OnBtnPointerDown();

        base.OnPointerDown(eventData);
    }

    //Detect if clicks are no longer registering
    public override void OnPointerUp(PointerEventData pointerEventData)
    {
        CancelInvoke("SetToDnDMode");

        myDnDHelper.OnBtnPointerUp();

        Debug.Log(name + " No longer being clicked " + acuumlatedSwipePixel.x + " " +  minPixelToOpenTab);

        if (acuumlatedSwipePixel.x > minPixelToOpenTab)// move a lot, detemine Swipe 
            return;

        //close tab
        //swipeAnimTweener = BtnHolder.DOLocalMoveX(0, AnimDuration).OnComplete(() => { swipeOpened = false; });
        BtnHolder.localPosition = Vector3.zero;

        //Invoke Btn if Tab Closed
        if (!swipeOpened)
        {
            //Debug.Log("Clicked!");
            if (myOnClicked != null)
                myOnClicked();
            base.OnPointerUp(pointerEventData);
        }
    }
    #endregion

    #region Drag
    //private float beginDragTime;
    public void OnBeginDrag(PointerEventData eventData)
    {
        myDnDHelper.CancelCountDownInvoke();
        if (myDnDHelper.dragnDropMode)
            return;

        //Debug.Log("OnBeginDrag" + eventData);
        ResetData();

        myScrollRect.OnBeginDrag(eventData);
    }

    public void OnDrag(PointerEventData data)
    {
        if (myDnDHelper.dragnDropMode)
            return;

        Debug.Log("OnDrag" + data);

        acuumlatedSwipePixel.x += Mathf.Abs(data.delta.x);
        acuumlatedSwipePixel.y += Mathf.Abs(data.delta.y);

        if (acuumlatedSwipePixel.x > acuumlatedSwipePixel.y)
        {
            MoveBtnHolder(data.delta.x);
        }
        else
        {
            ///Set the drag target to ScrollRect
            data.eligibleForClick = false;
            data.pointerDrag = myScrollRect.gameObject;

            DetermineSwipeResult();
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (myDnDHelper.dragnDropMode)
            return;

        Debug.Log("OnEndDrag" + eventData);
        if (acuumlatedSwipePixel.x < minPixelToOpenTab)
        {
            //// IsClick
            OnPointerUp(eventData);
        }
        else
        {
            /// IsSwipe
            DetermineSwipeResult();
        }
    }
    #endregion

    void MoveBtnHolder(float _amount)
    {
        var v3 = BtnHolder.localPosition;
        if (_amount > 1)
        {
            v3.x += sideTabMovementPerFrame;
        }
        else
        {
            v3.x -= sideTabMovementPerFrame;
        }

        if (v3.x < -btnHolderWidth)
            v3.x = -btnHolderWidth;

        if (v3.x > 0)
            v3.x = 0;

        BtnHolder.localPosition = v3;
    }

    // determine on/ off based on current location
    void DetermineSwipeResult()
    {
        //Debug.Log("DetermineSwipeResult:" + (BtnHolder.localPosition.x < widthToOpen));
        if (BtnHolder.localPosition.x < widthToOpen)
        {
            swipeOpened = true;

            ////open it
            BtnHolder.localPosition = Vector3.left * btnHolderWidth;
            //swipeAnimTweener = BtnHolder.DOLocalMoveX(-btnHolderWidth, AnimDuration)
            //    .OnComplete(() => {
            //        ResetData();
            //    });
        }
        else
        {
            ////close it
            BtnHolder.localPosition = Vector3.zero;
            //swipeAnimTweener = BtnHolder.DOLocalMoveX(0, AnimDuration)
            //    .OnComplete(() => {
            //        swipeOpened = false;
            //        ResetData();
            //    });
        }
    }
}