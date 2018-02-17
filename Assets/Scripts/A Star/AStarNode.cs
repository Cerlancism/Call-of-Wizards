using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AStarNode : IHeapItem<AStarNode>
{
    public bool Walkable;
    public Vector3 WorldPosition;
    public int GridX;
    public int GridY;

    public int GCost;
    public int HCost;

    int heapIndex;

    public AStarNode Parent;

    public AStarNode(bool walkable, Vector3 worldposition, int x, int y)
    {
        Walkable = walkable;
        WorldPosition = worldposition;
        GridX = x;
        GridY = y;
    }

    public int FCost
    {
        get
        {
            return GCost + HCost;
        }
    }

    public int HeapIndex
    {
        get
        {
            return heapIndex;
        }
        set
        {
            heapIndex = value;
        }
    }

    public int CompareTo(AStarNode toCompare)
    {
        int compare = FCost.CompareTo(toCompare.FCost);
        if (compare == 0)
        {
            compare = HCost.CompareTo(toCompare.HCost);
        }
        return -compare;
    }
}
