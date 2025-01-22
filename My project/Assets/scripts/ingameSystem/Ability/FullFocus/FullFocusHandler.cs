using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FullFocusHandler : MonoBehaviour
{
    public float focusCounter = 0;
    public float fullFocusCount = 3;

    // Start is called before the first frame update
    void Start() { }

    // Update is called once per frame
    void Update() { }

    void FixedUpdate()
    {
        if (focusCounter <= fullFocusCount)
            focusCounter += Time.deltaTime;
    }

    public float ShootCheck()
    {
        float ret = 0;
        if (focusCounter >= fullFocusCount)
        {
            ret = (GetComponent<Player>().pow + GetComponent<Player>().DamageAdd) * 10;
        }
        focusCounter = 0;
        return ret;
    }
}
