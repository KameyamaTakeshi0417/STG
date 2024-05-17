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
    //弾の撃つ角度の正規化
    public void setRotate(Vector3 rot) {
        rotate = rot.normalized;
    }
    //弾の速度決定
    public void setBulletSpeed(float mag){
        rotate *= mag;
        StartCoroutine(move());
    }
    //弾を撃ち出す
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
