using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Alpha.Enemy.Weapons
{
    public class Alpha_FunnelController : MonoBehaviour
    {
        public enum AimMode
        {
            Player,
            Outward,
            Inward,
            Fixed
        }

        [Header("Funnel Movement (Self-Managed)")]
        public float moveSpeed = 10f;
        public float rotateSpeed = 360f;
        
        [HideInInspector] public Transform centerTarget;
        [HideInInspector] public float orbitRadius = 0f;
        [HideInInspector] public float orbitSpeed = 0f;
        [HideInInspector] public float currentAngle = 0f;
        [HideInInspector] public AimMode aimMode = AimMode.Player;

        [Header("Laser Setup")]
        public GameObject laserPrefab; // Alpha_LaserBeamプレハブ
        [Tooltip("発射位置のオフセット（ローカル座標）")]
        public Vector2 spawnOffset = Vector2.zero;
        
        [HideInInspector] public int wayCount = 1;
        [HideInInspector] public float spreadAngle = 0f;
        [HideInInspector] public float laserLength = 30f;
        [HideInInspector] public float laserThickness = 1.5f;
        [HideInInspector] public float laserExpandTime = 0.3f;
        [HideInInspector] public float laserDamage = 1f;

        [HideInInspector] public float fireInterval = 0f; // 0なら撃ちっぱなし
        [HideInInspector] public float fireDuration = 0.5f;

        private List<Alpha_LaserBeam> activeLasers = new List<Alpha_LaserBeam>();
        private Transform playerTarget;
        private Coroutine fireCoroutine;

        void Start()
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) playerTarget = player.transform;

            if (fireInterval <= 0f)
            {
                // 撃ちっぱなし
                FireLasers(wayCount, spreadAngle, laserLength, laserThickness, laserExpandTime, laserDamage);
            }
            else
            {
                fireCoroutine = StartCoroutine(FireRoutine());
            }
        }

        private float currentRadius = 0f; // 徐々に広がるための内部変数

        void Update()
        {
            if (centerTarget == null) return;

            // 1. 位置の計算（円周上を移動）
            currentAngle += orbitSpeed * Time.deltaTime;
            float angleRad = currentAngle * Mathf.Deg2Rad;

            // スポーン直後はボスの位置から指定半径まで徐々に広がる
            currentRadius = Mathf.Lerp(currentRadius, orbitRadius, moveSpeed * Time.deltaTime);

            // ボス中心から、現在の半径と角度で正確な円軌道の座標を算出
            Vector2 targetPos = (Vector2)centerTarget.position + new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad)) * currentRadius;
            
            // 位置を強制的に同期（Lerpを使わないことで、ボス移動時も完璧な円軌道を維持する）
            transform.position = targetPos;

            // 2. 角度の計算
            Quaternion targetRot = transform.rotation;
            if (aimMode == AimMode.Player && playerTarget != null)
            {
                Vector2 dir = ((Vector2)playerTarget.position - (Vector2)transform.position).normalized;
                if (dir != Vector2.zero)
                {
                    float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
                    targetRot = Quaternion.AngleAxis(angle, Vector3.forward);
                }
            }
            else if (aimMode == AimMode.Outward)
            {
                Vector2 dir = ((Vector2)transform.position - (Vector2)centerTarget.position).normalized;
                if (dir != Vector2.zero)
                {
                    float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
                    targetRot = Quaternion.AngleAxis(angle, Vector3.forward);
                }
            }
            else if (aimMode == AimMode.Inward)
            {
                Vector2 dir = ((Vector2)centerTarget.position - (Vector2)transform.position).normalized;
                if (dir != Vector2.zero)
                {
                    float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
                    targetRot = Quaternion.AngleAxis(angle, Vector3.forward);
                }
            }

            if (aimMode != AimMode.Fixed)
            {
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, rotateSpeed * Time.deltaTime);
            }
        }

        private IEnumerator FireRoutine()
        {
            while (true)
            {
                // 待機中もターゲットを向く（Update内で処理済）
                yield return new WaitForSeconds(fireInterval);

                // 発射
                FireLasers(wayCount, spreadAngle, laserLength, laserThickness, laserExpandTime, laserDamage);

                // 照射時間
                yield return new WaitForSeconds(fireDuration);

                // 停止
                ClearLasers();
            }
        }

        public void FireLasers(int ways, float spread, float length, float thickness, float expandDuration, float damage)
        {
            ClearLasers();
            
            if (laserPrefab == null) return;

            // 扇状発射のロジック (Behavior_Barrageと同じ仕様)
            Vector2 centerDir = transform.up;
            float baseAngle = Mathf.Atan2(centerDir.y, centerDir.x) * Mathf.Rad2Deg;
            float startAngle = baseAngle - (spread / 2f);
            float angleStep = ways > 1 ? spread / (ways - 1) : 0f;
            
            // 発射位置の計算（ローカルオフセットを加味）
            Vector3 spawnPos = transform.position + (transform.rotation * spawnOffset);

            for (int i = 0; i < ways; i++)
            {
                float angle = startAngle + (angleStep * i);
                // レーザーは自身の上方向(Y軸)が正面として作られている前提なので、angle - 90 とする
                Quaternion rot = Quaternion.AngleAxis(angle - 90f, Vector3.forward);
                
                GameObject laserObj = Instantiate(laserPrefab, spawnPos, rot, transform);
                Alpha_LaserBeam laser = laserObj.GetComponent<Alpha_LaserBeam>();
                if (laser != null)
                {
                    laser.Setup(length, thickness, expandDuration, damage);
                    activeLasers.Add(laser);
                }
            }
        }

        public void ClearLasers()
        {
            activeLasers.RemoveAll(l => l == null);
            foreach (var laser in activeLasers)
            {
                if (laser != null) Destroy(laser.gameObject);
            }
            activeLasers.Clear();
        }

        private void OnDestroy()
        {
            ClearLasers();
        }
    }
}
