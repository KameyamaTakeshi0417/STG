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
}
