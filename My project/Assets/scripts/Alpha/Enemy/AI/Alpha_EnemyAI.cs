using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Alpha_EnemyAI : MonoBehaviour
{
    [Header("Behavior Settings")]
    [Tooltip("起動時に実行する初期の挙動")]
    public EnemyBehaviorData_Base initialBehavior;

    // AIの現在の状態（挙動など）
    public EnemyBehaviorData_Base CurrentBehavior { get; protected set; }

    // キャッシュされたコンポーネント
    public Rigidbody2D Rb { get; protected set; }
    public Transform TargetTransform { get; protected set; }
    
    // 生成時などの初期位置（距離維持や元の位置へ戻る処理などで使用可能）
    public Vector3 InitialPosition { get; protected set; }

    private Coroutine activeBehaviorCoroutine;

    // そのフェーズで召喚したオブジェクトを管理するリスト
    [HideInInspector]
    public System.Collections.Generic.List<GameObject> PhaseSpawnedObjects = new System.Collections.Generic.List<GameObject>();

    protected virtual void Awake()
    {
        Rb = GetComponent<Rigidbody2D>();
    }

    protected virtual void Start()
    {
        InitialPosition = transform.position;
        
        // プレイヤーのTransformを取得
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
        {
            TargetTransform = playerObj.transform;
        }

        if (initialBehavior != null)
        {
            ChangeBehavior(initialBehavior);
        }
    }

    // 挙動を安全に切り替えるメソッド（外部や挙動自身から呼び出し可能）
    public void ChangeBehavior(EnemyBehaviorData_Base newBehavior)
    {
        if (newBehavior == null) return;

        // 実行中の挙動があれば停止する
        if (activeBehaviorCoroutine != null)
        {
            StopCoroutine(activeBehaviorCoroutine);
            activeBehaviorCoroutine = null;
        }

        // 新しい挙動を開始する
        CurrentBehavior = newBehavior;
        activeBehaviorCoroutine = StartCoroutine(CurrentBehavior.RunBehavior(this));
        
        // 挙動切り替え時に速度を一旦リセット（慣性を残したい場合は要調整）
        Rb.velocity = Vector2.zero;
    }

    // 必要に応じたユーティリティメソッド群
    public bool HasTarget()
    {
        return TargetTransform != null && TargetTransform.gameObject.activeInHierarchy;
    }

    // 召喚したオブジェクトを一括破棄・返却するメソッド
    public void ClearSpawnedObjects()
    {
        foreach (var obj in PhaseSpawnedObjects)
        {
            if (obj != null)
            {
                // プール管理されている場合は Return() が理想ですが、
                // IAlphaPoolable などを利用しているオブジェクトは SetActive(false) または Destroy でプールに戻るか消滅します
                // ここでは安全にDestroyを呼びます。もしプール機構が対応していれば適宜変更してください。
                if (obj.activeInHierarchy)
                {
                    // オブジェクトプール機構があれば対応する処理、ここではDestroyで破棄
                    Destroy(obj);
                }
            }
        }
        PhaseSpawnedObjects.Clear();
    }

    protected virtual void OnDestroy()
    {
        // 自身が破棄される（＝死亡する）際にも、道連れとして残っている召喚物を全消去する
        ClearSpawnedObjects();
    }
}
