using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Bullet_Base : MonoBehaviour
{
    public float Dmg;
    public float Speed;
    public float DestroyTime;
    public Vector3 rotate;
    // Start is called before the first frame update
    void Start()
    {
       
    }
    // Update is called once per frame
    void Update()
    {
        
    }
    public void setRotate(Vector3 rot) {
        rotate = rot;
        StartCoroutine(move());
    }
    private IEnumerator move()
    {
        int count = 0;

        while (count <= DestroyTime)
        {
          
            // ’e‚ÌˆÊ’u‚ðXV‚·‚é
            transform.Translate(rotate * Speed * Time.deltaTime, Space.Self);

            // ’e‚ð‰ñ“]‚³‚¹‚é
           // transform.rotation = rotate;

            count++;
            yield return new WaitForSeconds(0.01f);
        }

        yield break;
    }

}
