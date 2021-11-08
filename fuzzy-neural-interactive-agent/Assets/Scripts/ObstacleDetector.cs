using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleDetector : MonoBehaviour {
    public float aheadDistance;
    public float onTheLeftDistance;
    public float onTheRightDistance;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        Vector3 v;
        v = transform.TransformDirection(Vector3.forward);
        this.aheadDistance = this.DetectObstacle(v, Color.blue);
        v = Quaternion.AngleAxis(-45, Vector3.up) * transform.TransformDirection(Vector3.forward);
        this.onTheLeftDistance = this.DetectObstacle(v, Color.red);
        v = Quaternion.AngleAxis(45, Vector3.up) * transform.TransformDirection(Vector3.forward);
        this.onTheRightDistance = this.DetectObstacle(v, Color.green);
    }

    float DetectObstacle(Vector3 rayDirection, Color rayColor)
    {
        float distance = .0f;
        RaycastHit hit;
        if (Physics.Raycast(transform.position, rayDirection, out hit, Mathf.Infinity))
        {
            distance = hit.distance;
            Debug.DrawRay(transform.position, rayDirection * distance, rayColor);
            Debug.Log("Object at distance: " + distance);
        }
        else
        {
            distance = 1000f;
            Debug.Log("No Object detected: " + distance);
            Debug.DrawRay(transform.position, rayDirection * distance, Color.white);
        }
        return distance;
    }
}
