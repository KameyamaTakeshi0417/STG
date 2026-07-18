using UnityEngine;
using UnityEngine.UI;

namespace Alpha.UI
{
    public class RecommendationGlow_Alpha : MonoBehaviour
    {
        private Outline outline;
        private float time;
        public float speed = 3f;
        
        private void Awake()
        {
            outline = GetComponent<Outline>();
            if (outline == null)
            {
                outline = gameObject.AddComponent<Outline>();
            }
            outline.effectColor = new Color(1f, 0.8f, 0.2f, 1f); // Gold
            outline.effectDistance = new Vector2(5f, -5f);
            outline.enabled = false;
        }

        public void SetRecommended(bool isRecommended)
        {
            if (outline == null)
            {
                outline = GetComponent<Outline>();
                if (outline == null) outline = gameObject.AddComponent<Outline>();
                outline.effectColor = new Color(1f, 0.8f, 0.2f, 1f);
                outline.effectDistance = new Vector2(5f, -5f);
            }
            
            outline.enabled = isRecommended;
            if (isRecommended)
            {
                time = 0f;
            }
        }

        private void Update()
        {
            if (outline != null && outline.enabled)
            {
                time += Time.unscaledDeltaTime * speed; 
                float alpha = (Mathf.Sin(time) + 1f) / 2f; 
                alpha = Mathf.Lerp(0.3f, 1f, alpha);
                Color c = outline.effectColor;
                c.a = alpha;
                outline.effectColor = c;
            }
        }
    }
}
