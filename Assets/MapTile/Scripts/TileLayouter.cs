using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileLayouter : MonoBehaviour
{
    public float squareLength = 640;
    public int minZoom = 12;
    public int maxZoom = 18;

    public SquareTile squareTilePrefab;

    public void LayoutTiles()
    {
        // minZoom      -> 2^0
        // mizZoom+1    -> 2^2
        // mizZoom+2    -> 2^4

    }
}
