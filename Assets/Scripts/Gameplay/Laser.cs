using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Laser : MonoBehaviour
{
    public LayerMask BulletLayer;

    LineRenderer _line;
    
    void Start()
    {
        _line = GetComponent<LineRenderer>();
    }

    void Update()
    {
        RaycastHit hit;
        Physics.Raycast(transform.position, transform.forward, out hit, Mathf.Infinity, BulletLayer);
        _line.SetPosition(1, Vector3.forward * hit.distance / transform.lossyScale.x);
    }
}
