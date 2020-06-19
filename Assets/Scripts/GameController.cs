﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public OrbitController[] orbits;
    public GameObject obstacle;
    public GameObject portal;
    public float proximity = 0.2f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public OrbitController GetOrbit(int index)
    {
        return orbits[index];
    }
}
