// creation: 09-jul-2018 pierre.chevaillier@b-com.com
// revision: 20-aug-2018 pierre.chevaillier@b-com.com

using UnityEngine;

public class KinematicModel : MonoBehaviour {

    public Transform currentTransform;

    public float linearVelocity = 0.0f;
    public float linearVelocityMin = 0.0f;
    public float linearVelocityMax = 1.0f;

    public void DefineLinearVelocity(float minValue, float currentValue, float maxValue) {
        this.linearVelocityMin = minValue;
        this.linearVelocityMax = maxValue;
        this.linearVelocity = currentValue;
        this.linearVelocity = Mathf.Min(this.linearVelocity, this.linearVelocityMax);
        this.linearVelocity = Mathf.Max(this.linearVelocity, this.linearVelocityMin);
    }

    public bool IsLinearVelocityOutOfRange() {
        return ((linearVelocity >= linearVelocityMax) || (linearVelocity <= linearVelocityMin));
    }

    public float linearAcceleration = 0.0f;
   
 
    public float angularVelocity = 0.0f;
    public float angularVelocityMin = -1.0f;
    public float angularVelocityMax = 1.0f;
    
    public void DefineAngularVelocity(float minValue, float currentValue, float maxValue) {
        this.angularVelocityMin = minValue;
        this.angularVelocityMax = maxValue;
        this.angularVelocity = currentValue;
        this.angularVelocity = Mathf.Min(this.angularVelocity, this.angularVelocityMax);
        this.angularVelocity = Mathf.Max(this.angularVelocity, this.angularVelocityMin);
    }

    public bool IsAngularVelocityOutOfRange() {
        return ((angularVelocity >= angularVelocityMax) || (angularVelocity <= angularVelocityMin));
    }

    public float angularAcceleration = 0.0f;

	// Use this for initialization
	void Start () {
        UpdateState();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void UpdateState() {
        currentTransform = gameObject.transform;
    }

    public void MoveForward()
    {
        float distanceOfMove = Time.deltaTime * this.linearVelocity;
        gameObject.transform.Translate(Vector3.forward * distanceOfMove, Space.Self);
    }

    public void Turn()
    {
        float angleOfTurn = Time.deltaTime * this.angularVelocity;
        //gameObject.transform.Rotate(gameObject.transform.up, angleOfTurn);
        gameObject.transform.Rotate(Vector3.up, angleOfTurn, Space.Self);
    }

    public void ApplyLinearAcceleration() {
        this.linearVelocity += linearAcceleration * Time.deltaTime;
        this.linearVelocity = Mathf.Min(this.linearVelocity, this.linearVelocityMax);
        this.linearVelocity = Mathf.Max(this.linearVelocity, this.linearVelocityMin);
        this.MoveForward();
    }

    public void ApplyAngularAcceleration()
    {
        this.angularVelocity += angularAcceleration * Time.deltaTime;
        this.angularVelocity = Mathf.Min(this.angularVelocity, this.angularVelocityMax);
        this.angularVelocity = Mathf.Max(this.angularVelocity, this.angularVelocityMin);
        Turn();
    }

    // TODO: not consistent anymore. Do not use as is. 
    bool CheckAccelerationControlledMove()
    {
        bool isOk = true;
        float distance = Vector3.Distance(gameObject.transform.position, currentTransform.position);
        float resultVelocity = distance / Time.deltaTime;
        if (resultVelocity > linearVelocity)
        {
            print("Error / linear velocity: " + resultVelocity + " / " + linearVelocity);
            isOk = false;
        }
        return isOk;
    }
    /*
    protected void TurnToward(Transform target, float angularAcceleration)
    {
        Vector3 targetDir = target.position - gameObject.transform.position;
        float angle = Vector3.SignedAngle(gameObject.transform.forward, targetDir, gameObject.transform.up);
        if (Mathf.Abs(angle) > Mathf.Epsilon)
        {
            //print(".....");
            angularAcceleration  = Mathf.Sign(angle) * Mathf.Abs(angularAcceleration);
            ApplyAngularAcceleration(angularAcceleration);

           // float localRotation = Mathf.Sign(angle) * Mathf.Min(Mathf.Abs(angle), Mathf.Abs(this.angularVelocity) * Time.deltaTime);
            //this.transform.Rotate(this.transform.up, localRotation);
        }
        else
        {
            this.angularVelocity = 0f;
        }
        print("angle: " + angle + " angular velo: " + this.angularVelocity);
    }
*/

}
