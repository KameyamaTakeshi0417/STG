using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NodeMapGenerator : MonoBehaviour
{
    [Header("Grid Settings")]
    public int width = 5; // 幅 (奇数が推奨)
    public int height = 10; // 高さ
    public Vector2 cellSize = new Vector2(100, 160); // セルサイズ

    [Header("UI Settings")]
    public GameObject nodePrefab; // ノードのPrefab (UI要素)
    public Transform nodeParent; // ScrollViewのContent

    private List<Node> nodeList = new List<Node>();

    void Start()
    {
        GenerateNodes(); // ノードを生成
        ConnectNodes(); // ノードを接続
        DrawNodes(); // ノードを描画
    }

    /// <summary>
    /// ノードの生成: 下から上へ均等に配置
    /// </summary>
    private void GenerateNodes()
    {
        nodeList.Clear();
        float startX = -(width / 2) * cellSize.x; // X座標の開始位置 (左右均等)

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Node node = new Node
                {
                    grid = new Vector2Int(x, y),
                    position = new Vector2(startX + x * cellSize.x, y * cellSize.y),
                };

                // スタートとゴールの設定
                if (y == 0 && x == width / 2)
                    node.nodeType = NodeType.Start;
                else if (y == height - 1 && x == width / 2)
                    node.nodeType = NodeType.Goal;

                nodeList.Add(node);
            }
        }
    }

    /// <summary>
    /// ノードの接続: 上方向と左右1マス以内を接続
    /// </summary>
    private void ConnectNodes()
    {
        foreach (Node node in nodeList)
        {
            if (node.grid.y == height - 1)
                continue; // 最上段は接続不要

            foreach (Node other in nodeList)
            {
                if (other.grid.y == node.grid.y + 1 && Mathf.Abs(other.grid.x - node.grid.x) <= 1)
                {
                    node.ConnectNode(other);
                }
            }
        }
    }

    /// <summary>
    /// ノードをUIとして描画
    /// </summary>
    private void DrawNodes()
    {
        foreach (Node node in nodeList)
        {
            GameObject nodeObj = Instantiate(nodePrefab, nodeParent);
            RectTransform rect = nodeObj.GetComponent<RectTransform>();

            // RectTransformの位置設定
            rect.anchoredPosition = new Vector2(node.position.x, -node.position.y);

            nodeObj.name = $"Node_{node.grid.x}_{node.grid.y}";
            nodeObj.GetComponent<Image>().color = GetNodeColor(node.nodeType);
        }

        // Contentサイズを調整
        RectTransform contentRect = nodeParent.GetComponent<RectTransform>();
        contentRect.sizeDelta = new Vector2(width * cellSize.x, height * cellSize.y);
        contentRect.anchorMin = new Vector2(0.5f, 1);
        contentRect.anchorMax = new Vector2(0.5f, 1);
        contentRect.pivot = new Vector2(0.5f, 1);
        contentRect.anchoredPosition = Vector2.zero;
    }

    /// <summary>
    /// ノードの色設定
    /// </summary>
    private Color GetNodeColor(NodeType type)
    {
        switch (type)
        {
            case NodeType.Start:
                return Color.green;
            case NodeType.Goal:
                return Color.red;
            default:
                return Color.grey;
        }
    }
}
