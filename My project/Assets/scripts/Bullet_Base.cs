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
          
            // 弾の位置を更新する
            transform.Translate(rotate * Speed * Time.deltaTime, Space.Self);

            // 弾を回転させる
           // transform.rotation = rotate;

            count++;
            yield return new WaitForSeconds(0.01f);
        }

        yield break;
    }

}
