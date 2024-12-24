using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HomingPrimer : Primer_Base
{
    // Start is called before the first frame update
    public override void StrikePrimer()
    {
        EquipManager equipManager = FindGameManager("GameManager").GetComponent<EquipManager>();
        // 現在装備している弾丸、ケース、プライマーを取得
        GameObject activeBullet = equipManager.GetActiveBullet();
        GameObject activeCase = equipManager.GetActiveCase();
        GameObject bulletPrefab;
        float ratio = 1.5f;
        Vector3 watch = FindGameManager("Player").GetComponent<Player>().watch;
        float bulletSpeed = FindGameManager("Player").GetComponent<Player>().bulletSpeed;
        Vector3 createPos = transform.position + (watch * ratio);

        bulletPrefab = Instantiate(
            Resources.Load<GameObject>("Objects/Bullet/NormalBullet"),
            createPos,
            Quaternion.identity
        );

        // 弾丸の基本ステータスを設定
        Bullet_Base bulletScript = bulletPrefab.GetComponent<Bullet_Base>();

        bulletScript.setStatus(watch, bulletSpeed, 10);

        // 弾丸の発射
        bulletScript?.fire();
    }

    GameObject FindGameManager(string targetName)
    {
        // すべての GameObject を取得する
        GameObject[] allGameObjects = FindObjectsOfType<GameObject>();

        // すべてのオブジェクトをループして "GameManager" の名前を持ち、DontDestroyOnLoad にあるか確認
        foreach (GameObject obj in allGameObjects)
        {
            if (obj.name == targetName && obj.scene.name == null)
            {
                return obj; // GameManager が見つかったら返す
            }
        }

        // 見つからなかった場合は null を返す
        return null;
    }
}
