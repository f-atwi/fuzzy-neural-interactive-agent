using UnityEngine;

public class KBVelocityController : MonoBehaviour {

    public KinematicModel kinematicModel;

    public float linearVelocity;
    float linearVeloSensitivity = 0.01f;
    public float angularVelocity;
    float angularVeloSensitivity = 1.0f;

	// Use this for initialization
	public void Start () {
        kinematicModel = GetComponent(typeof(KinematicModel)) as KinematicModel;
        DefineKinematicModel();
	}
	
	// Update is called once per frame
    void Update () {
        this.linearVelocity = kinematicModel.linearVelocity;
        this.angularVelocity = kinematicModel.angularVelocity;

        userAction();
        speedDown();

        this.linearVelocity = Mathf.Clamp(this.linearVelocity, 
                                          kinematicModel.linearVelocityMin, 
                                          kinematicModel.linearVelocityMax);
        this.angularVelocity = Mathf.Clamp(this.angularVelocity, 
                                           kinematicModel.angularVelocityMin, 
                                           kinematicModel.angularVelocityMax);
        // Apply changes
        kinematicModel.linearVelocity = this.linearVelocity;
        kinematicModel.angularVelocity = this.angularVelocity;
        kinematicModel.Turn();
        kinematicModel.MoveForward();
	}

    protected void DefineKinematicModel()
    {
        kinematicModel = gameObject.AddComponent(typeof(KinematicModel)) as KinematicModel;
        kinematicModel.DefineLinearVelocity(-2.0f, 0.0f, 5.0f); // (min, cur, max)
        kinematicModel.DefineAngularVelocity(-50.0f, 0.0f, 50.0f); // (min, cur, max)
        kinematicModel.currentTransform = gameObject.transform;
    }

   private void userAction() {
        // keys for controlling the linear velocity
        if (Input.GetKey(KeyCode.UpArrow))
            this.linearVelocity += linearVeloSensitivity;
        if (Input.GetKey(KeyCode.DownArrow))
            this.linearVelocity -= linearVeloSensitivity;
        
        // keys for controlling the angular velocity
        if (Input.GetKey(KeyCode.LeftArrow))
            this.angularVelocity -= angularVeloSensitivity;
        if (Input.GetKey(KeyCode.RightArrow))
            this.angularVelocity += angularVeloSensitivity;
    }

    private void speedDown() {
        float linearVeloChange = this.linearVelocity - kinematicModel.linearVelocity;
        if ((Mathf.Abs(this.linearVelocity) > Mathf.Epsilon) && (Mathf.Abs(linearVeloChange) < Mathf.Epsilon))
            this.linearVelocity = kinematicModel.linearVelocity 
                - Mathf.Sign(kinematicModel.linearVelocity) * this.linearVeloSensitivity;

        float angularVeloChange = this.angularVelocity - kinematicModel.angularVelocity;
        if ((Mathf.Abs(this.angularVelocity) > Mathf.Epsilon) && (Mathf.Abs(angularVeloChange) < Mathf.Epsilon))
            this.angularVelocity = kinematicModel.angularVelocity 
                - Mathf.Sign(kinematicModel.angularVelocity) * this.angularVeloSensitivity;
    }
}
