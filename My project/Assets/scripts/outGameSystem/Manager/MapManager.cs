using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapManager : MonoBehaviour
{
    public GameObject nodePrefab; // ノード用のプレハブ
    public Sprite[] nodeSprites; // 0-3に対応するノード画像
    public Sprite mapBackground; // マップ背景画像
    public GameObject mapPanel; // マップ表示用のPanel
    public GameObject linePrefab; // 経路を描画するLineRenderer用プレハブ

    public int rows = 10; // 縦 (行数)
    public int columns = 3; // 横 (列数)
    private int[,] mapData; // ノードデータ格納
    private List<GameObject> nodes = new List<GameObject>(); // 生成したノードのリスト
    private List<LineRenderer> lines = new List<LineRenderer>(); // 経路のライン

    void Start()
    {
        InitializeMapData();
        CreateMap();
        CreatePaths();
    }

    // ノードデータ初期化
    void InitializeMapData()
    {
        mapData = new int[columns, rows];
        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < columns; x++)
            {
                mapData[x, y] = Random.Range(0, nodeSprites.Length); // 0-3のランダム値
            }
        }
    }

    // マップとノード生成
    void CreateMap()
    {
        // 背景設定
        Image bgImage = mapPanel.AddComponent<Image>();
        bgImage.sprite = mapBackground;
        bgImage.type = Image.Type.Sliced;

        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < columns; x++)
            {
                CreateNode(x, y, mapData[x, y]);
            }
        }
    }

    // ノードの生成
    void CreateNode(int x, int y, int nodeType)
    {
        GameObject node = Instantiate(nodePrefab, mapPanel.transform);
        node.name = $"Node_{x}_{y}";

        // ノードの見た目を設定
        Image nodeImage = node.GetComponent<Image>();
        if (nodeImage != null && nodeType >= 0 && nodeType < nodeSprites.Length)
        {
            nodeImage.sprite = nodeSprites[nodeType];
        }

        // ノードの位置を設定
        RectTransform rectTransform = node.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = new Vector2(x * 200 - 200, -y * 150 + 600);
        rectTransform.sizeDelta = new Vector2(100, 100);

        nodes.Add(node);
    }

    // 経路 (ライン) の生成
    void CreatePaths()
    {
        for (int y = 0; y < rows - 1; y++) // 最終行は接続しない
        {
            for (int x = 0; x < columns; x++)
            {
                int currentIndex = y * columns + x;
                ConnectToNextRow(currentIndex, y, x);
            }
        }
    }

    // 次の行への接続経路生成
    void ConnectToNextRow(int currentIndex, int y, int x)
    {
        // 同じ列、斜め左、斜め右のノードに接続
        for (int nextX = Mathf.Max(0, x - 1); nextX <= Mathf.Min(columns - 1, x + 1); nextX++)
        {
            int nextIndex = (y + 1) * columns + nextX;

            // LineRendererで経路を描画
            GameObject lineObj = Instantiate(linePrefab, mapPanel.transform);
            LineRenderer lineRenderer = lineObj.GetComponent<LineRenderer>();

            lineRenderer.positionCount = 2;
            lineRenderer.SetPosition(
                0,
                nodes[currentIndex].GetComponent<RectTransform>().anchoredPosition
            );
            lineRenderer.SetPosition(
                1,
                nodes[nextIndex].GetComponent<RectTransform>().anchoredPosition
            );

            lineRenderer.startWidth = 5f;
            lineRenderer.endWidth = 5f;

            lines.Add(lineRenderer);
        }
    }
}
