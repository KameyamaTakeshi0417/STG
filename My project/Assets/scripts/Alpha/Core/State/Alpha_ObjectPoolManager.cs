using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// プール可能なオブジェクトが必ず実装するインターフェース（再利用時の初期化用）
public interface IAlphaPoolable
{
    // プールから取り出される直前に呼ばれる（ステータスやタイマーのリセットなど）
    void OnRentFromPool();
    // プールに返却される直前に呼ばれる（エフェクトの停止など）
    void OnReturnToPool();
}

public class Alpha_ObjectPoolManager : MonoBehaviour
{
    public static Alpha_ObjectPoolManager Instance { get; private set; }

    [System.Serializable]
    public class PoolSetting
    {
        public GameObject prefab;    // プールするプレハブ
        public int initialPoolSize;  // はじめに作っておく数
        [HideInInspector]
        public Transform parentNode; // Hierarchyを整理するための親オブジェクト
    }

    [Header("Pool Settings (Inspectorで事前登録)")]
    [Tooltip("よく使うプレハブ（通常弾、爆発、帯電領域など）を登録")]
    public List<PoolSetting> initialPools;

    // プレハブのInstanceIDをキーにして、利用可能な非アクティブオブジェクトを管理する辞書
    private Dictionary<int, Queue<GameObject>> poolDictionary = new Dictionary<int, Queue<GameObject>>();

    private void Awake()
    {
        // シングルトン化＆シーン間保持 (DontDestroyOnLoad)
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // シーン遷移時に不要なゴミを消すためのイベント登録
        SceneManager.sceneLoaded += OnSceneLoaded;

        // 初期プールの生成
        InitializePools();
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // もし「このシーン専用のプールオブジェクト」等があり、消したい場合はここでリセット処理を入れることが可能。
        // （通常、DontDestroyOnLoad配下にあるものはそのまま使い回すので空で良い）
    }

    // 起動時に事前登録されたプレハブを初めから生成しておく
    private void InitializePools()
    {
        foreach (var setting in initialPools)
        {
            if (setting.prefab == null) continue;

            int prefabId = setting.prefab.GetInstanceID();
            
            // 辞書に未登録なら新しくQueueを作る
            if (!poolDictionary.ContainsKey(prefabId))
            {
                poolDictionary.Add(prefabId, new Queue<GameObject>());
            }

            // Hierarchyが散らからないように、プレハブ名ごとの親フォルダを作る
            GameObject folder = new GameObject($"Pool_{setting.prefab.name}");
            folder.transform.SetParent(this.transform);
            setting.parentNode = folder.transform;

            for (int i = 0; i < setting.initialPoolSize; i++)
            {
                GameObject newObj = Instantiate(setting.prefab, setting.parentNode);
                newObj.SetActive(false);
                poolDictionary[prefabId].Enqueue(newObj);
            }
        }
    }

    // ★重要: Instantiateの代わりに使うメソッド
    public GameObject Rent(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        if (prefab == null) return null;

        int prefabId = prefab.GetInstanceID();

        // 辞書が存在しなかった場合（事前登録されていないプレハブが不意に要求された場合）
        if (!poolDictionary.ContainsKey(prefabId))
        {
            poolDictionary.Add(prefabId, new Queue<GameObject>());
            
            // 新規フォルダも作る
            GameObject folder = new GameObject($"Pool_Auto_{prefab.name}");
            folder.transform.SetParent(this.transform);
        }

        Queue<GameObject> queue = poolDictionary[prefabId];
        GameObject objToRent = null;

        if (queue.Count > 0)
        {
            // プールに空き（非表示）があれば取り出す
            objToRent = queue.Dequeue();
        }
        else
        {
            // プールが空の場合は仕方なく新規生成する（動的拡張）
            // （本来はここで生成されないように initialPoolSize を大きめに設定しておくのが理想）
            Transform parentNode = transform.Find($"Pool_{prefab.name}") ?? transform.Find($"Pool_Auto_{prefab.name}");
            objToRent = Instantiate(prefab, parentNode);
            Debug.Log($"<color=yellow>プールが枯渇したため、{prefab.name} を追加生成しました(動的拡張)</color>");
        }

        // 位置・回転のセット
        objToRent.transform.position = position;
        objToRent.transform.rotation = rotation;

        // オブジェクトが IAlphaPoolable を持っていたら、内部ステータスをリセットさせる
        IAlphaPoolable[] poolables = objToRent.GetComponentsInChildren<IAlphaPoolable>(true);
        foreach (var p in poolables)
        {
            p.OnRentFromPool();
        }

        // アクティブにしてゲーム内に登場させる
        objToRent.SetActive(true);

        return objToRent;
    }

    // ★重要: Destroyの代わりに使うメソッド
    public void Return(GameObject obj, GameObject originalPrefab)
    {
        if (originalPrefab == null || obj == null) return;

        int prefabId = originalPrefab.GetInstanceID();

        // オブジェクトを非表示にする
        obj.SetActive(false);

        // オブジェクトが IAlphaPoolable を持っていたら、返却時の処理を行わせる
        IAlphaPoolable[] poolables = obj.GetComponentsInChildren<IAlphaPoolable>(true);
        foreach (var p in poolables)
        {
            p.OnReturnToPool();
        }

        // プールに返却（ただし親フォルダがDestoryされている等で辞書が無ければ本当に破棄する）
        if (poolDictionary.ContainsKey(prefabId))
        {
            poolDictionary[prefabId].Enqueue(obj);
        }
        else
        {
            Destroy(obj);
        }
    }
}
