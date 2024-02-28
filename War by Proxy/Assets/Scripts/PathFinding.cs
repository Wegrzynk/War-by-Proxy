using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathFinding
{
    private const int MOVE_STRAIGHT_COST = 10;

    private GameGrid<PathNode> grid;
    private List<PathNode> openList;
    private List<PathNode> closedList;

    public PathFinding(int width, int height)
    {
        grid = new GameGrid<PathNode>(width, height, 10f, Vector3.zero, false, (GameGrid<PathNode> g, int x, int z) => new PathNode(g, x, z));
    }

    public List<PathNode> FindPath(int startX, int startZ, int endX, int endZ, Unit unit, Tilemap tilemap, Unitmap unitmap, bool ignoreBlockers , FogSystem fog)
    {
        PathNode startNode = grid.GetGridObject(startX, startZ);
        PathNode endNode = grid.GetGridObject(endX, endZ);

        openList = new List<PathNode> { startNode };
        closedList = new List<PathNode>();

        for(int x = 0; x < grid.GetWidth(); x++)
        {
            for(int z = 0; z < grid.GetHeight(); z++)
            {
                PathNode pathNode = grid.GetGridObject(x, z);
                pathNode.gCost = int.MaxValue;
                pathNode.CalculateFCost();
                pathNode.cameFromNode = null;
            }
        }

        startNode.gCost = 0;
        startNode.hCost = CalculateDistanceCost(startNode, endNode);
        startNode.CalculateFCost();

        while(openList.Count > 0)
        {
            PathNode currentNode = GetLowestFCostNode(openList);
            if(currentNode == endNode)
            {
                //Reached final node
                return CalculatePath(endNode);
            }

            openList.Remove(currentNode);
            closedList.Add(currentNode);

            foreach(PathNode neighbourNode in GetNeighbourList(currentNode))
            {
                if(closedList.Contains(neighbourNode)) continue;
                Unit blocker = null;
                //Debug.Log("Check whole logic for" + neighbourNode.x + "," + neighbourNode.z + ":" + (!ignoreBlockers) + "," + (fog != null) + "," + (!fog.GetGrid().GetGridObject(neighbourNode.x, neighbourNode.z).isFogged));
                if (!ignoreBlockers || !fog.GetGrid().GetGridObject(neighbourNode.x, neighbourNode.z).isFogged)
                {
                    //Debug.Log("Check logic for" + neighbourNode.x + "," + neighbourNode.z + ":" + (fog != null && !fog.GetGrid().GetGridObject(neighbourNode.x, neighbourNode.z).isFogged));
                    blocker = unitmap.GetGrid().GetGridObject(neighbourNode.x, neighbourNode.z);
                    if(blocker != null)
                    {
                        //Debug.Log("Found blocker unit on coordinates: " + neighbourNode.x + "," + neighbourNode.z);
                        //Debug.Log(fog.GetGrid().GetGridObject(neighbourNode.x, neighbourNode.z).isFogged);
                    }
                }
                if(tilemap.GetGrid().GetGridObject(neighbourNode.x, neighbourNode.z).GetMovementPenaltyType(unit) == 0 || (blocker != null && blocker.GetTeam() != unit.GetTeam()))
                {
                    closedList.Add(neighbourNode);
                    continue;
                }

                //int tentativeGCost;
                int tentativeGCost = currentNode.gCost + tilemap.GetGrid().GetGridObject(neighbourNode.x, neighbourNode.z).GetMovementPenaltyType(unit);
                //tentativeGCost = currentNode.gCost + CalculateDistanceCost(currentNode, neighbourNode);
                if(tentativeGCost < neighbourNode.gCost)
                {
                    neighbourNode.cameFromNode = currentNode;
                    neighbourNode.gCost = tentativeGCost;
                    neighbourNode.hCost = CalculateDistanceCost(neighbourNode, endNode);
                    neighbourNode.CalculateFCost();

                    if(!openList.Contains(neighbourNode))
                    {
                        openList.Add(neighbourNode);
                    }
                }
            }
        }

        // Out of nodes on the openList
        return null;
    }

    private List<PathNode> GetNeighbourList(PathNode currentNode)
    {
        List<PathNode> neighbourList = new List<PathNode>();

        //Left
        if(currentNode.x - 1 >= 0) neighbourList.Add(GetNode(currentNode.x - 1, currentNode.z));
        //Right
        if(currentNode.x + 1 < grid.GetWidth()) neighbourList.Add(GetNode(currentNode.x + 1, currentNode.z));
        //Down
        if(currentNode.z - 1 >= 0) neighbourList.Add(GetNode(currentNode.x, currentNode.z - 1));
        //Up
        if(currentNode.z + 1 < grid.GetHeight()) neighbourList.Add(GetNode(currentNode.x, currentNode.z + 1));
    
        return neighbourList;
    }

    private PathNode GetNode(int x, int z)
    {
        return grid.GetGridObject(x, z);
    }

    private List<PathNode> CalculatePath(PathNode endNode)
    {
        List<PathNode> path = new List<PathNode>();
        path.Add(endNode);
        PathNode currentNode = endNode;
        while(currentNode.cameFromNode != null)
        {
            path.Add(currentNode.cameFromNode);
            currentNode = currentNode.cameFromNode;
        }
        path.Reverse();
        return path;
    }

    private int CalculateDistanceCost(PathNode a, PathNode b)
    {
        int xDistance = Mathf.Abs(a.x - b.x);
        int zDistance = Mathf.Abs(a.z - b.z);
        //int remaining = Mathf.Abs(xDistance - zDistance);
        return xDistance + zDistance;
    }

    private PathNode GetLowestFCostNode(List<PathNode> pathNodeList)
    {
        PathNode lowestFCostNode = pathNodeList[0];
        for (int i = 1; i < pathNodeList.Count; i++)
        {
            if(pathNodeList[i].fCost < lowestFCostNode.fCost)
            {
                lowestFCostNode = pathNodeList[i];
            }
        }
        return lowestFCostNode;
    }
}
