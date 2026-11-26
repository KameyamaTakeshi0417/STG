using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Alpha.Enemy.Chicory
{
    /// <summary>
    /// 三女・チコリのボス統括コントローラー
    /// 決着時（HP0）：派手な爆破ではなく「チコリが自ら暴走を抑え込んだ」演出として、
    /// フィールド上の全ライン・地雷・弾幕をピタッと静止させ、ダメージ判定をオフにして青い光の粒へと収束消滅させる。
    /// </summary>
    public class ChicoryBossController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Alpha_EliteHealth eliteHealth;
        [SerializeField] private Alpha_EliteEnemyAI eliteAI;

        private bool hasTriggeredCalmDown = false;

        private void Awake()
        {
            if (eliteHealth == null) eliteHealth = GetComponent<Alpha_EliteHealth>();
            if (eliteAI == null) eliteAI = GetComponent<Alpha_EliteEnemyAI>();
        }

        private void Update()
        {
            // ボスが撃破・戦闘終了状態になったかを検知
            if (!hasTriggeredCalmDown && eliteHealth != null && eliteHealth.isDead)
            {
                TriggerSelfSuppressionSequence();
            }
        }

        /// <summary>
        /// チコリが自ら暴走している感情を抑え込む決着演出
        /// </summary>
        public void TriggerSelfSuppressionSequence()
        {
            if (hasTriggeredCalmDown) return;
            hasTriggeredCalmDown = true;

            // 1. フィールド上の全「根ライン」をピタッと止めて判定オフ＆収束消滅
            ChicoryRootLine[] lines = FindObjectsOfType<ChicoryRootLine>();
            foreach (var line in lines)
            {
                if (line != null) line.ForceStopAndVanish();
            }

            // 2. フィールド上の全「花時計地雷」を即時停止＆消滅
            ChicoryClockMine[] mines = FindObjectsOfType<ChicoryClockMine>();
            foreach (var mine in mines)
            {
                if (mine != null) mine.ForceStopAndVanish();
            }

            // 3. 敵弾（栞弾など）のダメージ判定オフ＆消去
            Bullet_Base[] bullets = FindObjectsOfType<Bullet_Base>();
            foreach (var bullet in bullets)
            {
                if (bullet != null && bullet.isEnemyBullet && bullet.gameObject.activeInHierarchy)
                {
                    Collider2D col = bullet.GetComponent<Collider2D>();
                    if (col != null) col.enabled = false;

                    if (Alpha_ObjectPoolManager.Instance != null && bullet.sourcePrefab != null)
                    {
                        Alpha_ObjectPoolManager.Instance.Return(bullet.gameObject, bullet.sourcePrefab);
                    }
                    else
                    {
                        Destroy(bullet.gameObject);
                    }
                }
            }

            // 4. ボスAIの全コルーチンを停止
            if (eliteAI != null)
            {
                eliteAI.StopAllBehaviors();
            }
        }
    }
}
