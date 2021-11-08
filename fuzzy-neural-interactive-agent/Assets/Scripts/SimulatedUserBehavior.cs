// description: simulate the behavior of the user
//              as a moving point on the horizontal plan 
// creation: 22-jun-2018 pierre.chevaillier@b-com.com
// revision: 27-jun-2018 pierre.chevaillier@b-com.com
// revision: 09-jul-2018 pierre.chevaillier@b-com.com kinematicModel
// revision: 24-aug-2018 pierre.chevaillier@b-com.com new kinematicModel
// comments: unused yet
// warning: not validated

using UnityEngine;

public class SimulatedUserBehavior : MonoBehaviour {
    KinematicModel kinematicModel;

	// Use this for initialization
	void Start () {
        DefineKinematicModel();
	}
	
	// Update is called once per frame
	void Update () {
        if (kinematicModel.IsLinearVelocityOutOfRange())
            kinematicModel.linearAcceleration *= -1.0f;
        if (kinematicModel.IsAngularVelocityOutOfRange())
            kinematicModel.angularAcceleration *= -1.0f;
        
        this.ApplyAccelerations();
	}

    protected void DefineKinematicModel() {
        this.kinematicModel = gameObject.AddComponent(typeof(KinematicModel)) as KinematicModel;
        this.kinematicModel.DefineLinearVelocity(0.0f, 0.0f, 10.0f);
        this.kinematicModel.DefineAngularVelocity(-90.0f, 0.0f, 90.0f);
        this.kinematicModel.linearAcceleration = 2.0f;
        this.kinematicModel.angularAcceleration = 10.0f;
    }

    protected void ApplyAccelerations() {
        // set the direction of move
        kinematicModel.ApplyAngularAcceleration();
        
        // and then move forward in this new direction
        kinematicModel.ApplyLinearAcceleration();
    }
}
