using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReactiveBarrier : MonoBehaviour
{
    // Start is called before the first frame update
    protected int disappearCount = 2;
    public int damage;
    public float pushForce = 10f; // 弾く力の強さ

    // Start is called before the first frame update
    void Start()
    {
        startDisappear();
    }

    public void startDisappear()
    {
        StartCoroutine("disappear");
    }

    // Update is called once per frame
    void Update() { }

    private IEnumerator disappear()
    {
        int count = 0;
        while (count < disappearCount)
        {
            count++;
            yield return new WaitForSecondsRealtime(disappearCount);
        }
        Destroy(this.gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 衝突したオブジェクトのタグをチェック
        if (collision.CompareTag("Player"))
        {
            // HPを持つコンポーネントを取得してダメージを与える
            PlayerHealth health = collision.GetComponent<PlayerHealth>();
            if (health != null)
                health.TakeDamage(damage);

            // 敵を引き寄せる処理
            Rigidbody2D enemyRb = collision.GetComponent<Rigidbody2D>();
            if (enemyRb != null)
            {
                Vector3 directionToPlayer = (
                    collision.transform.position - transform.position
                ).normalized;
                enemyRb.AddForce(directionToPlayer * pushForce, ForceMode2D.Impulse);
            }
        }
    }
}
