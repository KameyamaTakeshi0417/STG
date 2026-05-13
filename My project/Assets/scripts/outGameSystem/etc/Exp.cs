using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Exp : MonoBehaviour
{ //エネミーのヘルスで、Hit後の死亡判定で呼び出されてるよ
    public int addPoint = 3;

    private bool isCollected = false;
    private Transform playerTransform;

    void Start()
    {
        // プレイヤーを変数にキャッシュしておく
        GameObject playerObj = GameObject.Find("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
        }
    }

    /// <summary>
    /// エネミー死亡時に呼ばれるセットアップメソッド
    /// 散らばりアニメーションと、その後のホーミング処理を開始する
    /// </summary>
    public void SetupScatter(int index, int totalDrops, Vector3 origin)
    {
        StartCoroutine(ScatterAndHoming(index, totalDrops, origin));
    }

    private IEnumerator ScatterAndHoming(int index, int totalDrops, Vector3 origin)
    {
        // 1. 個数に応じて円の半径を決定する
        float radius = 1.0f;
        if (totalDrops >= 4 && totalDrops <= 10)
        {
            radius = 2.0f;
        }
        else if (totalDrops > 10)
        {
            radius = 3.5f;
        }

        // 2. 自分が散らばるべき円周上の位置を計算
        float angle = index * (Mathf.PI * 2f / totalDrops);
        Vector3 targetScatterPos = origin + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * radius;

        // 3. 散らばるアニメーション（約0.5秒かけてスムーズに移動）
        float scatterDuration = 0.5f;
        float timer = 0f;
        Vector3 startPos = transform.position;

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
            GameObject p = GameObject.Find("Player");
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
                Collect(playerTransform.gameObject);
                break;
            }

            yield return null;
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        // プレイヤーと衝突したら回収
        if (!isCollected && collision.CompareTag("Player"))
        {
            Collect(collision.gameObject);
        }
    }

    private void Collect(GameObject playerObj)
    {
        if (isCollected) return;
        isCollected = true;

        // 安全にPlayerコンポーネントを取得（親オブジェクトについている場合も考慮）
        Player playerScript = playerObj.GetComponentInParent<Player>();
        if (playerScript != null)
        {
            // ここでaddExp内部でエラーが起きても、次のDestroyが呼ばれるように保護
            try
            {
                playerScript.addExp(addPoint);
            }
            catch (System.Exception e)
            {
                Debug.LogError("Error adding EXP: " + e.Message);
            }
        }

        // 確実に破壊する
        Destroy(gameObject);
    }
}
