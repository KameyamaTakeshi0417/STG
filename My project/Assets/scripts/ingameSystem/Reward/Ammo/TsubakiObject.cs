using UnityEngine;
using Alpha.Core;

// 椿弾用のカスタムバレットクラス
// 実際の挙動（3秒滞留・拡大・ターゲットロック・無減衰・動き止めなど）は
// 全て Effect_Tsubaki_Alpha.cs とそれに付随するコントローラーが処理するため、
// ここはBullet_Baseを継承するシンプルな構造となります。
public class TsubakiObject : Bullet_Base
{
    protected override void Awake()
    {
        base.Awake();
        // 椿の花弁チリ軌跡エミッターを自動追加
        if (GetComponent<ProceduralCamelliaTrailEmitter>() == null)
        {
            gameObject.AddComponent<ProceduralCamelliaTrailEmitter>();
        }
    }

    public override void DestroyAction()
    {
        base.DestroyAction();
    }
}
