using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Alpha_Bullet_Controller : MonoBehaviour
{
    // Start is called before the first frame update
    public float damage = 0f;
    public int piercingCount = 0;
    public void Init() 
    {
    
    }

    protected  void OnTriggerEnter2D(Collider2D collision)
    {
        // 衝突したオブジェクトのタグをチェック
        if (collision.CompareTag("Enemy"))
        {

        }

    }

    protected void DestroyCheck()
    {
        piercingCount--;

        if (piercingCount < 0)
        {
            Destroy(this.gameObject);
        }
    }
}
