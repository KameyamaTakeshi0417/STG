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

    public enum AimType
    {
        Relative,    // 親弾の進行方向を基準にする
        Absolute,    // 世界座標の絶対角度（画面上方向が0度基準）
        AimAtPlayer  // プレイヤーの方向を狙う
    }

    public enum SpawnPositionType
    {
        Relative,    // 親弾の進行方向を基準にオフセット移動する
        Absolute     // 世界座標基準でそのままオフセット移動する
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

        [Header("Spawn Position")]
        [Tooltip("スポーン位置の基準")]
        public SpawnPositionType spawnPositionType;
        [Tooltip("発射位置のオフセット")]
        public Vector3 offset;

        [Header("Aim Direction")]
        [Tooltip("発射方向の基準")]
        public AimType aimType;
        [Tooltip("発射方向（AimTypeに合わせて加算される角度）")]
        public Vector3 rotate;

        // 以前の変数（互換性とインスペクタデータの保持用）
        [HideInInspector] public List<EnemyBehaviorData_Base> behaviorsToRunInParallel;
    }

    [Header("Rows (1D struct array)")]
    [SerializeField] private ChildrenBulletInfo[] CreateBulletInfo; // これがセットされていたら行数分処理する

    [Header("Safety Settings")]
    [Tooltip("子弾に自分自身(RichBullet)を指定した場合の最大分裂回数（無限増殖によるフリーズ防止）")]
    public int maxGeneration = 1;
    [HideInInspector] public int currentGeneration = 0;

    private List<Coroutine> flightCoroutines = new List<Coroutine>();
    private bool hasFired = false;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
    }

    public override void OnRentFromPool()
    {
        base.OnRentFromPool();
        currentGeneration = 0; // プール再利用時に世代をリセット（生成元から上書きされる想定）
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
        if (currentGeneration >= maxGeneration) return; // 世代上限に達していたら航行中発射しない

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

        // 子弾の発射位置計算
        Vector3 spawnPos = transform.position;
        if (info.spawnPositionType == SpawnPositionType.Relative)
        {
            // 親の向きを基準にオフセットを加算
            spawnPos += (transform.rotation * info.offset);
        }
        else
        {
            // 絶対座標でオフセットを加算（親の回転を無視）
            spawnPos += info.offset;
        }

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

                // もし子弾もRichBullet_Baseなら、世代を+1して引き継ぐ
                RichBullet_Base richChild = childBullet as RichBullet_Base;
                if (richChild != null)
                {
                    richChild.currentGeneration = this.currentGeneration + 1;
                    richChild.maxGeneration = this.maxGeneration;
                }

                // 新しい速度とダメージを設定
                float childDmg = this.dmg * (info.damageMultiplier > 0f ? info.damageMultiplier : 1f);
                
                // 子弾の向き(rotate)を決定する
                Vector3 childDir = Vector3.up;

                if (info.aimType == AimType.Relative)
                {
                    // 親の向き(rotate)を基準に回転させる
                    childDir = Quaternion.Euler(info.rotate) * this.rotate.normalized;
                }
                else if (info.aimType == AimType.Absolute)
                {
                    // 世界座標の上（Vector3.up）を基準に絶対角度を指定する
                    childDir = Quaternion.Euler(info.rotate) * Vector3.up;
                }
                else if (info.aimType == AimType.AimAtPlayer)
                {
                    // プレイヤーの方向を狙う
                    GameObject player = GameObject.FindWithTag("Player");
                    if (player != null)
                    {
                        Vector3 dirToPlayer = (player.transform.position - spawnPos).normalized;
                        // rotateを「プレイヤー方向からのズレ（オフセット角度）」として適用
                        childDir = Quaternion.Euler(info.rotate) * dirToPlayer;
                    }
                    else
                    {
                        // プレイヤーがいない場合はRelativeと同じ挙動にする
                        childDir = Quaternion.Euler(info.rotate) * this.rotate.normalized;
                    }
                }

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
        if (currentGeneration >= maxGeneration) return; // 世代上限に達していたら分裂しない

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
