using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Laser : MonoBehaviour
{
    LineRenderer _line;
    
    void Start()
    {
        _line = GetComponent<LineRenderer>();
    }

    void Update()
    {
        RaycastHit hit;
        Physics.Raycast(transform.position, transform.right, out hit);
        _line.SetPosition(1, Vector3.right * hit.distance / transform.lossyScale.x);
    }
}
