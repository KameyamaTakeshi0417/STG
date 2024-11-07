using UnityEngine;
using UnityEngine.UI;

public class EnemyHPBar : MonoBehaviour
{
    public GameObject enemy; // 表示する対象の敵オブジェクト
    public Image healthBarFill; // HPバーのImage（進捗バー）
    private Health enemyHealth; // 敵のHealthスクリプト

    void Start()
    {
        if (enemy != null)
        {
            enemyHealth = enemy.GetComponent<Health>();
        }
    }

    void Update()
    {
        if (enemy != null && enemyHealth != null)
        {
            // 敵のHPをもとにHPバーを更新
            float healthPercentage = (float)enemyHealth.getCurrentHP() / enemyHealth.getHP();
            healthBarFill.fillAmount = healthPercentage;

            // HPバーの位置を敵の位置に追従
            Vector3 enemyScreenPosition = Camera.main.WorldToScreenPoint(enemy.transform.position);
            transform.position = enemyScreenPosition + new Vector3(0, 50, 0); // オフセットを調整
        }
        else
        {
            // 敵がいなくなったときにHPバーを非アクティブにする
            gameObject.SetActive(false);
        }
    }
}
