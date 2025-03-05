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

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 衝突したオブジェクトのタグをチェック
        if (collision.gameObject.CompareTag("Player"))
        {
            // プレイヤーの健康を管理しているコンポーネントを取得し、ダメージを与える
            PlayerHealth health = collision.gameObject.GetComponent<PlayerHealth>();
            if (health != null)
            {
                health.TakeDamage(damage);
            }

            // プレイヤーを突き放す処理
            Vector2 directionToPlayer = (
                collision.transform.position - transform.position
            ).normalized;

            // プレイヤーの新しい位置を計算して移動させる
            float distanceToMove = 10f; // 1フレームで移動させる距離
            Vector2 newPlayerPosition = Vector2.Lerp(
                collision.transform.position,
                (Vector2)collision.transform.position + directionToPlayer * distanceToMove,
                Time.deltaTime * pushForce
            );
            // プレイヤーの位置を更新
            collision.transform.position = newPlayerPosition;
        }
    }
}
