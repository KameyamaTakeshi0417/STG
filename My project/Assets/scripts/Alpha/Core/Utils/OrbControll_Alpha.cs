using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class OrbControll_Alpha : MonoBehaviour
{
    // Start is called before the first frame update
    public int rarelity;
    private Transform playerTransform;
    private bool isCollected = false;
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            Collect(collision.gameObject);
        } 
        
    }
    void Start()
    {
        // プレイヤーを変数にキャッシュしておく
        GameObject playerObj = GameObject.Find("Player");
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

    private void Collect(GameObject playerObj)
    {
        if (!isCollected)
        {
            isCollected = true;
            if (GameObject.Find("manager") != null && GameObject.Find("manager").GetComponent<treasureManager_Alpha>() != null)
            {
                GameObject.Find("manager").GetComponent<treasureManager_Alpha>().GetTreasure(rarelity);
            }
        }
        // 確実に破壊する
        Destroy(gameObject);
    }

}
