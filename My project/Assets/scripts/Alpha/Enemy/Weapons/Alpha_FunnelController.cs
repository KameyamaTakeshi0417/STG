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
        public Transform[] laserSpawnPoints; // 1WAYなら1つ、2WAYなら2つ

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

        public void FireLasers(float length, float thickness, float expandDuration, float damage)
        {
            ClearLasers();
            
            if (laserPrefab == null) return;

            foreach (var point in laserSpawnPoints)
            {
                if (point == null) continue;
                GameObject laserObj = Instantiate(laserPrefab, point.position, point.rotation, point);
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
