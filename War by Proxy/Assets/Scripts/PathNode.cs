using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node
{
    private GameGrid<Node> grid;
    public int x;
    public int z;
}

public class PathNode : Node
{
    private GameGrid<PathNode> grid;

    public int gCost;
    public int hCost;
    public int fCost;

    public PathNode cameFromNode;
    public bool isWalkable;

    public PathNode(GameGrid<PathNode> grid, int x, int z)
    {
        this.grid = grid;
        this.x = x;
        this.z = z;
        isWalkable = true;
    }

    public void SetIsWalkable(bool isWalkable)
    {
        this.isWalkable = isWalkable;
        grid.TriggerGenericGridChanged(x, z);
    }

    public void CalculateFCost()
    {
        fCost = gCost + hCost;
    }

    public override string ToString()
    {
        return x + "," + z;
    }
}

public class DijkstraNode: Node
{
    private GameGrid<DijkstraNode> grid;
    public int moveCost;
    public List<DijkstraNode> connectedFromNodes;
    public DijkstraNode cameFromNode;
    public bool isWalkable;

    public DijkstraNode(GameGrid<DijkstraNode> grid, int x, int z)
    {
        this.grid = grid;
        this.x = x;
        this.z = z;
        this.connectedFromNodes = new List<DijkstraNode>();
    }

    public void SetMoveCost(int moveCost)
    {
        this.moveCost = moveCost;
        grid.TriggerGenericGridChanged(x, z);
    }

    public void SetIsWalkable(bool isWalkable)
    {
        this.isWalkable = isWalkable;
        grid.TriggerGenericGridChanged(x, z);
    }

    public override string ToString()
    {
        return moveCost + "," + connectedFromNodes.Count;
    }

    public string nodePosition()
    {
        return x + "," + z;
    }

    public string connectedFromNodesElements()
    {
        string result = "List of connected nodes: ";
        foreach(DijkstraNode node in connectedFromNodes)
        {
            result += node.nodePosition() + "-";
        }
        result += "| so the amount of elements is " + connectedFromNodes.Count;
        return result;
    }
}