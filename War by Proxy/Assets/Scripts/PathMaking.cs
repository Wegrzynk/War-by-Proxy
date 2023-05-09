using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathMaking
{
    public static PathMaking Instance { get; private set; }

    private GameGrid<DijkstraNode> grid;
    private List<DijkstraNode> unprocessedVertices;
    private List<DijkstraNode> processedVertices;

    public PathMaking(int width, int height)
    {
        Instance = this;
        grid = new GameGrid<DijkstraNode>(width, height, 2f, Vector3.zero, false, (GameGrid<DijkstraNode> g, int x, int z) => new DijkstraNode(g, x, z));
    }

    public GameGrid<DijkstraNode> GetGrid()
    {
        return grid;
    }

    public List<DijkstraNode> ApplyDijkstra(int x, int z, List<DijkstraNode> graph, Unit unit, Unitmap unitmap, bool getWholeMap)
    {
        DijkstraNode startNode = grid.GetGridObject(x, z);
        int distance = unit.GetMovementDistance();
        int[] moveCost = new int[graph.Count];
        DijkstraNode[] shortestPreviousNodes = new DijkstraNode[graph.Count];
        processedVertices = new List<DijkstraNode>();
        unprocessedVertices = new List<DijkstraNode>();
        unprocessedVertices.AddRange(graph);
        
        List<DijkstraNode> neighbours = new List<DijkstraNode>();

        for(int i = 0; i < graph.Count; i++)
        {
            moveCost[i] = int.MaxValue;
            shortestPreviousNodes[i] = null;
        }
        moveCost[unprocessedVertices.IndexOf(startNode)] = 0;
        
        while(unprocessedVertices.Count != 0)
        {
            int currentSmallest = int.MaxValue;
            int smallestNodeIndex = -1;
            for(int i = 0; i < graph.Count; i++)
            {
                if(moveCost[i] < currentSmallest && unprocessedVertices.IndexOf(graph[i]) != -1)
                {
                    currentSmallest = moveCost[i];
                    smallestNodeIndex = i;
                }
            }
            processedVertices.Add(graph[smallestNodeIndex]);
            unprocessedVertices.Remove(graph[smallestNodeIndex]);
            for(int i = 0; i < graph.Count; i++)
            {
                if(unprocessedVertices.Contains(graph[i]) && graph[i].connectedFromNodes.IndexOf(graph[smallestNodeIndex]) != -1)
                {
                    if(moveCost[i] > moveCost[smallestNodeIndex] + graph[i].moveCost){
                        moveCost[i] = moveCost[smallestNodeIndex] + graph[i].moveCost;
                        shortestPreviousNodes[i] = graph[smallestNodeIndex];
                    }
                }
            }
        }

        if(getWholeMap)
        {
            for(int i = 0; i < moveCost.Length; i++)
            {
                graph[i].SetMoveCost(moveCost[i]);
                if(moveCost[i] > 2048 || (i != 0 && unitmap.GetGrid().GetGridObject(graph[i].x, graph[i].z) != null))
                {
                    processedVertices.Remove(graph[i]);
                }
            }
        }
        else
        {
            for(int i = 0; i < moveCost.Length; i++)
            {
                if(moveCost[i] > unit.GetMovementDistance() || (i != 0 && unitmap.GetGrid().GetGridObject(graph[i].x, graph[i].z) != null))
                {
                    processedVertices.Remove(graph[i]);
                }
            }
        }

        return processedVertices;
    }

    public List<DijkstraNode> ApplyDijkstra(int x, int z, List<DijkstraNode> graph, Unit unit, Unitmap unitmap, bool getWholeMap, bool includeFriendlies)
    {
        DijkstraNode startNode = grid.GetGridObject(x, z);
        int distance = unit.GetMovementDistance();
        int[] moveCost = new int[graph.Count];
        DijkstraNode[] shortestPreviousNodes = new DijkstraNode[graph.Count];
        processedVertices = new List<DijkstraNode>();
        unprocessedVertices = new List<DijkstraNode>();
        unprocessedVertices.AddRange(graph);
        
        List<DijkstraNode> neighbours = new List<DijkstraNode>();

        for(int i = 0; i < graph.Count; i++)
        {
            moveCost[i] = int.MaxValue;
            shortestPreviousNodes[i] = null;
        }
        moveCost[unprocessedVertices.IndexOf(startNode)] = 0;
        
        while(unprocessedVertices.Count != 0)
        {
            int currentSmallest = int.MaxValue;
            int smallestNodeIndex = -1;
            for(int i = 0; i < graph.Count; i++)
            {
                if(moveCost[i] < currentSmallest && unprocessedVertices.IndexOf(graph[i]) != -1)
                {
                    currentSmallest = moveCost[i];
                    smallestNodeIndex = i;
                }
            }
            processedVertices.Add(graph[smallestNodeIndex]);
            unprocessedVertices.Remove(graph[smallestNodeIndex]);
            for(int i = 0; i < graph.Count; i++)
            {
                if(unprocessedVertices.Contains(graph[i]) && graph[i].connectedFromNodes.IndexOf(graph[smallestNodeIndex]) != -1)
                {
                    if(moveCost[i] > moveCost[smallestNodeIndex] + graph[i].moveCost){
                        moveCost[i] = moveCost[smallestNodeIndex] + graph[i].moveCost;
                        shortestPreviousNodes[i] = graph[smallestNodeIndex];
                    }
                }
            }
        }

        if(getWholeMap)
        {
            for(int i = 0; i < moveCost.Length; i++)
            {
                graph[i].SetMoveCost(moveCost[i]);
                if(moveCost[i] > 2048)
                {
                    processedVertices.Remove(graph[i]);
                }
            }
        }
        else
        {
            for(int i = 0; i < moveCost.Length; i++)
            {
                if(moveCost[i] > unit.GetMovementDistance())
                {
                    processedVertices.Remove(graph[i]);
                }
            }
        }

        return processedVertices;
    }

    private string printArray(int[] moveCost)
    {
        string result = "Current moveCost array values are: ";
        for(int i = 0; i < moveCost.Length; i++)
        {
            result += moveCost[i] + "-";
        }
        result += "|";
        return result;
    }

    public List<DijkstraNode> CreateReachableGraph(int x, int z, Unit unit, Tilemap selectedTilemap, Unitmap selectedUnitmap, bool isEnemy, bool getWholeMap)
    {
        DijkstraNode startNode = grid.GetGridObject(x, z);
        startNode.SetMoveCost(selectedTilemap.GetGrid().GetGridObject(x, z).GetMovementPenaltyType(unit));
        if(startNode.moveCost == 0) startNode.SetMoveCost(4096);
        List<DijkstraNode> checkerList = new List<DijkstraNode> { startNode };
        List<DijkstraNode> graphVertices = new List<DijkstraNode>();
        List<DijkstraNode> replacer = new List<DijkstraNode>();
        int givenMovementDistance;

        if(isEnemy)
        {
            givenMovementDistance = unit.GetMovementDistance() + 1;
        }
        else if(getWholeMap)
        {
            givenMovementDistance = 2048;
        }
        else
        {
            givenMovementDistance = unit.GetMovementDistance();
        }

        for(int i = 0; i < givenMovementDistance; i++)
        {
            foreach(DijkstraNode checkerNode in checkerList)
            {
                foreach(DijkstraNode neighbourNode in GetNeighbourList(checkerNode))
                {
                    neighbourNode.connectedFromNodes.Add(checkerNode);
                    grid.TriggerGenericGridChanged(neighbourNode.x, neighbourNode.z);
                    if(i == unit.GetMovementDistance() - 1 && !graphVertices.Contains(neighbourNode))
                    {
                        checkerNode.connectedFromNodes.Add(neighbourNode);
                        grid.TriggerGenericGridChanged(checkerNode.x, checkerNode.z);
                    }
                    if(!graphVertices.Contains(neighbourNode) && !replacer.Contains(neighbourNode))
                    {
                        Unit blocker = selectedUnitmap.GetGrid().GetGridObject(neighbourNode.x, neighbourNode.z);
                        neighbourNode.SetMoveCost(selectedTilemap.GetGrid().GetGridObject(neighbourNode.x, neighbourNode.z).GetMovementPenaltyType(unit));
                        if(neighbourNode.moveCost == 0 || (blocker != null && blocker.GetTeam() != unit.GetTeam())) neighbourNode.SetMoveCost(4096);
                        replacer.Add(neighbourNode);
                    }
                }
                graphVertices.Add(checkerNode);
            }
            checkerList.Clear();
            checkerList.AddRange(replacer);
            replacer.Clear();
        }
        foreach(DijkstraNode remaining in checkerList)
        {
            graphVertices.Add(remaining);
        }


        replacer.AddRange(ApplyDijkstra(x, z, graphVertices, unit, selectedUnitmap, getWholeMap));

        return replacer;
    }

    public List<DijkstraNode> CreateReachableGraph(int x, int z, Unit unit, Unit.UnitType type, Tilemap selectedTilemap, Unitmap selectedUnitmap, bool isEnemy, bool getWholeMap)
    {
        DijkstraNode startNode = grid.GetGridObject(x, z);
        startNode.SetMoveCost(selectedTilemap.GetGrid().GetGridObject(x, z).GetMovementPenaltyType((int)type));
        if(startNode.moveCost == 0) startNode.SetMoveCost(4096);
        List<DijkstraNode> checkerList = new List<DijkstraNode> { startNode };
        List<DijkstraNode> graphVertices = new List<DijkstraNode>();
        List<DijkstraNode> replacer = new List<DijkstraNode>();
        int givenMovementDistance;

        if(isEnemy)
        {
            givenMovementDistance = 3 + 1;
        }
        else if(getWholeMap)
        {
            givenMovementDistance = 2048;
        }
        else
        {
            givenMovementDistance = 3;
        }

        for(int i = 0; i < givenMovementDistance; i++)
        {
            foreach(DijkstraNode checkerNode in checkerList)
            {
                foreach(DijkstraNode neighbourNode in GetNeighbourList(checkerNode))
                {
                    neighbourNode.connectedFromNodes.Add(checkerNode);
                    grid.TriggerGenericGridChanged(neighbourNode.x, neighbourNode.z);
                    if(i == 3 - 1 && !graphVertices.Contains(neighbourNode))
                    {
                        checkerNode.connectedFromNodes.Add(neighbourNode);
                        grid.TriggerGenericGridChanged(checkerNode.x, checkerNode.z);
                    }
                    if(!graphVertices.Contains(neighbourNode) && !replacer.Contains(neighbourNode))
                    {
                        Unit blocker = selectedUnitmap.GetGrid().GetGridObject(neighbourNode.x, neighbourNode.z);
                        neighbourNode.SetMoveCost(selectedTilemap.GetGrid().GetGridObject(neighbourNode.x, neighbourNode.z).GetMovementPenaltyType((int)type));
                        if(neighbourNode.moveCost == 0 || (blocker != null && blocker.GetTeam() != unit.GetTeam())) neighbourNode.SetMoveCost(4096);
                        replacer.Add(neighbourNode);
                    }
                }
                graphVertices.Add(checkerNode);
            }
            checkerList.Clear();
            checkerList.AddRange(replacer);
            replacer.Clear();
        }
        foreach(DijkstraNode remaining in checkerList)
        {
            graphVertices.Add(remaining);
        }


        replacer.AddRange(ApplyDijkstra(x, z, graphVertices, unit, selectedUnitmap, getWholeMap, true));

        return replacer;
    }

    public List<DijkstraNode> GetNeighbourList(DijkstraNode currentNode)
    {
        List<DijkstraNode> neighbourList = new List<DijkstraNode>();

        if (currentNode.x - 1 >= 0) neighbourList.Add(GetNode(currentNode.x - 1, currentNode.z));
        if (currentNode.x + 1 < grid.GetWidth()) neighbourList.Add(GetNode(currentNode.x + 1, currentNode.z));
        if (currentNode.z - 1 >= 0) neighbourList.Add(GetNode(currentNode.x, currentNode.z - 1));
        if (currentNode.z + 1 < grid.GetHeight()) neighbourList.Add(GetNode(currentNode.x, currentNode.z + 1));

        return neighbourList;
    }

    public DijkstraNode GetNode(int x, int y)
    {
        return grid.GetGridObject(x, y);
    }

    public void ClearGrid()
    {
        for (int x = 0; x < grid.GetWidth(); x++)
            {
                for (int z = 0; z < grid.GetHeight(); z++)
                {
                    DijkstraNode cleaner = grid.GetGridObject(x, z);
                    cleaner.SetMoveCost(0);
                    cleaner.connectedFromNodes.Clear();
                    grid.TriggerGenericGridChanged(cleaner.x, cleaner.z);
                }
            }
    }
}
