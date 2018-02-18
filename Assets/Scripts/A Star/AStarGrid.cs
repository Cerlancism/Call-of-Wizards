using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AStarGrid : MonoBehaviour
{
    public bool DisplayGridGizmos;

    public string[] UnwalkableTags;
    public Vector2 GridWorldSize;
    public float NodeRadius;
    AStarNode[,] grid;

    float nodeDiameter;
    int gridSizeX, gridSizeY;

    void Start()
    {
        nodeDiameter = NodeRadius * 2;
        gridSizeX = Mathf.RoundToInt(GridWorldSize.x / nodeDiameter);
        gridSizeY = Mathf.RoundToInt(GridWorldSize.y / nodeDiameter);

        CreateGrid();
    }

    public int MaxSize
    {
        get
        {
            return gridSizeX * gridSizeY;
        }
    }

    void CreateGrid()
    {
        grid = new AStarNode[gridSizeX, gridSizeY];
        Vector3 WorldbottomLeft = transform.position - Vector3.right * GridWorldSize.x / 2 - Vector3.forward * GridWorldSize.y / 2;

        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                Vector3 worldPoint = WorldbottomLeft + Vector3.right * (x * nodeDiameter + NodeRadius) + Vector3.forward * (y * nodeDiameter + NodeRadius);
                bool walkable = true;
                foreach (var collisions in Physics.OverlapSphere(worldPoint, NodeRadius))
                {
                    if (UnwalkableTags.Contains(collisions.tag))
                    {
                        walkable = false;
                        break;
                    }
                }
                grid[x, y] = new AStarNode(walkable, worldPoint, x, y);
            }
        }
        Debug.Log("A Star nodes counts: " + (grid.Length));
    }

    public AStarNode NodeFromWorldPoint(Vector3 worldPosition)
    {
        worldPosition.x -= transform.position.x;
        worldPosition.z -= transform.position.z;
        float percentX = Mathf.Clamp01((worldPosition.x + GridWorldSize.x / 2) / GridWorldSize.x);
        float percentY = Mathf.Clamp01((worldPosition.z + GridWorldSize.y / 2) / GridWorldSize.y);

        int x = Mathf.RoundToInt((gridSizeX - 1) * percentX);
        int y = Mathf.RoundToInt((gridSizeY - 1) * percentY);

        return grid[x, y];
    }

    public List<AStarNode> GetNeighbours(AStarNode node)
    {
        List<AStarNode> neighbours = new List<AStarNode>();

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0)
                {
                    continue;
                }

                int checkX = node.GridX + x;
                int checkY = node.GridY + y;

                if (checkX >= 0 && checkX < gridSizeX && checkY >= 0 && checkY < gridSizeY)
                {
                    neighbours.Add(grid[checkX, checkY]);
                }
            }
        }
        return neighbours;
    }

    void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position, new Vector3(GridWorldSize.x, 1, GridWorldSize.y));

        if (grid != null && DisplayGridGizmos)
        {
            foreach (var node in grid)
            {
                Gizmos.color = node.Walkable ? Color.white : Color.red;
                Gizmos.DrawCube(node.WorldPosition, Vector3.one * (nodeDiameter - 0.1f));
            }
        }
    }
}
