using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Alpha.Item
{
    public class ExpItem_Alpha : MonoBehaviour
    {
        public int expValue = 1;
        private Transform playerTransform;
        private bool isCollected = false;

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                Collect();
            }
        }

        private void OnTriggerEnter2D(Collider2D collider)
        {
            if (collider.CompareTag("Player"))
            {
                Collect();
            }
        }

        void Start()
        {
            // プレイヤーを変数にキャッシュしておく
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj == null) playerObj = GameObject.Find("Player");
            
            if (playerObj != null)
            {
                playerTransform = playerObj.transform;
            }
            SetupScatter();
        }

        public void SetupScatter()
        {
            StartCoroutine(Homing());
        }

        private IEnumerator Homing()
        {
            // 3. 散らばるアニメーション
            float scatterDuration = 0.5f;
            float timer = 0f;
            Vector3 startPos = transform.position;
            // ランダムな方向へ散らばる目標位置を計算
            Vector2 randomDir = Random.insideUnitCircle.normalized;
            Vector3 targetScatterPos = startPos + new Vector3(randomDir.x, randomDir.y, 0f) * Random.Range(1.0f, 2.0f);

            while (timer < scatterDuration)
            {
                if (isCollected) yield break; // すでに回収されていたら中断

                timer += Time.deltaTime;
                // イーズアウト（徐々に減速するような動き）で移動
                float t = timer / scatterDuration;
                t = 1f - (1f - t) * (1f - t);
                transform.position = Vector3.Lerp(startPos, targetScatterPos, t);
                yield return null;
            }
            if (!isCollected) transform.position = targetScatterPos;

            // 4. 1秒間待機する
            yield return new WaitForSeconds(1.0f);

            // 5. プレイヤーに向かって飛んでいく（ホーミング）
            if (playerTransform == null)
            {
                GameObject p = GameObject.FindGameObjectWithTag("Player");
                if (p == null) p = GameObject.Find("Player");
                if (p != null) playerTransform = p.transform;
            }

            float homingSpeed = 2.0f;
            while (!isCollected && playerTransform != null)
            {
                // 時間経過で徐々に加速する
                homingSpeed += Time.deltaTime * 15f;
                transform.position = Vector3.MoveTowards(transform.position, playerTransform.position, homingSpeed * Time.deltaTime);

                // 念のため、距離が十分に近ければ回収処理を強制的に呼ぶ
                if (Vector3.Distance(transform.position, playerTransform.position) < 0.5f)
                {
                    Collect();
                    break;
                }

                yield return null;
            }
        }

        private void Collect()
        {
            if (!isCollected)
            {
                isCollected = true;
                
                var playerStatus = FindObjectOfType<playerStatusManager_Alpha>();
                if (playerStatus != null)
                {
                    playerStatus.AddExp(expValue);
                }

                if (Alpha.Core.ProceduralJuiceManager_Alpha.Instance != null)
                {
                    Alpha.Core.ProceduralJuiceManager_Alpha.Instance.SpawnTextPopup(
                        transform.position, 
                        "Orb Acquired!", 
                        new Color(1f, 1f, 0.5f, 1f)
                    );
                }
            }
            // 確実に破壊する
            Destroy(gameObject);
        }
    }
}
