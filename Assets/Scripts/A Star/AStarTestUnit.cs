using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AStarTestUnit : MonoBehaviour
{
    public Transform Target;
    public float Speed = 20;

    Vector3[] path;
    int targetIndex;

    // Use this for initialization
    void Start()
    {
        InvokeRepeating("GetPath", 1, 1);
    }

    // Update is called once per frame
    void Update()
    {

    }

    void GetPath()
    {
        AStarPathRequestManager.RequestPath(transform.position, Target.position, OnPathFound);
    }

    void OnPathFound(Vector3[] newPath, bool pathSuccessful)
    {
        if (pathSuccessful)
        {
            path = newPath;
            targetIndex = 0;
            StopCoroutine("FollowPath");
            StartCoroutine("FollowPath");
        }
    }

    IEnumerator FollowPath()
    {
        if (path.Length > 1)
        {
            Vector3 currentWayPoint = path[0];
            while (true)
            {
                if (transform.position == currentWayPoint)
                {
                    targetIndex++;
                    if (targetIndex >= path.Length)
                    {
                        yield break;
                    }
                    currentWayPoint = path[targetIndex];
                }

                transform.position = Vector3.MoveTowards(transform.position, currentWayPoint, Speed * Time.deltaTime);
                yield return null;
            }
        }
    }

    public void OnDrawGizmos()
    {
        if (path != null)
        {
            for (int i = targetIndex; i < path.Length; i++)
            {
                Gizmos.color = Color.black;
                Gizmos.DrawCube(path[i], Vector3.one);

                Gizmos.color = Color.white;
                if (i == targetIndex)
                {
                    Gizmos.DrawLine(transform.position, path[i]);
                }
                else
                {
                    Gizmos.DrawLine(path[i - 1], path[i]);
                }
            }
        }
    }
}
