using UnityEngine;
using System;

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
    public class MarkerData_Alpha
    {
        [Tooltip("タイムライン上の出現タイミング（秒）")]
        public float time;
        [Tooltip("マーカーの種類（UIの見た目に影響）")]
        public MarkerType_Alpha markerType;
    }
}
