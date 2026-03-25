using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PiercingBullet : Bullet_Base
{
    // Start is called before the first frame update


    void Awake()
    {
        // プレハブなどから生成された直後に初期化
        piercingCount = rarelity + 1;
    }

    public override void OnRentFromPool()
    {
        base.OnRentFromPool(); // 先に0リセットなどのベース処理を行う
        
        // オブジェクトプールから再利用されるたびに、貫通弾本来の回数を再度設定する
        piercingCount = rarelity + 1;
    }
}
