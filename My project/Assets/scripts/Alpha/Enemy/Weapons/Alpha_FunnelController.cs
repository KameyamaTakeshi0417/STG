using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Alpha.Enemy.Weapons
{
    public class Alpha_FunnelController : MonoBehaviour
    {
        [Header("Funnel Movement")]
        public float moveSpeed = 10f;
        public float rotateSpeed = 360f;

        [Header("Laser Setup")]
        public GameObject laserPrefab; // Alpha_LaserBeamプレハブ
        [Tooltip("発射位置のオフセット（ローカル座標）")]
        public Vector2 spawnOffset = Vector2.zero;

        private Vector2 targetPosition;
        private Quaternion targetRotation;

        private List<Alpha_LaserBeam> activeLasers = new List<Alpha_LaserBeam>();

        void Update()
        {
            // 位置への補間
            transform.position = Vector3.Lerp(transform.position, targetPosition, moveSpeed * Time.deltaTime);
            // 角度への補間
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotateSpeed * Time.deltaTime);
        }

        public void SetTargetPosition(Vector2 pos)
        {
            targetPosition = pos;
        }

        public void SetTargetRotation(Quaternion rot)
        {
            targetRotation = rot;
        }

        public void LookAtTarget(Vector2 targetPos)
        {
            Vector2 dir = (targetPos - (Vector2)transform.position).normalized;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
            targetRotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }

        public void FireLasers(int wayCount, float spreadAngle, float length, float thickness, float expandDuration, float damage)
        {
            ClearLasers();
            
            if (laserPrefab == null) return;

            // 扇状発射のロジック (Behavior_Barrageと同じ仕様)
            Vector2 centerDir = transform.up;
            float baseAngle = Mathf.Atan2(centerDir.y, centerDir.x) * Mathf.Rad2Deg;
            float startAngle = baseAngle - (spreadAngle / 2f);
            float angleStep = wayCount > 1 ? spreadAngle / (wayCount - 1) : 0f;
            
            // 発射位置の計算（ローカルオフセットを加味）
            Vector3 spawnPos = transform.position + (transform.rotation * spawnOffset);

            for (int i = 0; i < wayCount; i++)
            {
                float angle = startAngle + (angleStep * i);
                // レーザーは自身の上方向(Y軸)が正面として作られている前提なので、angle - 90 とする
                Quaternion rot = Quaternion.AngleAxis(angle - 90f, Vector3.forward);
                
                GameObject laserObj = Instantiate(laserPrefab, spawnPos, rot, transform);
                Alpha_LaserBeam laser = laserObj.GetComponent<Alpha_LaserBeam>();
                if (laser != null)
                {
                    laser.length = length;
                    laser.targetThickness = thickness;
                    laser.expandDuration = expandDuration;
                    laser.damage = damage;
                    activeLasers.Add(laser);
                }
            }
        }
        
        public void UpdateLaserLengths(float length)
        {
            foreach(var laser in activeLasers)
            {
                if(laser != null) laser.SetLength(length);
            }
        }

        public void ClearLasers()
        {
            foreach (var laser in activeLasers)
            {
                if (laser != null) Destroy(laser.gameObject);
            }
            activeLasers.Clear();
        }

        public bool HasActiveLasers()
        {
            // 有効なレーザーが1つでもあればtrue
            activeLasers.RemoveAll(l => l == null);
            return activeLasers.Count > 0;
        }

        private void OnDestroy()
        {
            ClearLasers();
        }
    }
}
