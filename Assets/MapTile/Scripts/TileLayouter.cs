using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileLayouter : MonoBehaviour
{
    public int minZoom = 15;
    public int maxZoom = 18;

    public SquareTile squareTilePrefab;

    public Dictionary<int, List<SquareTile>> zoomTiles;

    private void Start()
    {
        zoomTiles = new Dictionary<int, List<SquareTile>>();

        for (int i = minZoom; i < maxZoom; i++)
            zoomTiles[i] = new List<SquareTile>();

        LayoutFirstTile(minZoom);
    }

    public void LayoutFirstTile(int _zoomLevel)
    {
        var til = Instantiate(squareTilePrefab, this.transform);
        til.Setup(this, _zoomLevel, (maxZoom- minZoom));
    }

    public SquareTile Instantiater(Transform _parent)
    {
        return Instantiate(squareTilePrefab, _parent);
    }


}
