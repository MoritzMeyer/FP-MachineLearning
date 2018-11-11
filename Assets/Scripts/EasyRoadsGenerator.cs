using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EasyRoads3Dv3;
public class EasyRoadsGenerator : MonoBehaviour {
    public ERRoadNetwork network;
    public GameObject TestObject;
	// Use this for initialization
	void Start () {
        Vector3[] positionArray = new[] { new Vector3(0, 0, 0), new Vector3(0, 100, 0) };
        network = new ERRoadNetwork();
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
