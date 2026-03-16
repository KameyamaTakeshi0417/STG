using UnityEngine;

// 敵キャラクター「Scareclaw」の専用スクリプト。Alpha_Enemy_Movementを継承しているため、移動処理は不要でそのまま使える。
public class Alpha_Enemy_Scareclaw : Alpha_Enemy_Movement
{
    protected override void Start()
    {
        // 親クラス（Alpha_Enemy_Movement）のStartを呼び、初期位置を確実にとる
        base.Start();
        
        // Scareclaw特有のステータス初期化などが必要な場合はここに記述
    }

    protected override void Update()
    {
        // 親クラスのUpdateを呼び、移動や帰還処理などを完全に任せる
        base.Update();
        
        // Scareclawとして、移動しながら別の行動（弾を撃つ、プレイヤーを監視するなど）
        // をさせたい場合はここに処理を追記できます
    }
}
