using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnArea : MonoBehaviour {
    public Vector3 GetRandomSpawnPosition()
    {
        return new Vector3(
            Random.Range(transform.position.x - transform.lossyScale.x / 2, transform.position.x + transform.lossyScale.x / 2),
            Random.Range(transform.position.y - transform.lossyScale.y / 2, transform.position.y + transform.lossyScale.y / 2),
            Random.Range(transform.position.z - transform.lossyScale.z / 2, transform.position.z + transform.lossyScale.z / 2));
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position, transform.lossyScale);
    }
}
