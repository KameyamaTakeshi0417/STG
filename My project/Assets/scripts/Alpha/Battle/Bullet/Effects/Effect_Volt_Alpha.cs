using System.Collections.Generic;
using UnityEngine;

public class Effect_Volt_Alpha : Alpha_Effect_Base
{
    private GameObject voltAreaPrefab;

    public Effect_Volt_Alpha(BASE_WeaponData_Alpha data, int pos) : base(data, pos) 
    {
        // 航行中の展開間隔。例えば0.5秒おきに帯電領域を落としながら進む
        flightEffectInterval = 0.5f;

        // リソース等から帯電領域プレハブをロード（仮のパスや名前。必要に応じて修正してください）
        voltAreaPrefab = Resources.Load<GameObject>("Objects/Effect_Volt");
    }

    public override void Setup(Bullet_Base bullet, playerStatusManager_Alpha playerStatus)
    {
        if (playerStatus != null)
        {
            // バフ等で航行中の雷配置間隔を短縮
            flightEffectInterval = 0.5f * playerStatus.BulletSpanMag;
        }
    }

    protected override void DoFireEffect(Bullet_Base bullet)
    {
        // 生成時に足元に帯電領域を展開
        SpawnVoltArea(bullet.transform.position, bullet.dmg, bullet.getRarelity());
    }

    protected override void DoFlightEffect(Bullet_Base bullet)
    {
        // 航行中にポロポロと帯電領域を展開
        SpawnVoltArea(bullet.transform.position, bullet.dmg, bullet.getRarelity());
    }

    protected override void DoHitEffect(Bullet_Base bullet, Collider2D target)
    {
        // 着弾時にも帯電領域を展開
        SpawnVoltArea(bullet.transform.position, bullet.dmg, bullet.getRarelity());
    }

    private void SpawnVoltArea(Vector3 position, float dmg, int rarity)
    {
        // xは装備のレアリティに等しい
        int voltLevelX = rarity;
        
        // 最低1はないと全く連鎖・付与されないため、0以下の場合は補正（任意）
        if (voltLevelX <= 0) voltLevelX = 1;

        if (voltAreaPrefab != null)
        {
            GameObject obj = null;
            if (Alpha_ObjectPoolManager.Instance != null)
            {
                obj = Alpha_ObjectPoolManager.Instance.Rent(voltAreaPrefab, position, Quaternion.identity);
            }
            else
            {
                obj = GameObject.Instantiate(voltAreaPrefab, position, Quaternion.identity);
            }
            
            // 帯電領域側にダメージとVoltレベルを渡す
            Alpha_VoltArea areaScript = obj.GetComponent<Alpha_VoltArea>();
            if (areaScript != null)
            {
                areaScript.sourcePrefab = voltAreaPrefab; // プール用
                areaScript.ActivateVoltArea(dmg, voltLevelX);
            }
        }
        else
        {
            Debug.LogWarning("Alpha_VoltArea のプレハブが見つかりません。Resources/Objects/Effect_VoltArea_Alpha を作成・確認してください。");
        }
    }
}
