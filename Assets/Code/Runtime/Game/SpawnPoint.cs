using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class SpawnPoint : MonoBehaviour
{
    public bool inUse;

    public Vector3 Position => gameObject.transform.position;
    // Start is called before the first frame update
    void Start()
    {
        inUse = false;
    }

    // Update is called once per frame
    void Update()
    {
        inUse = false;
    }
}
