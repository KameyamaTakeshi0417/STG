using UnityEngine;

// 椿弾用のカスタムバレットクラス
// 実際の挙動（3秒滞留・拡大・ターゲットロック・無減衰・動き止めなど）は
// 全て Effect_Tsubaki_Alpha.cs とそれに付随するコントローラーが処理するため、
// ここはBullet_Baseを継承するだけのシンプルな構造となります。
public class TsubakiObject : Bullet_Base
{
    public override void DestroyAction()
    {
        base.DestroyAction();
        // 必要に応じて椿弾専用の消滅処理をここに記述できます
    }
}
