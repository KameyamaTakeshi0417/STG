using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodeMapGenerator : MonoBehaviour
{
    public int width = 7; // グリッドの幅
    public int height = 10; // グリッドの高さ
    public Vector2 cellSize = new Vector2(100, 160); // セルサイズ

    private List<Node> nodeList = new List<Node>();
    public GameObject nodePrefab;
    public Transform nodeParent;

    void Start()
    {
        GenerateGrid(); // グリッドとノードを生成
        CreatePaths(); // 通り道を生成
        SetNodeTypes(); // ノードタイプを配置
        CreateGoal(); // ゴールノードを作成
        DrawNodes(); // ノードを描画
    }

    public void GenerateGrid()
    {
        Vector2 startPos = cellSize * 0.5f;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Node node = new Node
                {
                    grid = new Vector2Int(x, y),
                    position =
                        startPos
                        + new Vector2(x * cellSize.x, y * cellSize.y)
                        + new Vector2(
                            Random.Range(-cellSize.x * 0.25f, cellSize.x * 0.25f),
                            Random.Range(-cellSize.y * 0.25f, cellSize.y * 0.25f)
                        ),
                };

                nodeList.Add(node);
            }
        }
    }

    public void CreatePaths()
    {
        List<Node> currentRowNodes = nodeList.FindAll(n => n.grid.y == 0);

        for (int i = 0; i < 3; i++) // 道の本数
        {
            Node currentNode = currentRowNodes[Random.Range(0, currentRowNodes.Count)];
            for (int y = 1; y < height; y++)
            {
                List<Node> nextRowNodes = nodeList.FindAll(n =>
                    n.grid.y == y && Mathf.Abs(n.grid.x - currentNode.grid.x) <= 1
                );
                if (nextRowNodes.Count == 0)
                    break;

                Node nextNode = nextRowNodes[Random.Range(0, nextRowNodes.Count)];
                currentNode.ConnectNode(nextNode);
                currentNode = nextNode;
            }
        }
    }

    public void SetNodeTypes()
    {
        foreach (Node node in nodeList)
        {
            if (node.grid.y == 0)
                node.nodeType = NodeType.Start;
            else if (node.grid.y == height - 1)
                node.nodeType = NodeType.Rest;
            else if (Random.value < 0.1f)
                node.nodeType = NodeType.Treasure;
            else
                node.nodeType = NodeType.Battle;
        }
    }

    public void CreateGoal()
    {
        Node goalNode = new Node { nodeType = NodeType.Goal };
        nodeList.Add(goalNode);

        List<Node> lastRowNodes = nodeList.FindAll(n => n.grid.y == height - 1);
        Node randomNode = lastRowNodes[Random.Range(0, lastRowNodes.Count)];
        randomNode.ConnectNode(goalNode);
    }

    public void DrawNodes()
    {
        foreach (Node node in nodeList)
        {
            GameObject nodeObj = Instantiate(
                nodePrefab,
                node.position,
                Quaternion.identity,
                nodeParent
            );
            nodeObj.name = $"Node_{node.ID}";
            // タイプに応じて色を変更
            nodeObj.GetComponent<SpriteRenderer>().color = GetNodeColor(node.nodeType);
        }
    }

    private Color GetNodeColor(NodeType type)
    {
        switch (type)
        {
            case NodeType.Start:
                return Color.green;
            case NodeType.Goal:
                return Color.red;
            case NodeType.Battle:
                return Color.grey;
            case NodeType.Treasure:
                return Color.yellow;
            case NodeType.Rest:
                return Color.blue;
            default:
                return Color.white;
        }
    }
}
