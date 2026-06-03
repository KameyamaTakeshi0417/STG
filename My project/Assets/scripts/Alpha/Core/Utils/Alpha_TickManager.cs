using UnityEngine;
using System;

namespace Alpha.Core.Utils
{
    public class Alpha_TickManager : MonoBehaviour
    {
        public static Alpha_TickManager Instance { get; private set; }

        public event Action OnTick;

        private float tickTimer = 0f;
        private const float TICK_INTERVAL = 0.5f;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                // DontDestroyOnLoad is not used assuming it lives on SequenceBarUI or similar which manages its own lifecycle.
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Update()
        {
            tickTimer += Time.deltaTime;
            if (tickTimer >= TICK_INTERVAL)
            {
                tickTimer -= TICK_INTERVAL;
                OnTick?.Invoke();
            }
        }
    }
}
