using System.Collections;
using UnityEngine;

public class EnemyChecker : MonoBehaviour
{
    public float checkInterval = 1.0f; // チェック間隔（秒）
    public GameObject[] enemies;
    void Start()
    {
        GameObject.Find("GameManager").GetComponent<GameManager>().setCleared(false);
        // 定期的に敵の数をチェックするコルーチンを開始
        StartCoroutine(CheckEnemies());
    }

    private IEnumerator CheckEnemies()
    {
        yield return new WaitForEndOfFrame();
        while (true)
        {
            yield return new WaitForSeconds(checkInterval); // チェック間隔を待機

            // Enemyタグを持つオブジェクトを全て取得
            enemies = GameObject.FindGameObjectsWithTag("Enemy");

            // 敵の数が0かどうかを確認
            if (enemies.Length == 0)
            {
                OnAllEnemiesDefeated();
            }
        }
    }

    private void OnAllEnemiesDefeated()
    {
        // 敵が0になったときの処理
//        Debug.Log("All enemies have been defeated!");
        // 必要な処理をここに追加
        GameObject.Find("GameManager").GetComponent<GameManager>().setCleared(true);
    }
}