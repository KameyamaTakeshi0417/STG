using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Alpha_EnemyAI : MonoBehaviour
{
    public enum BehaviorSlot
    {
        Movement,
        Attack,
        Summon
    }

    [Header("Behavior Settings (Max 3 Parallel)")]
    [Tooltip("起動時に実行する移動挙動（旧 initialBehavior 相当）")]
    public EnemyBehaviorData_Base initialMovementBehavior;

    [Tooltip("起動時に実行する攻撃挙動（任意）")]
    public EnemyBehaviorData_Base initialAttackBehavior;

    [Tooltip("起動時に実行する召喚/特殊挙動（任意）")]
    public EnemyBehaviorData_Base initialSummonBehavior;

    // 現在実行中の挙動（スロット別）
    public EnemyBehaviorData_Base CurrentMovementBehavior { get; protected set; }
    public EnemyBehaviorData_Base CurrentAttackBehavior { get; protected set; }
    public EnemyBehaviorData_Base CurrentSummonBehavior { get; protected set; }

    [Tooltip("スポーン直後に2秒間の無敵待機時間を設けるか（エリートやボス用）")]
    public bool spawnWithDelay = false;

    [Tooltip("スポーン直後の行動開始までの待機時間（秒）。0より大きい場合、この時間だけ待機してから行動を開始します。（spawnWithDelayが優先されます）")]
    public float startDelayTime = 1f;

    // キャッシュされたコンポーネント
    public Rigidbody2D Rb { get; protected set; }
    public Transform TargetTransform { get; protected set; }
    public Vector3 InitialPosition { get; protected set; }

    // スロット別コルーチン
    private Coroutine movementCoroutine;
    private Coroutine attackCoroutine;
    private Coroutine summonCoroutine;

    // そのフェーズ/行動で召喚したオブジェクトを管理するリスト
    [HideInInspector]
    public List<GameObject> PhaseSpawnedObjects = new List<GameObject>();

    private static bool _layerCollisionIgnored = false;

    protected virtual void Awake()
    {
        Rb = GetComponent<Rigidbody2D>();
        // エネミー同士の衝突を防ぐため、強制的に enemy レイヤーに設定する
        int enemyLayer = LayerMask.NameToLayer("enemy");
        if (enemyLayer >= 0)
        {
            gameObject.layer = enemyLayer;
            // 子オブジェクトにアタッチされているコライダーのレイヤーも全て変更する（これが原因で干渉することが多いです）
            var colliders = GetComponentsInChildren<Collider2D>(true);
            foreach (var col in colliders)
            {
                col.gameObject.layer = enemyLayer;
            }

            // エネミーレイヤー同士の衝突を物理エンジンレベルで無効化する
            if (!_layerCollisionIgnored)
            {
                Physics2D.IgnoreLayerCollision(enemyLayer, enemyLayer, true);
                _layerCollisionIgnored = true;
            }
        }
    }

    protected virtual void Start()
    {
        InitialPosition = transform.position;

        // プレイヤーのTransformを取得
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
        {
            TargetTransform = playerObj.transform;
            IgnorePlayerSolidCollisions(playerObj);
        }

        // フラグが立っている場合は2秒間無敵待機してから行動開始
        if (spawnWithDelay)
        {
            Debug.Log("[Alpha_EnemyAI] New code running for " + gameObject.name);
            StartCoroutine(SpawnDelayRoutine());
        }
        else if (startDelayTime > 0f)
        {
            StartCoroutine(SimpleStartDelayRoutine());
        }
        else
        {
            // 初期挙動を開始（必要なスロットだけ）
            StartBehaviors(initialMovementBehavior, initialAttackBehavior, initialSummonBehavior);
        }
    }

    private IEnumerator SimpleStartDelayRoutine()
    {
        yield return new WaitForSeconds(startDelayTime);
        StartBehaviors(initialMovementBehavior, initialAttackBehavior, initialSummonBehavior);
    }

    private IEnumerator SpawnDelayRoutine()
    {
        var health = GetComponent<_Health_Base>();
        if (health != null) health.VulnerableFlg = true; // 無敵オン

        PlayerHealth playerHealth = null;
        if (TargetTransform != null)
        {
            playerHealth = TargetTransform.GetComponent<PlayerHealth>();
            if (playerHealth != null) playerHealth.isInvincible = true; // プレイヤーも無敵オン
        }

        yield return new WaitForSeconds(2f);

        if (health != null) health.VulnerableFlg = false; // 無敵オフ
        if (playerHealth != null) playerHealth.isInvincible = false;

        StartBehaviors(initialMovementBehavior, initialAttackBehavior, initialSummonBehavior);
    }

    private void IgnorePlayerSolidCollisions(GameObject playerObj)
    {
        // 自分（敵）のすべてのColliderを取得
        Collider2D[] myColliders = GetComponentsInChildren<Collider2D>();
        // プレイヤーのすべてのColliderを取得
        Collider2D[] playerColliders = playerObj.GetComponentsInChildren<Collider2D>();

        foreach (var myCol in myColliders)
        {
            // トリガー（攻撃判定など）は衝突を無視しない
            if (myCol.isTrigger) continue;

            foreach (var pCol in playerColliders)
            {
                // プレイヤー側のトリガーも衝突を無視しない
                if (pCol.isTrigger) continue;

                // お互いに「実体（Solid）」のコライダー同士だけ物理的な衝突判定を無効化する
                // これにより、物理的に押し合うことはなくなるが、突進などのTrigger判定は正常に動作する
                Physics2D.IgnoreCollision(myCol, pCol, true);
            }
        }
    }

    protected virtual void Update()
    {
        var health = GetComponent<_Health_Base>();
        if (health != null && health.isStunned)
        {
            if (Rb != null) Rb.velocity = Vector2.zero;
        }
    }

    /// <summary>
    /// 3スロットをまとめて開始（nullは開始しない）
    /// 既に動作中なら一旦停止してから開始
    /// </summary>
    public void StartBehaviors(
        EnemyBehaviorData_Base movement,
        EnemyBehaviorData_Base attack,
        EnemyBehaviorData_Base summon,
        bool clearSpawnedObjectsOnStop = false)
    {
        StopAllBehaviors(clearSpawnedObjectsOnStop);

        if (movement != null) StartBehavior(BehaviorSlot.Movement, movement);
        if (attack != null) StartBehavior(BehaviorSlot.Attack, attack);
        if (summon != null) StartBehavior(BehaviorSlot.Summon, summon);
    }

    /// <summary>
    /// 指定スロットの挙動を開始（既に動いていれば差し替え）
    /// </summary>
    public void StartBehavior(BehaviorSlot slot, EnemyBehaviorData_Base behavior)
    {
        if (behavior == null) return;

        StopBehavior(slot, clearSpawnedObjectsOnStop: false);

        switch (slot)
        {
            case BehaviorSlot.Movement:
                CurrentMovementBehavior = behavior;
                movementCoroutine = StartCoroutine(RunWithStunCheck(behavior.RunBehavior(this)));
                break;

            case BehaviorSlot.Attack:
                CurrentAttackBehavior = behavior;
                attackCoroutine = StartCoroutine(RunWithStunCheck(behavior.RunBehavior(this)));
                break;

            case BehaviorSlot.Summon:
                CurrentSummonBehavior = behavior;
                summonCoroutine = StartCoroutine(RunWithStunCheck(behavior.RunBehavior(this)));
                break;
        }
    }

    private System.Collections.IEnumerator RunWithStunCheck(System.Collections.IEnumerator coreRoutine)
    {
        var health = GetComponent<_Health_Base>();
        while (true)
        {
            // スタン中は元のコルーチンを進めずに待機
            if (health != null && health.isStunned)
            {
                yield return null;
                continue;
            }

            // 次のステップに進めるか確認
            if (!coreRoutine.MoveNext())
            {
                break; // 終了
            }

            // 元のコルーチンが返したyield instructionをそのまま返す
            yield return coreRoutine.Current;
        }
    }

    /// <summary>
    /// 指定スロットの挙動を停止（nullでも安全）
    /// </summary>
    public void StopBehavior(BehaviorSlot slot, bool clearSpawnedObjectsOnStop)
    {
        switch (slot)
        {
            case BehaviorSlot.Movement:
                if (movementCoroutine != null) StopCoroutine(movementCoroutine);
                movementCoroutine = null;
                CurrentMovementBehavior = null;
                break;

            case BehaviorSlot.Attack:
                if (attackCoroutine != null) StopCoroutine(attackCoroutine);
                attackCoroutine = null;
                CurrentAttackBehavior = null;
                break;

            case BehaviorSlot.Summon:
                if (summonCoroutine != null) StopCoroutine(summonCoroutine);
                summonCoroutine = null;
                CurrentSummonBehavior = null;
                break;
        }

        // 慣性を残したい場合はここを外す/条件化してください
        if (Rb != null) Rb.velocity = Vector2.zero;

        if (clearSpawnedObjectsOnStop)
        {
            ClearSpawnedObjects();
        }
    }

    /// <summary>
    /// 3スロットすべて停止
    /// </summary>
    public void StopAllBehaviors(bool clearSpawnedObjectsOnStop)
    {
        StopBehavior(BehaviorSlot.Movement, clearSpawnedObjectsOnStop: false);
        StopBehavior(BehaviorSlot.Attack, clearSpawnedObjectsOnStop: false);
        StopBehavior(BehaviorSlot.Summon, clearSpawnedObjectsOnStop: false);

        if (clearSpawnedObjectsOnStop)
        {
            ClearSpawnedObjects();
        }
    }

    // 互換用：旧 ChangeBehavior を Movementスロット差し替えとして残す
    // 既存コードから呼ばれても動くようにしておく
    public void ChangeBehavior(EnemyBehaviorData_Base newBehavior)
    {
        StartBehavior(BehaviorSlot.Movement, newBehavior);
    }

    public bool HasTarget()
    {
        return TargetTransform != null && TargetTransform.gameObject.activeInHierarchy;
    }

    public bool IsAnyBehaviorRunning()
    {
        return movementCoroutine != null || attackCoroutine != null || summonCoroutine != null;
    }

    // 召喚したオブジェクトを一括破棄・返却するメソッド
    public void ClearSpawnedObjects()
    {
        foreach (var obj in PhaseSpawnedObjects)
        {
            if (obj != null)
            {
                if (obj.activeInHierarchy)
                {
                    Destroy(obj);
                }
            }
        }
        PhaseSpawnedObjects.Clear();
    }
    
    /// <summary>
    /// カットイン中に無敵化/復帰させる
    /// </summary>
    /// <param name="flag">true = 無敵化, false = 復帰</param>
    public void SetInvulnerable(bool flag)
    {
        // 速度リセット
        if (Rb != null) Rb.velocity = Vector2.zero;

        // コライダー有効/無効切替
        var colliders = GetComponentsInChildren<Collider2D>();
        foreach (var col in colliders)
        {
            col.enabled = !flag;
        }

        // 攻撃ビヘイビア停止/再開
        if (flag)
        {
            // 現在の攻撃コルーチン停止
            if (attackCoroutine != null)
            {
                StopCoroutine(attackCoroutine);
                attackCoroutine = null;
                CurrentAttackBehavior = null;
            }
        }
    }

    protected virtual void OnDestroy()
    {
        StopAllBehaviors(clearSpawnedObjectsOnStop: true);
        ClearSpawnedObjects();
    }
}

