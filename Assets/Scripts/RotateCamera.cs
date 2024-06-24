using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateCamera : MonoBehaviour
{
    [SerializeField] private float speed = 2f;
    [SerializeField] private float radius = 10f;
    [SerializeField] private float height = 5f;
    void Update()
    {
        float x = Mathf.Sin(Time.time * speed) * radius;
        float z = Mathf.Cos(Time.time * speed) * radius;
        transform.position = new Vector3(x, height, z);
        transform.LookAt(Vector3.zero);
    }
}
