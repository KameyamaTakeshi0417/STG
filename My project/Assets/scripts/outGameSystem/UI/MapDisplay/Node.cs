using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public sealed class Node
{
    public int ID
    {
        get { return gridX * 1000 + gridY; }
    } // 一意のID

    public NodeType nodeType = NodeType.Undefined;

    private int gridX;
    private int gridY;

    public Vector2Int grid
    {
        get { return new Vector2Int(gridX, gridY); }
        set
        {
            gridX = value.x;
            gridY = value.y;
        }
    }

    public Vector2 position;

    private List<int> nextNodeIdList = new List<int>(3);
    public IReadOnlyList<int> NextNodeIDList => nextNodeIdList;

    public bool ConnectNode(Node other)
    {
        if (!nextNodeIdList.Contains(other.ID))
        {
            nextNodeIdList.Add(other.ID);
            return true;
        }
        return false;
    }
}
