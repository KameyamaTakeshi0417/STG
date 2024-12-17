using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Node
{
    public Vector2Int grid; // グリッド座標
    public Vector2 position; // UI上の位置
    public NodeType nodeType = NodeType.Battle; // ノードタイプ

    [SerializeField]
    private List<Node> connectedNodes = new List<Node>();

    public void ConnectNode(Node other)
    {
        if (!connectedNodes.Contains(other))
        {
            connectedNodes.Add(other);
        }
    }
}
