﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MultiTouchDetector : Selectable, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public RectTransform BtnHolder;
    public RectTransform animHolder;

    [Header("Single/Swipe Setting")]
    [Tooltip("min pixel to become Swipe/Scroll")]
    [Range(6f, 60f)]
    public float minPixelLeaveClick;

    [Header("Swipe Setting")]
    [Range(0.1f, 0.9f)]
    public float ratioToOpen = 0.4f;
    public float AnimDuration = 0.5f;
    [Range(10f, 100f)]
    public float sideTabMovementPerFrame;

    [Header("Drag n Drop Setting")]
    public DragnDropHelper myDnDHelper;

    #region Debug
    [Header("Debug Purpose")]

    [SerializeField]
    private ScrollRect myScrollRect;

    [SerializeField]
    private TouchBoardStatus myBoardStatus ;

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
    private System.Action myOnClicked;
    #endregion

    public void Init(ScrollRect _scrollRect, DragnDropHelper _dndHelper, System.Action _onClicked)
    {
        //Debug.Log("Init");
        BtnHolder.localPosition = Vector3.zero; //Vector3.right * btnHolderWidth;        

        myScrollRect = _scrollRect;
        myDnDHelper = _dndHelper;
        myOnClicked = _onClicked;
    }

    void ResetPointerMovementData()
    {
        acuumlatedSwipePixel = Vector2.zero;
    }

    #region Pointer
    public override void OnPointerDown(PointerEventData eventData)
    {
        myBoardStatus = TouchBoardStatus.SingleClick;

        btnHolderWidth = BtnHolder.sizeDelta.x;
        widthToOpen = -btnHolderWidth * ratioToOpen;

        myDnDHelper.OnBtnPointerDown(()=> { myBoardStatus = TouchBoardStatus.LongPress; });

        base.OnPointerDown(eventData);
    }

    //Detect if clicks are no longer registering
    public override void OnPointerUp(PointerEventData pointerEventData)
    {
        //Debug.Log("OnPointerUp:" + pointerEventData);

        myDnDHelper.CancelCountDownInvoke();

        switch (myBoardStatus)
        {
            case TouchBoardStatus.LeftRight:
                DetermineSwipeResult();
                break;
            case TouchBoardStatus.LongPress:
                myDnDHelper.OnBtnPointerUp();
                break;
            case TouchBoardStatus.SingleClick:
                ////Was Menu opened
                if (swipeOpened)
                {
                    swipeOpened = false;
                    //close tab
                    BtnHolder.localPosition = Vector3.zero;
                    //swipeAnimTweener = BtnHolder.DOLocalMoveX(0, AnimDuration).OnComplete(() => { swipeOpened = false; });
                    break;
                }

                ////Invoke Btn if Tab Closed
                if (myOnClicked != null)
                    myOnClicked();
                base.OnPointerUp(pointerEventData);
                break;
            case TouchBoardStatus.Unknown:
            case TouchBoardStatus.UpDown:
            default:
                break;
        }
    }
    #endregion

    #region Drag
    public void OnBeginDrag(PointerEventData eventData)
    {
        //Debug.Log("OnBeginDrag:" + eventData);
        ResetPointerMovementData();
    }

    public void OnDrag(PointerEventData data)
    {
        //Debug.Log("OnDrag:" + data);
        acuumlatedSwipePixel.x += Mathf.Abs(data.delta.x);
        acuumlatedSwipePixel.y += Mathf.Abs(data.delta.y);

        ////Determine whether stay in CLICK or move to SWIPE/ SCROLL
        DetermineClickOrDrag(data);

        switch (myBoardStatus)
        {
            case TouchBoardStatus.LeftRight:
                // Update hidden menu position
                MoveBtnHolder(data.delta.x);
                break;
            case TouchBoardStatus.LongPress:
                myDnDHelper.OnBtnDrag(this, data);
                break;
            case TouchBoardStatus.UpDown:
            case TouchBoardStatus.SingleClick:
            case TouchBoardStatus.Unknown:
            default:
                // do nothing onDrag()
                break;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        //Debug.Log("OnEndDrag:" + eventData);
        //// all the action are delayed to OnPointerUp()
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

    /// <summary>
    /// Determine whether stay in CLICK or move to SWIPE/ SCROLL
    /// WILL NOT move status from LONGPress to SWIPE/ SCROLL
    /// It is an One-way gate for state change
    /// </summary>
    /// <param name="_data"></param>
    public void DetermineClickOrDrag(PointerEventData _data)
    {
        // do nothing if already in status LongPress/ SwipeMenu/ Scroll
        if (myBoardStatus != TouchBoardStatus.SingleClick)
            return;

        // if total movement too little, wait for more data to determine
        if (acuumlatedSwipePixel.magnitude < minPixelLeaveClick)
            return;

        //cancel Long Press CountDown!
        myDnDHelper.CancelCountDownInvoke();

        //Debug.Log("Drag/ Scroll");
        _data.eligibleForClick = false;

        ////determine which direction is swiping        
        if (acuumlatedSwipePixel.x > acuumlatedSwipePixel.y)// Left or Right
        {
            myBoardStatus = TouchBoardStatus.LeftRight;
        }
        else  // Up or Down
        {
            myBoardStatus = TouchBoardStatus.UpDown;

            //Delayed Scrolling Setup from OnBeginDrag
            myScrollRect.OnBeginDrag(_data);
            _data.pointerDrag = myScrollRect.gameObject;
        }
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

public enum TouchBoardStatus
{
    Unknown,
    SingleClick,
    LeftRight,
    UpDown,
    LongPress
}