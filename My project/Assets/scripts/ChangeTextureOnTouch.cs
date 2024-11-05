using UnityEngine;
using System.Collections;

public class ChangeTextureOnTouch : MonoBehaviour
{
    public Sprite texture1;  // 最初のテクスチャ
    public Sprite texture2;  // 切り替え後のテクスチャ
    public float shakeDuration = 0.5f; // ピクピクする時間
    public float shakeMagnitude = 0.1f; // ピクピクの強さ

    private SpriteRenderer spriteRenderer;
    private bool isShaking = false;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = texture1; // 初期テクスチャを設定
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isShaking)
        {
            StartCoroutine(ShakeAndChangeTexture());
        }
    }

    private IEnumerator ShakeAndChangeTexture()
    {
        isShaking = true;
        Vector3 originalPosition = transform.position;
        float elapsed = 0.0f;

        // ピクピクと動かす処理
        while (elapsed < shakeDuration)
        {
            float offsetX = Random.Range(-1f, 1f) * shakeMagnitude;
            float offsetY = Random.Range(-1f, 1f) * shakeMagnitude;
            transform.position = new Vector3(originalPosition.x + offsetX, originalPosition.y + offsetY, originalPosition.z);

            elapsed += Time.deltaTime;
            yield return null;
        }

        // ピクピク終了後、位置を元に戻す
        transform.position = originalPosition;

        // テクスチャをtexture2に切り替える
        spriteRenderer.sprite = texture2;

        isShaking = false;
    }
}
