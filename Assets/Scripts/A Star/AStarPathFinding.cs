using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AStarPathFinding : MonoBehaviour
{
    public Transform TestSeeker, TestTarget;

    AStarGrid grid;

    void Awake()
    {
        grid = GetComponent<AStarGrid>();
    }

    void Start()
    {
        
    }

    void Update()
    {
        FindPath(TestSeeker.position, TestTarget.position);
    }

    void FindPath(Vector3 startPosition, Vector3 targetPosition)
    {
        AStarNode start = grid.NodeFromWorldPoint(startPosition);
        AStarNode target = grid.NodeFromWorldPoint(targetPosition);

        Heap<AStarNode> openSet = new Heap<AStarNode>(grid.MaxSize);
        HashSet<AStarNode> closedSet = new HashSet<AStarNode>();
        openSet.Add(start);

        while (openSet.Count > 0)
        {
            AStarNode current = openSet.RemoveFirst();
            closedSet.Add(current);

            if (current == target)
            {
                RetracePath(start, target);
                return;
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
                }
            }
        }
    }

    internal void StartFindPath(Vector3 pathStart, Vector3 pathEnd)
    {
        throw new NotImplementedException();
    }

    void RetracePath(AStarNode start, AStarNode end)
    {
        List<AStarNode> path = new List<AStarNode>();
        AStarNode current = end;

        while (current != start)
        {
            path.Add(current);
            current = current.Parent;
        }
        path.Reverse();

        grid.path = path;
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
