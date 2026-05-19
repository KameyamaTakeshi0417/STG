using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class BGScrollUV : MonoBehaviour
{
    [SerializeField] private Vector2 scrollSpeed = new Vector2(0f, -0.2f); // (x,y) / sec
    [SerializeField] private string textureProperty = "_MainTex";

    private Renderer rend;
    private Material mat;
    private Vector2 offset;

    void Awake()
    {
        rend = GetComponent<Renderer>();

        // sharedMaterial を直接いじると全オブジェクトに影響するので material を使う
        mat = rend.material;

        offset = mat.GetTextureOffset(textureProperty);
    }

    void Update()
    {
        offset += scrollSpeed * Time.deltaTime;

        // 値が大きくなりすぎないように 0〜1 に戻す（見た目は同じ）
        offset.x = Mathf.Repeat(offset.x, 1f);
        offset.y = Mathf.Repeat(offset.y, 1f);

        mat.SetTextureOffset(textureProperty, offset);
    }

    void OnDestroy()
    {
        // material を生成しているので破棄（エディタ警告対策）
        if (Application.isPlaying && mat != null) Destroy(mat);
    }
}