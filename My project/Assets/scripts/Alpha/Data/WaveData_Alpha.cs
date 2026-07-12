using UnityEngine;
using System;
using System.Collections.Generic;

namespace Alpha.Data
{
    public enum MarkerType_Alpha
    {
        Normal,
        Elite,
        Event,
        MidBoss,
        Boss
    }

    [Serializable]
    public class SpawnEntry_Alpha
    {
        [Tooltip("スポーンする敵のプレハブ")]
        public GameObject enemyPrefab;
        [Tooltip("スポーン位置")]
        public Vector2 spawnPosition;
    }

    [Serializable]
    public class WaveData_Alpha
    {
        [Tooltip("ウェーブの開始タイミング（秒）")]
        public float time;
        
        [Tooltip("進行バーに表示するマーカーの種類")]
        public MarkerType_Alpha markerType = MarkerType_Alpha.Normal;
        
        [Tooltip("このウェーブでスポーンする敵のリスト")]
        public List<SpawnEntry_Alpha> spawns = new List<SpawnEntry_Alpha>();
    }
}
