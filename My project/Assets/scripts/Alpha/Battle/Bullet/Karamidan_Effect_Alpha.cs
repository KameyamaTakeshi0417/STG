using UnityEngine;

namespace Alpha.Battle.Bullet
{
    public class Karamidan_Effect_Alpha : Alpha_Effect_Base
    {
        // 外部（EffectFactory）から生成される際に呼ばれる
        public Karamidan_Effect_Alpha(int pos, int rarity = 1) : base(pos, rarity)
        {
            // 薬莢（インデックス1）の場合は、航行中に断続的にダメージエリアを発生させるためインターバルを設定
            if (pos == 1)
            {
                flightEffectInterval = 0.5f; // 0.5秒おきに軌道上にエリア生成
            }
        }

        protected override void DoFireEffect(Bullet_Base bullet)
        {
            // 雷管（インデックス2）装備時: プレイヤー周囲にダメージエリア生成
            if (equipPosition == 2)
            {
                Debug.Log($"[Karamidan] 雷管発動！プレイヤー周囲にダメージエリアを生成します (Rarity: {rarity})");
                // TODO: 実際のダメージエリアPrefabを bullet.transform.position （発射位置＝プレイヤー周辺）に生成する処理
            }
        }

        protected override void DoFlightEffect(Bullet_Base bullet)
        {
            // 薬莢（インデックス1）装備時: 軌道跡に長時間残るダメージエリア生成
            if (equipPosition == 1)
            {
                Debug.Log($"[Karamidan] 薬莢発動！軌道上に長時間ダメージエリアを生成します (Rarity: {rarity})");
                // TODO: 実際のダメージエリアPrefabを bullet.transform.position に生成する処理
            }
        }

        protected override void DoHitEffect(Bullet_Base bullet, Collider2D target)
        {
            // 弾頭（インデックス0）装備時: 着弾地点に短時間ダメージエリア生成
            if (equipPosition == 0)
            {
                string targetName = target != null ? target.name : "壁または寿命";
                Debug.Log($"[Karamidan] 弾頭発動！{targetName} の位置に短時間ダメージエリアを生成します (Rarity: {rarity})");
                // TODO: 実際のダメージエリアPrefabを bullet.transform.position に生成する処理
            }
        }
    }
}
