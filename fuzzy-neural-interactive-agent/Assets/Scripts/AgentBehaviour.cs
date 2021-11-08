using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentBehaviour : MonoBehaviour {

    KinematicModel kinematicModel;
    InteractiveObject interactingObject;
    ObstacleDetector obstacleDetector;

    const float shortDistance = 3.0f;
    const float largeAngle = 90.0f; //Mathf.PI / 2.0f;

    public bool isFearfull = false; 
    public float fear;

	// Use this for initialization
	void Start () {
        gameObject.transform.position = new Vector3(-7f, 0.5f, 0f);

        this.interactingObject = GetComponent(typeof(InteractiveObject)) as InteractiveObject;
        this.obstacleDetector = GetComponent(typeof(ObstacleDetector)) as ObstacleDetector;
        DefineKinematicModel();

        isFearfull = false;
	}
	
    protected void DefineKinematicModel() {
        kinematicModel = gameObject.AddComponent(typeof(KinematicModel)) as KinematicModel;
        kinematicModel.DefineLinearVelocity(0.0f, 0.0f, 1.0f); // (min, cur, max)
        kinematicModel.DefineAngularVelocity(-50.0f, 0.0f, 50.0f); // (min, cur, max)
        kinematicModel.currentTransform = gameObject.transform;
    }

	// Update is called once per frame
	void Update () {
        // --- Perception
        float distanceToTarget = interactingObject.Distance;
        float angle = interactingObject.Azimuth;
        float targetVelocity = 0.0f;
        //print("Dist: " + distanceToTarget + " - angle: " + angle + " - forward: " + gameObject.transform.forward);

        // --- Decision 
        if (this.obstacleDetector == null || !this.obstacleDetector.isActiveAndEnabled)
            SpeedControlledMove(distanceToTarget, angle, targetVelocity);
        else
            headToTargetAndAvoidObstacles();

        // --- Action
        kinematicModel.Turn();
        kinematicModel.MoveForward();

        // --- Internal state update
        kinematicModel.UpdateState();
        if (isFearfull && fear > .0f) fear -= .1f * Time.deltaTime;
	}

    protected void headToTargetAndAvoidObstacles()
    {
        // sensors' values
        float distanceToTarget = interactingObject.Distance;
        float angle = interactingObject.Azimuth * Mathf.Deg2Rad;
        float distF = this.obstacleDetector.aheadDistance;
        float distL = this.obstacleDetector.onTheLeftDistance;
        float distR = this.obstacleDetector.onTheRightDistance;

        float vLin = kinematicModel.linearVelocity;
        float vAng = kinematicModel.angularVelocity;

        // Actuators' values
        float linearVelocity, angularVelocity;

        linearVelocity = kinematicModel.linearVelocityMax;

        float d = Mathf.Min(distF, distanceToTarget);
        float dSide = Mathf.Min(distL, distR);
        if (d < shortDistance)
        {
            linearVelocity = (d / shortDistance) * kinematicModel.linearVelocityMax;
        }
        else
        {
            if (dSide < 1.0f)
            {
                linearVelocity = (dSide / 1.0f)
                    * (kinematicModel.linearVelocityMax - 0.1f) + 0.1f;
            }
            else
            {
                if (Mathf.Abs(angle) < largeAngle / 2.0f)
                    linearVelocity *= Mathf.Abs(Mathf.Cos(angle));
                else
                    linearVelocity *= 1f;
            }
        }

        // turn 
        if (dSide < 1.0f)
        {
            angularVelocity = 0.0f;
            if (distR < 1.0f)
                angularVelocity = -0.1f * kinematicModel.angularVelocityMax * ( 1.0f - distR) / distF;
            if (distL < 1.0f)
                angularVelocity += 0.1f * kinematicModel.angularVelocityMax * (1.0f - distL) / distF;
        }
        else
        {
            angularVelocity = Mathf.Sign(angle) * kinematicModel.angularVelocityMax;
            if (Mathf.Abs(angle) < 0.75f)
            {
                angularVelocity = 0.0f;
            }
            else if (Mathf.Abs(angle) < largeAngle)
            {
                //angularVelocity *= Mathf.Abs(angle) / largeAngle;
                angularVelocity *= Mathf.Abs(Mathf.Sin(angle));
            }

        }

        // apply command
        kinematicModel.linearVelocity = linearVelocity;
        kinematicModel.angularVelocity = angularVelocity;
    }

    protected void SpeedControlledMove(float distanceToTarget, float angle, float targetVelocity) {
        float linearVelocity, angularVelocity;
        float angleInRadians = angle * Mathf.Deg2Rad;

        if (distanceToTarget < shortDistance)
            linearVelocity = (distanceToTarget / shortDistance)
                * (kinematicModel.linearVelocityMax - targetVelocity) + targetVelocity;
        else
            linearVelocity = kinematicModel.linearVelocityMax;

        if (Mathf.Abs(angle) < largeAngle / 2.0f)
            linearVelocity *= Mathf.Abs(Mathf.Cos(angleInRadians));
        else
            linearVelocity *= 1f;
        
        angularVelocity = Mathf.Sign(angle) * kinematicModel.angularVelocityMax;
        if (Mathf.Abs(angle) < 0.75f) {
            angularVelocity = 0.0f;
        } else if (Mathf.Abs(angle) < largeAngle) {
            //angularVelocity *= Mathf.Abs(angle) / largeAngle;
            angularVelocity *= Mathf.Abs(Mathf.Sin(angleInRadians));
        }

        //print("Lin velo:" + linearVelocity + " - Ang velo: " + angularVelocity);
        kinematicModel.linearVelocity = linearVelocity;
        kinematicModel.angularVelocity = angularVelocity;
    }

}
