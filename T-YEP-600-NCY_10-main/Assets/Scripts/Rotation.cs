using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotation : MonoBehaviour
{

    public GameObject prefab;
    public float rotationSpeed = 10f;

    private GameObject instance;

    void Start()
    {
        instance = Instantiate(prefab, Vector3.zero, Quaternion.identity);
    }

    void Update()
    {
        instance.transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
    }
}