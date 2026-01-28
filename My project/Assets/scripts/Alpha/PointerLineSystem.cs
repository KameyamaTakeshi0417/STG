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
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        }

        private void DrawLine()
        {
            // Position 0: Player
            Vector3 startPos = playerTransform.position;
            startPos.z = zOrder; // Force Z to consistent plane

            // Position 1: Mouse
            Vector3 mouseScreenPos = Input.mousePosition;
            // Set z distance from camera to ensure ScreenToWorldPoint works correctly for 2D
            mouseScreenPos.z = -mainCamera.transform.position.z + zOrder; 
            
            Vector3 targetPos = mainCamera.ScreenToWorldPoint(mouseScreenPos);
            targetPos.z = zOrder;

            lineRenderer.SetPosition(0, startPos);
            lineRenderer.SetPosition(1, targetPos);
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
