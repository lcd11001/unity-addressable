using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateCube : MonoBehaviour
{
    [SerializeField] private float speed = 10f;
    [SerializeField] private Vector3 axis = Vector3.up; // [0, 1, 0]

    void Update()
    {
        transform.rotation *= Quaternion.AngleAxis(speed * Time.deltaTime, axis);
    }

    public void SetSpeed(float newSpeed)
    {
        speed = newSpeed;
    }
}
