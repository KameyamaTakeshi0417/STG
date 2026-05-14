using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : _Health_Base
{
    public bool isInvincible = false;
    
    // マネージャーの参照をキャッシュ
    private playerStatusManager_Alpha statusManager;

    void Start()
    {
        // Managerオブジェクトからステータスマネージャーを取得する
        GameObject managerObj = GameObject.Find("manager");
        if (managerObj != null)
        {
            statusManager = managerObj.GetComponent<playerStatusManager_Alpha>();
        }
        else
        {
            Debug.LogError("[PlayerHealth] 'manager' GameObject not found in the scene!");
        }

        // 初期化時にマネージャー側のHPでローカル変数を同期しておく
        if (statusManager != null)
        {
            HP = statusManager.HP;
            currentHP = statusManager.currentHP;
        }
    }

    protected override void Update()
    {
        base.Update(); // _Health_Base側の無敵時間などのカウントダウン処理を実行

        // マネージャー側の実HPと常に同期させておく
        // （他スクリプトが_Health_Baseとして現在のHPを参照した場合の齟齬防止）
        if (statusManager != null)
        {
            HP = statusManager.HP;
            currentHP = statusManager.currentHP;
        }
    }

    public override void TakeDamage(float damage)
    {
        if (isInvincible) return; // 無敵中ならダメージを無視する

        float setDmg = damage;
        // 弱点倍率などローカル（自身）の状態に基づく計算
        if (VulnerableFlg)
        {
            setDmg *= 1.5f;
        }
        
        // 実際のダメージ適用はすべて一元管理しているマネージャーへ委譲
        if (statusManager != null)
        {
            statusManager.ApplyDamage(setDmg);
        }
        else
        {
            Debug.LogWarning("[PlayerHealth] StatusManager is missing. Cannot apply damage.");
        }
    }
}
