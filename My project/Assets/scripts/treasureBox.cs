using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class treasureBox : MonoBehaviour
{
    public int treasureType;
    // Start is called before the first frame update
    void Start()
    {
        treasureType=Random.Range(0,4);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
          void OnTriggerEnter2D(Collider2D collision)
    {
        // 衝突したオブジェクトのタグをチェック
        if (collision.CompareTag("Player"))
        {
        collision.GetComponent<Player>().GetItem(treasureType);   
            // 破壊
            Destroy(gameObject);
        }
    }

}
