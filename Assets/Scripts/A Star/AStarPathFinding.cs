using System;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class AStarPathFinding : MonoBehaviour
{
    AStarPathRequestManager requestManager;
    AStarGrid grid;

    void Awake()
    {
        requestManager = GetComponent<AStarPathRequestManager>();
        grid = GetComponent<AStarGrid>();
    }

    public void StartFindPath(Vector3 start, Vector3 end)
    {
        StartCoroutine(FindPath(start, end));
    }

    IEnumerator FindPath(Vector3 startPosition, Vector3 targetPosition)
    {
        Vector3[] wayPoints = new Vector3[0];
        bool pathSuccess = false;

        AStarNode start = grid.NodeFromWorldPoint(startPosition);
        AStarNode target = grid.NodeFromWorldPoint(targetPosition);

        if (target.Walkable)
        {
            Heap<AStarNode> openSet = new Heap<AStarNode>(grid.MaxSize);
            HashSet<AStarNode> closedSet = new HashSet<AStarNode>();
            openSet.Add(start);

            while (openSet.Count > 0)
            {
                AStarNode current = openSet.RemoveFirst();
                closedSet.Add(current);

                if (current == target)
                {
                    pathSuccess = true;
                    break;
                }

                foreach (var neighbour in grid.GetNeighbours(current))
                {
                    if (!neighbour.Walkable || closedSet.Contains(neighbour))
                    {
                        continue;
                    }

                    int newMovementCostToNeighBour = current.GCost + GetDistance(current, neighbour);
                    if (newMovementCostToNeighBour < neighbour.GCost || !openSet.Contains(neighbour))
                    {
                        neighbour.GCost = newMovementCostToNeighBour;
                        neighbour.HCost = GetDistance(neighbour, target);
                        neighbour.Parent = current;
                        if (!openSet.Contains(neighbour))
                        {
                            openSet.Add(neighbour);
                        }
                        else
                        {
                            openSet.UpdateItem(neighbour);
                        }
                    }
                }
            }
        }
        yield return null;
        if (pathSuccess)
        {
            wayPoints = RetracePath(start, target);
        }
        requestManager.FinishProcessing(wayPoints, pathSuccess);
    }

    Vector3[] RetracePath(AStarNode start, AStarNode end)
    {
        List<AStarNode> path = new List<AStarNode>();
        AStarNode current = end;

        while (current != start)
        {
            path.Add(current);
            current = current.Parent;
        }
        var waypoints = SimplifyPath(path);
        Array.Reverse(waypoints);
        return waypoints;
    }

    Vector3[] SimplifyPath(List<AStarNode> path)
    {
        var waypoints = new List<Vector3>();
        Vector2 directionOld = Vector2.zero;

        for (int i = 1; i < path.Count; i++)
        {
            Vector2 directionNew = new Vector2(path[i - 1].GridX - path[i].GridX, path[i - 1].GridY - path[i].GridY);
            if (directionNew != directionOld)
            {
                waypoints.Add(path[i].WorldPosition);
            }
            directionOld = directionNew;
        }
        return waypoints.ToArray();
    }

    int GetDistance(AStarNode A, AStarNode B)
    {
        int distX = Mathf.Abs(A.GridX - B.GridX);
        int distY = Mathf.Abs(A.GridY - B.GridY);

        if (distX > distY)
        {
            return 14 * distY + 10 * (distX - distY);
        }
        else
        {
            return 14 * distX + 10 * (distY - distX);
        }
    }
}
