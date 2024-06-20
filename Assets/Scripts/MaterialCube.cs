using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class MaterialCube : MonoBehaviour
{
    [SerializeField] private Material[] material;

    private MeshRenderer meshRenderer;
    private int index = 0;

    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
    }

    private void Start()
    {
        RandomMaterial();
    }

    private void RandomMaterial()
    {
        var randomIndex = Random.Range(0, material.Length);
        while (randomIndex == index)
        {
            randomIndex = Random.Range(0, material.Length);
        };
        meshRenderer.material = material[randomIndex];
        index = randomIndex;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            RandomMaterial();
        }
    }
}
