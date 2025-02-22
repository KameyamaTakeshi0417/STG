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
        //シーン読み込み時のステータス更新
        PlayerHealth targetScript = GameObject.Find("Player").GetComponent<PlayerHealth>();
        LoadStatus(GameObject.Find("Player"));
    }

    // Update is called once per frame
    void Update() { }

    public void getStatus(GameObject targetObj)
    {
        PlayerHealth playerHPScript = targetObj.GetComponent<PlayerHealth>();
        Player playerStatusScript = targetObj.GetComponent<Player>();
        //数字入れる処理
    }

    public void saveStatus(GameObject targetObj)
    {
        PlayerHealth playerHPScript = targetObj.GetComponent<PlayerHealth>();
        Player playerStatusScript = targetObj.GetComponent<Player>();
        //数字入れる処理
        HP = playerHPScript.getHP();
        currentHP = playerHPScript.getCurrentHP();

        pow = playerStatusScript.pow;
        DamageAdd = playerStatusScript.DamageAdd; // バフとかで増やす値
        DamageMag = playerStatusScript.DamageMag; // 追加したい変数
        BlockDmg = playerStatusScript.BlockDmg; // 追加したい変数
        BlockMag = playerStatusScript.BlockMag; // 追加したい変数

        moveSpeed = playerStatusScript.moveSpeed;
        moveSpeedMag = playerStatusScript.moveSpeedMag;
        bulletSpeed = playerStatusScript.bulletSpeed;
        bulletSpeedMag = playerStatusScript.bulletSpeedMag;
        BulletSpan = playerStatusScript.BulletSpan;
        BulletSpanMag = playerStatusScript.BulletSpanMag;
        lockOnRadius = playerStatusScript.lockOnRadius;
        Exp = playerStatusScript.Exp;
    }

    public void LoadStatus(GameObject targetObj)
    {
        PlayerHealth playerHPScript = targetObj.GetComponent<PlayerHealth>();
        Player playerStatusScript = targetObj.GetComponent<Player>();

        // 保存されているデータを対応するプレイヤーのスクリプトにセット
        playerHPScript.setPlayerHP(HP, currentHP);
        playerStatusScript.pow = pow;
        playerStatusScript.DamageAdd = DamageAdd;
        playerStatusScript.DamageMag = DamageMag;
        playerStatusScript.BlockDmg = BlockDmg;
        playerStatusScript.BlockMag = BlockMag;

        playerStatusScript.moveSpeed = moveSpeed;
        playerStatusScript.moveSpeedMag = moveSpeedMag;
        playerStatusScript.bulletSpeed = bulletSpeed;
        playerStatusScript.bulletSpeedMag = bulletSpeedMag;
        playerStatusScript.BulletSpan = BulletSpan;
        playerStatusScript.BulletSpanMag = BulletSpanMag;
        playerStatusScript.lockOnRadius = lockOnRadius;
        playerStatusScript.Exp = Exp;
    }
}
