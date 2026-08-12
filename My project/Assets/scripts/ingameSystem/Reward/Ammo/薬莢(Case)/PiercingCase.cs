using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PiercingCase : Case_Base
{
    // Start is called before the first frame update
    void Start()
    {
        gameObject.GetComponent<Bullet_Base>().piercingCount += rarelity + 1;
    }

    // Update is called once per frame
    void Update() { }

    
}

