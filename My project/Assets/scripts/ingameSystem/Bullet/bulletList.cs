using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bulletList : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
public static class BulletListContainer
{
    // 静的なList<string>を定義し、初期値を設定
    public static List<string> stringList = new List<string>
    {
        "NormalBullet",
        "PiercingBullet",
        "HomingBullet",
        "BloomBullet",
        "ExplosionBullet",
        "GravityBullet",
        // 必要に応じて追加
    };
}