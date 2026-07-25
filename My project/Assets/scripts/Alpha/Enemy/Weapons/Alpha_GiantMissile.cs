using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Alpha.Enemy.Weapons
{
    public class Alpha_GiantMissile : Health
    {
        public float fallDuration = 5f;
        public Vector2 startOffset = new Vector2(0, 15f);
        public Vector2 targetPosition; // 地面や落下完了座標

        public delegate void MissileResultHandler(bool isDestroyedByPlayer);
        public event MissileResultHandler OnMissileEnd;

        private float elapsed = 0f;
        private bool hasFinished = false;
        private Vector3 initialPos;

        protected virtual void Start()
        {
            // Health.csのUI化や設定が走りすぎないよう、最小限で動かす
            // オフセットから目標に向けてゆっくり落下
            initialPos = transform.position;
            currentHP = HP;
            setSlideHPBar(); // スライダーHPをアクティブ化
        }

        void Update()
        {
            if (hasFinished || isDead) return;

            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fallDuration);

            // ゆっくり落下させつつ、最後にちょっと加速するなどの演出ができるようにLerp
            transform.position = Vector3.Lerp(initialPos, targetPosition, t * t); // 次第に加速する二次カーブ

            if (t >= 1f)
            {
                hasFinished = true;
                // 落下時間切れ＝失敗、大ダメージ発生
                OnMissileEnd?.Invoke(false);
                Destroy(gameObject);
            }
        }

        protected override void Die()
        {
            if (hasFinished) return;
            hasFinished = true;
            isDead = true;

            // 破壊されたときは爆発（攻撃判定のあるダメージ爆発）はしない
            OnMissileEnd?.Invoke(true);

            if (hpSlider != null)
            {
                Destroy(hpSlider.gameObject);
            }

            Destroy(gameObject);
        }
    }
}
