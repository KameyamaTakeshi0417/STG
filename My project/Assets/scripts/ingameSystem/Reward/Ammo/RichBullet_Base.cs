using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RichBullet_Base : Bullet_Base
{
    public enum TriggerTiming
    {
        OnDestroy, // 消滅時に発射
        OnFlight   // 航行中に発射
    }

    [System.Serializable]
    public struct ChildrenBulletInfo
    {
        [Tooltip("発射する弾のプレハブ")]
        public GameObject bulletPrefab;
        [Tooltip("発射するタイミング")]
        public TriggerTiming timing;
        [Tooltip("OnFlightの場合の発射間隔（秒）")]
        public float flightInterval;
        [Tooltip("生成される弾の速度")]
        public float speed;
        [Tooltip("生成される弾のダメージ倍率（親弾に対する割合。1.0で等倍）")]
        public float damageMultiplier;
        [Tooltip("発射位置のオフセット（親弾の進行方向を基準にする）")]
        public Vector3 offset;
        [Tooltip("発射方向（親弾の進行方向に加算される角度）")]
        public Vector3 rotate;

        // 以前の変数（互換性とインスペクタデータの保持用）
        [HideInInspector] public List<EnemyBehaviorData_Base> behaviorsToRunInParallel;
    }

    [Header("Rows (1D struct array)")]
    [SerializeField] private ChildrenBulletInfo[] CreateBulletInfo; // これがセットされていたら行数分処理する

    private List<Coroutine> flightCoroutines = new List<Coroutine>();
    private bool hasFired = false;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    protected override IEnumerator move()
    {
        hasFired = true;
        StartFlightCoroutines();
        // 親クラス（Bullet_Base）のmoveコルーチンを待機
        yield return StartCoroutine(base.move());
    }

    private void StartFlightCoroutines()
    {
        StopFlightCoroutines();

        if (CreateBulletInfo == null) return;

        for (int i = 0; i < CreateBulletInfo.Length; i++)
        {
            if (CreateBulletInfo[i].timing == TriggerTiming.OnFlight)
            {
                flightCoroutines.Add(StartCoroutine(FlightShootCoroutine(CreateBulletInfo[i])));
            }
        }
    }

    private void StopFlightCoroutines()
    {
        foreach (var c in flightCoroutines)
        {
            if (c != null) StopCoroutine(c);
        }
        flightCoroutines.Clear();
    }

    private IEnumerator FlightShootCoroutine(ChildrenBulletInfo info)
    {
        // 弾が動き出すまで少し待つ
        yield return new WaitForEndOfFrame();

        while (true)
        {
            float waitTime = info.flightInterval > 0f ? info.flightInterval : 1f;
            yield return new WaitForSeconds(waitTime);

            SpawnChildBullet(info);
        }
    }

    private void SpawnChildBullet(ChildrenBulletInfo info)
    {
        if (info.bulletPrefab == null) return;

        // 子弾の発射位置計算（親の向きを考慮）
        Vector3 spawnPos = transform.position + (transform.rotation * info.offset);

        GameObject bObj = null;
        if (Alpha_ObjectPoolManager.Instance != null)
        {
            bObj = Alpha_ObjectPoolManager.Instance.Rent(info.bulletPrefab, spawnPos, Quaternion.identity);
        }
        else
        {
            bObj = Instantiate(info.bulletPrefab, spawnPos, Quaternion.identity);
        }

        if (bObj != null)
        {
            Bullet_Base childBullet = bObj.GetComponent<Bullet_Base>();
            if (childBullet != null)
            {
                // 親弾の情報を引き継ぐ（プレイヤーの弾か、敵の弾か）
                childBullet.isEnemyBullet = this.isEnemyBullet;
                childBullet.canHitBoth = this.canHitBoth;

                // 新しい角度と速度とダメージを設定
                float childDmg = this.dmg * (info.damageMultiplier > 0f ? info.damageMultiplier : 1f);
                
                // 親の向き(rotate)を基準に回転させる
                Vector3 childDir = Quaternion.Euler(info.rotate) * this.rotate.normalized;
                if (childDir.sqrMagnitude < 0.001f) childDir = Vector3.up;

                childBullet.setStatus(childDir, info.speed, childDmg);
                childBullet.setRotate(childDir);
                childBullet.shoot();
            }
        }
    }

    public override void OnReturnToPool()
    {
        if (hasFired)
        {
            ExecuteDestroyAction();
            hasFired = false;
        }
        StopFlightCoroutines();
        base.OnReturnToPool();
    }

    public override void DestroyAction()
    {
        if (hasFired)
        {
            ExecuteDestroyAction();
            hasFired = false;
        }
        base.DestroyAction(); // ここでDestroy(gameObject)が呼ばれる
    }

    private void ExecuteDestroyAction()
    {
        if (CreateBulletInfo != null)
        {
            for (int i = 0; i < CreateBulletInfo.Length; i++)
            {
                if (CreateBulletInfo[i].timing == TriggerTiming.OnDestroy)
                {
                    SpawnChildBullet(CreateBulletInfo[i]);
                }
            }
        }
    }
}
