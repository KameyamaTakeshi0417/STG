using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

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
        private TextMeshPro countdownText;
        private SpriteRenderer spriteRenderer;

        protected override void Start()
        {
            base.Start();
            initialPos = transform.position;
            
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

            // Create Countdown UI at targetPosition
            GameObject textObj = new GameObject("MissileCountdown");
            textObj.transform.position = targetPosition;
            countdownText = textObj.AddComponent<TextMeshPro>();
            countdownText.alignment = TextAlignmentOptions.Center;
            countdownText.fontSize = 5f;
            countdownText.color = Color.yellow;
            countdownText.GetComponent<MeshRenderer>().sortingLayerName = "inGameUI";
            countdownText.GetComponent<MeshRenderer>().sortingOrder = -1;
        }

        protected override void Update()
        {
            base.Update();
            if (hasFinished || isDead) return;

            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fallDuration);
            float remaining = Mathf.Max(0, fallDuration - elapsed);

            if (countdownText != null)
            {
                countdownText.text = remaining.ToString("F2");
                countdownText.fontSize = Mathf.Lerp(5f, 50f, t * t); // Accelerate growth

                if (remaining > 4f)
                {
                    float colorT = Mathf.InverseLerp(fallDuration, 4f, remaining);
                    countdownText.color = Color.Lerp(Color.yellow, Color.red, colorT);
                }
                else if (remaining <= 3f)
                {
                    float colorT = Mathf.InverseLerp(3f, 0f, remaining);
                    countdownText.color = Color.Lerp(Color.red, new Color(0.3f, 0f, 0f), colorT);
                }
                else
                {
                    countdownText.color = Color.red;
                }
            }

            if (spriteRenderer != null)
            {
                spriteRenderer.color = Color.Lerp(Color.white, Color.red, t);
            }

            // ゆっくり落下させつつ、最後にちょっと加速するなどの演出ができるようにLerp
            transform.position = Vector3.Lerp(initialPos, targetPosition, t * t); // 次第に加速する二次カーブ

            if (t >= 1f)
            {
                hasFinished = true;
                if (countdownText != null) Destroy(countdownText.gameObject);
                
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

            if (countdownText != null) Destroy(countdownText.gameObject);

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
