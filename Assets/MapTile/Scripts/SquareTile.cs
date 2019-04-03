using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SquareTile : MonoBehaviour
{
    public TileLayouter owner;
    public SquareTile parent;

    public UnityEngine.UI.RawImage myRawImage;

    [Header("Debug Purpose")]
    [SerializeField]
    private int zoomLevel;
    [SerializeField]
    private RectTransform myRect;

    public void Setup(TileLayouter _owner, SquareTile _parent, int _zoomLevel, int _index, int _levelCount)
    {
        parent = _parent;

        name = _zoomLevel.ToString() + "-" + _index;

        Setup(_owner, _zoomLevel, _levelCount);
    }

    public void Setup(TileLayouter _owner, int _zoomLevel, int levelCount)
    {
        zoomLevel = _zoomLevel;

        owner = _owner;
        //_owner.Register(this);

        if (levelCount > 0)
            StartCoroutine(DrawFourSquare(levelCount));
    }

    IEnumerator DrawFourSquare(int _layerCount)
    {
        Debug.Log(name);

        var nexZoo = zoomLevel + 1;
        var nexLayCou = _layerCount - 1;
        Vector3 nexLeveScal = myRect.localScale / 2;
        float halfLength = myRect.sizeDelta.x / 4;

        // four square centers
        Vector2 center01 =  (Vector2.up + Vector2.left) * halfLength;
        Vector2 center02 =  (Vector2.up + Vector2.right) * halfLength;
        Vector2 center03 =  (Vector2.left + Vector2.down) * halfLength;
        Vector2 center04 =  (Vector2.down + Vector2.right) * halfLength;

        //Instantiate
        var til01 = owner.Instantiater(transform);
        yield return null;
        var til02 = owner.Instantiater(transform);
        yield return null;
        var til03 = owner.Instantiater(transform);
        yield return null;
        var til04 = owner.Instantiater(transform);
        yield return null;

        til01.Setup(owner, this, nexZoo, 1, nexLayCou);
        til02.Setup(owner, this, nexZoo, 2, nexLayCou);
        til03.Setup(owner, this, nexZoo, 3, nexLayCou);
        til04.Setup(owner, this, nexZoo, 4, nexLayCou);

        til01.myRect.localScale = nexLeveScal;
        til02.myRect.localScale = nexLeveScal;
        til03.myRect.localScale = nexLeveScal;
        til04.myRect.localScale = nexLeveScal;

        til01.myRect.localPosition = center01;
        til02.myRect.localPosition = center02;
        til03.myRect.localPosition = center03;
        til04.myRect.localPosition = center04;
    }
}