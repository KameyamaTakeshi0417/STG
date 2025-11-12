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
            // プレイヤーを吹き飛ばす方向
            Vector2 directionToPlayer = (
                collision.transform.position - transform.position
            ).normalized;

            // プレイヤーを移動させる処理
            float distanceToMove = 10f; // 1フレームで移動させる距離
            Vector2 startPosition = collision.transform.position;
            Vector2 targetPosition =
                (Vector2)collision.transform.position + directionToPlayer * distanceToMove;

            // 壁のチェック
            RaycastHit2D hit = Physics2D.Raycast(
                startPosition,
                directionToPlayer,
                distanceToMove,
                LayerMask.GetMask("Wall")
            );

            // 壁がなければLerpで移動
            if (hit.collider == null)
            {
                // 壁がない場合、Lerpで移動
                float lerpSpeed = 5f; // 移動速度（適宜調整）
                Vector2 newPosition = Vector2.Lerp(
                    startPosition,
                    targetPosition,
                    Time.deltaTime * lerpSpeed
                );
                collision.transform.position = newPosition;
            }
            else
            {
                // 壁があった場合、壁に衝突する位置で停止
                collision.transform.position = hit.point;
            }

            // プレイヤーにダメージを与える処理（そのまま保持）
            PlayerHealth health = collision.gameObject.GetComponent<PlayerHealth>();
            if (health != null)
            {
                health.TakeDamage(damage);
            }
        }
    }
}
