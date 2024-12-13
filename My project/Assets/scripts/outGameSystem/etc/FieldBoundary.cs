using UnityEngine;

public class FieldBoundary : MonoBehaviour
{
    public Collider2D[] boundaries; // 見えない壁の配列

    private void Start()
    {
        EnableBoundary(true); // 初期状態では壁を有効にする
    }

    public void EnableBoundary(bool enable)
    {
        foreach (var boundary in boundaries)
        {
            boundary.enabled = enable;
        }
    }
}
