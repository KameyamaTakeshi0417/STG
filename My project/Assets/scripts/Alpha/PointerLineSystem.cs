using UnityEngine;

namespace Alpha
{
    [RequireComponent(typeof(LineRenderer))]
    public class PointerLineSystem : MonoBehaviour
    {
        [Header("Target Settings")]
        [SerializeField, Tooltip("Reference to the player transform. If null, tries to find object with 'Player' tag.")]
        private Transform playerTransform;

        [Header("Line Settings")]
        [SerializeField] private float lineWidth = 0.05f;
        [SerializeField] private Color startColor = Color.white;
        [SerializeField] private Color endColor = new Color(1f, 1f, 1f, 0.5f);
        [SerializeField] private float zOrder = 10f; // To ensure it renders above/below as needed

        [Header("Snapping Settings")]
        [SerializeField] private string targetTag = "Enemy";
        [SerializeField] private float snapRadius = 2.0f;
        [SerializeField] private Color lockOnColor = Color.red;

        public Transform CurrentTarget { get; private set; }

        private LineRenderer lineRenderer;
        private Camera mainCamera;

        private void Awake()
        {
            lineRenderer = GetComponent<LineRenderer>();
            mainCamera = Camera.main;

            InitializeLineRenderer();
        }

        private void Start()
        {
            // Auto-assign player if not set
            if (playerTransform == null)
            {
                GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
                if (playerObj != null)
                {
                    playerTransform = playerObj.transform;
                }
                else
                {
                    Debug.LogWarning("[PointerLineSystem] Player Transform is missing and no object with 'Player' tag found.");
                }
            }
        }

        private void LateUpdate()
        {
            if (playerTransform == null || mainCamera == null) return;

            DrawLine();
        }

        private void InitializeLineRenderer()
        {
            lineRenderer.startWidth = lineWidth;
            lineRenderer.endWidth = lineWidth;
            lineRenderer.startColor = startColor;
            lineRenderer.endColor = endColor;
            lineRenderer.positionCount = 2;
            lineRenderer.useWorldSpace = true;
            
            // Basic material setup to ensure visibility without external assets
            // Using Sprites/Default shader generally works well for 2D flat lines
            if (lineRenderer.material == null || lineRenderer.material.shader.name != "Sprites/Default")
            {
                lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            }
        }

        private void DrawLine()
        {
            // Position 0: Player
            Vector3 startPos = playerTransform.position;
            startPos.z = zOrder; // Force Z to consistent plane

            // Position 1: Mouse or Target
            Vector3 mouseScreenPos = Input.mousePosition;
            // Set z distance from camera to ensure ScreenToWorldPoint works correctly for 2D
            mouseScreenPos.z = -mainCamera.transform.position.z + zOrder; 
            
            Vector3 worldMousePos = mainCamera.ScreenToWorldPoint(mouseScreenPos);
            worldMousePos.z = zOrder;

            // Find target
            CurrentTarget = FindClosestTarget(worldMousePos);

            Vector3 endPos;
            if (CurrentTarget != null)
            {
                endPos = CurrentTarget.position;
                lineRenderer.startColor = lockOnColor;
                lineRenderer.endColor = lockOnColor;
            }
            else
            {
                endPos = worldMousePos;
                lineRenderer.startColor = startColor;
                lineRenderer.endColor = endColor;
            }
            endPos.z = zOrder;

            // Optional: Draw line from player center or exactly match bullet spawn point.
            // Keeping it from center (startPos) is usually fine.
            lineRenderer.SetPosition(0, startPos);
            lineRenderer.SetPosition(1, endPos);
        }

        private Transform FindClosestTarget(Vector3 center)
        {
            Collider2D closest = null;
            float minDist = Mathf.Infinity;

            // Get all colliders in range
            Collider2D[] hits = Physics2D.OverlapCircleAll(center, snapRadius);

            foreach (var hit in hits)
            {
                // Filter by tag
                if (!hit.CompareTag(targetTag)) continue;

                float dist = Vector2.Distance(center, hit.transform.position);
                if (dist < minDist)
                {
                    minDist = dist;
                    closest = hit;
                }
            }

            return closest != null ? closest.transform : null;
        }

        private void OnDrawGizmosSelected()
        {
            if (mainCamera != null)
            {
                 Vector3 mouseScreenPos = Input.mousePosition;
                 mouseScreenPos.z = -mainCamera.transform.position.z + zOrder;
                 Vector3 worldMousePos = mainCamera.ScreenToWorldPoint(mouseScreenPos);
                 
                 Gizmos.color = Color.yellow;
                 Gizmos.DrawWireSphere(worldMousePos, snapRadius);
            }
        }

        // Allow runtime updates of width/color for debug/tweaking
        private void OnValidate()
        {
            if (lineRenderer != null)
            {
                lineRenderer.startWidth = lineWidth;
                lineRenderer.endWidth = lineWidth;
                lineRenderer.startColor = startColor;
                lineRenderer.endColor = endColor;
            }
        }
    }
}
