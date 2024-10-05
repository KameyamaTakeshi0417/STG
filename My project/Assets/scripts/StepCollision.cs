using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StepCollision : MonoBehaviour
{
    public bool Onstep;
    public bool canEnter;
    // Start is called before the first frame update
    void Start()
    {
        Onstep=false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void setEnterable(bool set){
canEnter=set;

    }
     void OnTriggerEnter2D(Collider2D collision)
    {
if(collision.gameObject.tag=="player")
Onstep=true;

    }
}
