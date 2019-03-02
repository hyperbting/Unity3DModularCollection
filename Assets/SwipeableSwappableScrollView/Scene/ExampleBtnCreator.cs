using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent (typeof(ScrollRect))]
public class ExampleBtnCreator : MonoBehaviour
{
    public ScrollRect myScrollRect;
    public DragnDropHelper myDnDHelper;
    public MultiTouchDetector itemPrefab;
    public RectTransform contentHolder;

	// Use this for initialization
	void Start ()
    {
        //Create btns	
        for (int i = 0; i < 5; i++)
        {
            var script = Instantiate(itemPrefab, contentHolder);
            script.gameObject.name = i.ToString();
            script.Init(myScrollRect, myDnDHelper, ()=> { Debug.Log(i + " is Cliecked!"); });
        }
    }
}
