using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace Alpha.UI
{
    public class UIPetalParticleGenerator_Alpha : MonoBehaviour
    {
        [Header("Petal Settings")]
        [Tooltip("花びらの画像。指定がない場合はResourcesから petalBullet を読み込みます")]
        public Sprite petalSprite;
        public bool isEmitting = true;
        public int maxParticles = 30;
        public float spawnRate = 0.15f;
        
        [Header("Movement")]
        public float minSpeed = 50f;
        public float maxSpeed = 150f;
        public float swayAmount = 30f;
        public float swaySpeed = 2f;
        
        [Header("Lifetime & Scale")]
        public float minLifetime = 1.5f;
        public float maxLifetime = 3.5f;
        public float minScale = 0.3f;
        public float maxScale = 0.8f;

        private class PetalData
        {
            public RectTransform rectTransform;
            public Image image;
            public float speed;
            public float lifetime;
            public float currentLife;
            public float swayOffset;
            public float swayFrequency;
            public float startX;
            public Color baseColor;
            public float rotationSpeed;
        }

        private List<PetalData> activePetals = new List<PetalData>();
        private List<PetalData> pool = new List<PetalData>();
        private float spawnTimer;
        private RectTransform spawnArea;

        void Awake()
        {
            spawnArea = GetComponent<RectTransform>();
            
            if (petalSprite == null)
            {
                petalSprite = Resources.Load<Sprite>("Texture/Ammo/sampleTexture/petalBullet");
            }
        }

        void Update()
        {
            if (isEmitting)
            {
                spawnTimer += Time.deltaTime;
                if (spawnTimer >= spawnRate && activePetals.Count < maxParticles)
                {
                    spawnTimer = 0f;
                    SpawnPetal();
                }
            }

            for (int i = activePetals.Count - 1; i >= 0; i--)
            {
                var p = activePetals[i];
                p.currentLife += Time.deltaTime;

                if (p.currentLife >= p.lifetime)
                {
                    DespawnPetal(p);
                    activePetals.RemoveAt(i);
                    continue;
                }

                // Move up
                Vector2 pos = p.rectTransform.anchoredPosition;
                pos.y += p.speed * Time.deltaTime;
                
                // Sway (Sine wave)
                pos.x = p.startX + Mathf.Sin((Time.time + p.swayOffset) * p.swayFrequency) * swayAmount;
                
                p.rectTransform.anchoredPosition = pos;

                // Rotate slightly over time
                p.rectTransform.Rotate(0, 0, p.rotationSpeed * Time.deltaTime);

                // Fade out (using alpha)
                float alpha = 1f;
                // Fade in slightly
                if (p.currentLife < 0.2f) alpha = p.currentLife / 0.2f;
                // Fade out at end
                else if (p.currentLife > p.lifetime - 1f) alpha = (p.lifetime - p.currentLife) / 1f;
                
                Color c = p.baseColor;
                c.a = alpha;
                p.image.color = c;
            }
        }

        private void SpawnPetal()
        {
            PetalData p;
            if (pool.Count > 0)
            {
                p = pool[pool.Count - 1];
                pool.RemoveAt(pool.Count - 1);
                p.rectTransform.gameObject.SetActive(true);
            }
            else
            {
                GameObject go = new GameObject("PetalParticle");
                go.transform.SetParent(this.transform, false);
                p = new PetalData();
                p.rectTransform = go.AddComponent<RectTransform>();
                p.image = go.AddComponent<Image>();
                p.image.sprite = petalSprite;
                p.image.raycastTarget = false; // クリック判定を邪魔しないように
            }

            float scale = Random.Range(minScale, maxScale);
            p.rectTransform.localScale = new Vector3(scale, scale, 1f);
            
            p.speed = Random.Range(minSpeed, maxSpeed);
            p.lifetime = Random.Range(minLifetime, maxLifetime);
            p.currentLife = 0f;
            p.swayOffset = Random.Range(0f, 100f);
            p.swayFrequency = swaySpeed * Random.Range(0.8f, 1.2f);
            p.rotationSpeed = Random.Range(-60f, 60f);

            // Start position (bottom of the RectTransform)
            float width = spawnArea.rect.width;
            float startX = Random.Range(-width / 2f, width / 2f);
            float startY = -spawnArea.rect.height / 2f - 20f; // 少し下から
            
            p.startX = startX;
            p.rectTransform.anchoredPosition = new Vector2(startX, startY);

            // Random Cyan to Blue color
            float r = Random.Range(0f, 0.3f);
            float g = Random.Range(0.4f, 0.9f);
            float b = Random.Range(0.8f, 1.0f);
            p.baseColor = new Color(r, g, b, 1f);
            p.image.color = new Color(r, g, b, 0f); // 初期は透明(フェードイン)
            
            // Initial random rotation
            p.rectTransform.localRotation = Quaternion.Euler(0, 0, Random.Range(0f, 360f));

            activePetals.Add(p);
        }

        private void DespawnPetal(PetalData p)
        {
            p.rectTransform.gameObject.SetActive(false);
            pool.Add(p);
        }

        public void ClearParticles()
        {
            for (int i = activePetals.Count - 1; i >= 0; i--)
            {
                DespawnPetal(activePetals[i]);
                activePetals.RemoveAt(i);
            }
        }
    }
}
