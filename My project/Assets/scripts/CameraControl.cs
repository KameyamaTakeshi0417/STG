using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    public Vector3 cameraPosition;
    public GameObject player;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        ObeyPlayer();
    }
    void ObeyPlayer(){
transform.position=cameraPosition+player.transform.position;

    }
}
