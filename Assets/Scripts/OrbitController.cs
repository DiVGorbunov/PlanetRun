using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrbitController : MonoBehaviour
{
    public float X => gameObject.transform.position.x;
    public float Y => gameObject.transform.position.z;
    public float A => gameObject.transform.localScale.x / 2;
    public float B => gameObject.transform.localScale.z / 2;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
