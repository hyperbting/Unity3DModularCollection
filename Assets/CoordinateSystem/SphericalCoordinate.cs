using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SphericalCoordinate : MonoBehaviour
{
  [Tooltip("this Object always try to facing in LateUpdate")]
    public bool keepUpdating;

    [Header("Base Information")]
    [Range(0,360)]
    public float azimuthalDegree;// north as 0; east as 90...
    [Range(0,180)]
    public float polarDegree;
    public float radius;
    
    private Transform parentTraY;
    const float radian2Degree = 180/Mathf.PI;
    
    void Start ()
    {
        var objToSpawn = new GameObject("RotationParent-"+ gameObject.name);
        parentTraY = objToSpawn.transform;
        parentTraY.SetParent(transform.parent);

        if (transform.parent == null)
            Debug.LogError("Object MUST be placed under a Game Object");

        transform.SetParent(parentTraY);

        SetTransform(radius, azimuthalDegree, polarDegree);
    }
    
    public void SetTransform(float _depth, float _polar, float _azimuthalDegree)
    {
        parentTraY.eulerAngles = new Vector3(_polar, _azimuthalDegree, 0);
        transform.localPosition = new Vector3(0, 0, _depth);
    }

    private void LateUpdate()
    {
        if(keepUpdating)
            SetTransform(radius, azimuthalDegree, polarDegree);
    }
}
