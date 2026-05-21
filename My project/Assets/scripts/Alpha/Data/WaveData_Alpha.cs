using UnityEngine;
using System;
using System.Collections.Generic;

namespace Alpha.Data
{
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
        
        [Tooltip("このウェーブでスポーンする敵のリスト")]
        public List<SpawnEntry_Alpha> spawns = new List<SpawnEntry_Alpha>();
    }
}
