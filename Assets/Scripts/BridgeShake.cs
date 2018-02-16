using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BridgeShake : MonoBehaviour
{
    public List<GameObject> ForcePoints;

    // Use this for initialization
    void Start()
    {
        InvokeRepeating("AddBridgeForce", 1, 1);
    }

    void AddBridgeForce()
    {
        foreach (var item in ForcePoints)
        {
            var body = item.GetComponent<Rigidbody>();
            body.AddForce(body.transform.forward * (100 - Random.Range(0, 100)));
        }
    }
}
