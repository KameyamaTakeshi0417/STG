using UnityEngine;

// 敵キャラクター「Scareclaw」の専用スクリプト。新しい Alpha_EnemyAI を継承しているため、
// 移動処理はInspectorでセットした Behavior Data (ScriptableObject) に委譲されます。
public class Alpha_Enemy_Scareclaw : Alpha_EnemyAI
{
    protected override void Start()
    {
        // 親クラス（Alpha_EnemyAI）のStartを呼び、ターゲット取得や初期挙動を起動する
        base.Start();
        
        // Scareclaw特有のステータス初期化などが必要な場合はここに記述
    }

    // 移動処理はすべて Alpha_EnemyAI のコルーチンに一任されているため、
    // ここで Update() をオーバーライドして移動処理を書く必要はありません。
}
