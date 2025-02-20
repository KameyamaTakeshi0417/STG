using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStatusManager : MonoBehaviour
{
    private static PlayerStatusManager instance; // Singletonインスタンス
    private bool isPaused = false;

    public float HP;
    public float currentHP;

    public float pow;
    public float DamageAdd = 0.0f; //バフとかで増やす値
    public float DamageMag = 1.0f; //非固定ダメージの倍率
    public float BlockDmg = 0f; //ダメージ軽減数値
    public float BlockMag = 1f; //ダメージ軽減倍率

    public float moveSpeed;
    public float moveSpeedMag = 1f;
    public float bulletSpeed;
    public float bulletSpeedMag = 1.0f;
    public float BulletSpan; // フレーム
    public float BulletSpanMag = 1.0f;
    public float lockOnRadius = 5f; // ロックオンの半径
    public int Exp;

    // Start is called before the first frame update
    void Start() { }

    void Awake()
    {
        // Singletonパターンの実装
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // シーンをまたいでもオブジェクトを破棄しない
        }
        else if (instance != this)
        {
            Destroy(gameObject); // 既存のインスタンスがある場合、新しいインスタンスを破棄
        }
    }

    // Update is called once per frame
    void Update() { }

    public void getStatus(GameObject targetObj)
    {
        Player targetScript = targetObj.GetComponent<Player>();
        //数字入れる処理
    }

    public void setStatus(GameObject targetObj)
    {
        Player targetScript = targetObj.GetComponent<Player>();
        //数字入れる処理
    }
}
