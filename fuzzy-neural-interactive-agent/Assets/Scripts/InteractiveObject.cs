// creation: 23-aug-2018 pierre.chevaillier@enib.fr
// revision: 29-sep-2021 pierre.chevaillier@enib.fr valence and strenght of the ineraction, estimated DirOfMove and velocity. 

using UnityEngine;

public class InteractiveObject : MonoBehaviour {

    public string objectName;
    public GameObject interactingObject;
    public bool found = false;

    public Transform objectCurrentTransform;

    public Vector3 directionOfMove;
    public Transform lastObservedTransform;
    float elapsedTime; // = .0f;
    float observationPeriod = 1.0f / 10.0f;
    float timeSinceLastObservation; // = .0f;

    public float valence;
    public float strength; 
 
    
    public float Distance {
        get {
            return Vector3.Distance(objectCurrentTransform.position, 
                                    gameObject.transform.position);
        }
    }

    public float Azimuth {
        get {
            Vector3 targetDir = objectCurrentTransform.position - gameObject.transform.position;
            return Vector3.SignedAngle(gameObject.transform.forward, targetDir, Vector3.up);
        }
    }

    public float EstimatedLinearVelocity {
        get {
            return this.directionOfMove.magnitude;
        }
    }

	// Use this for initialization
	void Start () {
        Find();
        this.lastObservedTransform = this.interactingObject.transform;
	}
	
    bool Find() {
        if (! found) {
            this.interactingObject = GameObject.Find(objectName);
            if (this.interactingObject == null)
                print("Cannot find the object named " + objectName);
            else
                this.objectCurrentTransform = this.interactingObject.transform;
        }
        found = (this.interactingObject == null);
        return found;
    }

    void estimateVelocity() {
        elapsedTime += Time.deltaTime;
        timeSinceLastObservation += Time.deltaTime;
        if (timeSinceLastObservation> observationPeriod) {
            this.directionOfMove = this.objectCurrentTransform.position - this.lastObservedTransform.position;
            timeSinceLastObservation = .0f;
        }
    }
	// Update is called once per frame
	void Update () {
		estimateVelocity();
	}
}
