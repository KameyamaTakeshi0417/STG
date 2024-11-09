using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class money : MonoBehaviour
{
    public int moneyValue;
    // Start is called before the first frame update
    void Start()
    {
//        gameObject.GetComponent<Health>().setMoneyCount(moneyValue);
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
            GameObject.Find("GameManager").GetComponent<pointManager>().addMoney(moneyValue);
            // 弾を破壊
            Destroy(this.gameObject);

        }

    }
}
