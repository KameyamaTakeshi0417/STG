using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Alpha.UI
{
    public class PopUpUIText : MonoBehaviour
    {
        [SerializeField]
        private float DeleteTime = 1.0f;

        [SerializeField]
        [Tooltip("上に移動する速度")]
        private float MoveSpeed = 100.0f;

        [SerializeField]
        private float EndAlpha = 0;
        
        [SerializeField]
        private string prefix = "+";

        private float TimeCnt;
        private TextMeshProUGUI NowText;
        
        [HideInInspector]
        public int value;

        void Start()
        {
            TimeCnt = 0.0f;
            Destroy(this.gameObject, DeleteTime);
            NowText = this.gameObject.GetComponent<TextMeshProUGUI>();
            
            // UI用（RectTransform）と3D用（Transform）両対応のオフセット処理
            RectTransform rt = GetComponent<RectTransform>();
            if (rt != null)
            {
                float randomOffset = Random.Range(-50f, 50f);
                rt.anchoredPosition += new Vector2(randomOffset, 0);
            }
            else
            {
                float randomOffset = Random.Range(-0.5f, 0.5f);
                this.transform.localPosition += new Vector3(randomOffset, 0, 0);
            }
        }

        void Update()
        {
            if (NowText != null)
            {
                NowText.text = prefix + value.ToString();
                
                // フェードアウト処理
                TimeCnt += Time.deltaTime;
                float _alpha = 1.0f - (1.0f - EndAlpha) * (TimeCnt / DeleteTime);
                if (_alpha <= 0.0f) _alpha = 0.0f;
                NowText.color = new Color(NowText.color.r, NowText.color.g, NowText.color.b, _alpha);
            }

            // 移動処理
            RectTransform rt = GetComponent<RectTransform>();
            if (rt != null)
            {
                // UIキャンバス内での移動
                rt.anchoredPosition += new Vector2(0, MoveSpeed * Time.deltaTime);
            }
            else
            {
                // 3D空間での移動
                this.gameObject.transform.position += new Vector3(0, (MoveSpeed / 100f) * Time.deltaTime, 0);
                
                // 3D空間の場合はカメラの方向を向く（DamageUI3Dと同じ挙動）
                if (Camera.main != null)
                {
                    Vector3 direction = Camera.main.transform.position - transform.position;
                    direction.y = 0;
                    transform.rotation = Quaternion.LookRotation(-direction);
                }
            }
        }
    }
}
