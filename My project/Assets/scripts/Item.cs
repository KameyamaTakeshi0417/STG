using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
    public float addPow;
    public float addShootSpeed;
    public float addHP;
    public int bulletChangeType;//弾種変更アイテムなら1以上の特定の数値を、そうでない場合は-1を指定する
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // HPを持つコンポーネントを取得
            Player playerscript = collision.gameObject.GetComponent<Player>();
            playerscript.setHP(addHP);
            playerscript.setPow(addPow);
            playerscript.setShootSpeed(addShootSpeed);
            
            if (bulletChangeType > 0)
            {
                playerscript.setBulletType(bulletChangeType);
            }
        }
        Destroy(this.gameObject);
    }
}
