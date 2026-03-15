using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerStatusManager_Alpha : ObjectStatus_Alpha
{
    // Start is called before the first frame update
    public float moveSpeed;
    public float moveSpeedMag = 1f;
    public float moveSpeedMag_CONST = 1.0f;
    public float bulletSpeed;
    public float bulletSpeedMag = 1.0f;
    public float BulletSpan; // フレーム
    public float BulletSpanMag = 1.0f;

    public int Exp;

    [Header("Weapon Synthesis Settings")]
    [Tooltip("特定条件下で1〜3行目のすべての武器効果を使用可能にするフラグ")]
    public bool canUseAllEffects = false;

    [Header("Pierce Settings")]
    [Tooltip("貫通時のダメージ減衰率（デフォルト25%減）。アイテム等で0.10fなどに変動")]
    public float pierceDamageReductionRate = 0.25f;
}
