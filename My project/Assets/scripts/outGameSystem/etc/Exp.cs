using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Exp : MonoBehaviour
{ //エネミーのヘルスで、Hit後の死亡判定で呼び出されてるよ
    private Player playerScript;
    public int addPoint = 3;

    // Start is called before the first frame update
    void Start()
    {
        playerScript = GameObject.Find("Player").GetComponent<Player>();
    }

    // Update is called once per frame
    void Update() { }

    void OnTriggerEnter2D(Collider2D collision)
    {
        // 衝突したオブジェクトのタグをチェック
        if (collision.CompareTag("Player"))
        {
            playerScript.addExp(addPoint);
            // 破壊
            Destroy(gameObject);
        }
    }
}
