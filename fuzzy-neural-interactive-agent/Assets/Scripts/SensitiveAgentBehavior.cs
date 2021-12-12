// creation: 03-oct-2021 pierre.chevaillier@enib.fr 
// creation: 07-nov-2021 pierre.chevaillier@enib.fr add comments

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SensitiveAgentBehavior : MonoBehaviour
{
    // General classes for state management (light version of the state design pattern)
    // Define here because only used here in this project.

    public abstract class State {
        public float entryTime;
        public readonly string name;
        public StateManager manager;

        public State(string name) { 
            this.name = name;
        }
        public State(string name, StateManager manager): this(name) {
            this.manager = manager;
        }

        // define the action to perform when activating the state (entry)
        public virtual void activate() { 
             this.entryTime = Time.time;    
        }

        // action performed while the stat is active (each frame)
        public abstract void update();
    }

    // A specific state: the intial one. 
    // As in UML, no action is performed in this state
    public class InitialState: State {
        public InitialState() : base("__Init__") {
        }

        public override void update() {
            // No action in initial state
            // empty in purpose
            return;
        }
    }

    public abstract class StateManager {
        // Components of the controlled agents: NYI

        public SensitiveAgentBehavior controlledBehavior;
        public State currentState;
        public Dictionary<string, State> states = new Dictionary<string, State>();

        public StateManager(SensitiveAgentBehavior behavior) {
            this.controlledBehavior = behavior;
        }

        protected void addState(State state) {
            this.states.Add(state.name, state);
            return;
        }

        public bool isState(string name) { return (!(this.currentState is null) && this.currentState.name.Equals(name)); }

        public virtual State initialize() {
            if (this.currentState is null) { // to prevend multi initializations, which won't be consistent 
                State init = new InitialState();
                this.addState(init);
                this.currentState = init;
            }
            Debug.Log("Initial State " + this.currentState.name);
            return this.currentState;
        }

        protected abstract State update();

        public State selectState() {
            State next = update();
            if (!(next is null) && (!isState(next.name))) { 
                // Warning: in case of a transition from state Si to state Si, we don not consider it as is.
                // therefore in this case the state Si is not (re)activated  
                Debug.Log("Current state has changed from " + this.currentState.name + " to " + next.name);
                this.currentState = next;
                this.currentState.activate();
            }
            return this.currentState;
        }
    }
    
    // ========================================================================
    // --- Specific classes for the types of behavior
    public class Cool: State {
        MeshRenderer renderer; // the agent aspect may differ, depending on its state => need to control the renderer
        Material material;     // and the material 
        public Cool(StateManager manager) : base("Cool", manager) {
            // need to retrieve the renderer and the material (never changed, thus can be done once in this constructor)
            this.renderer = this.manager.controlledBehavior.gameObject.GetComponent<MeshRenderer>();
            this.material = this.renderer.material;
        }
        public override void activate() {
            base.activate();
            this.material.color = Color.cyan; // change the agent's appearance 
            // Warning; the color should not be considered as a state variable
            // that another agent may perceive. 
            // It is set for the user to differenciate the two states

            // change the two parameters of the interaction
            // a negative valence means taht the agent tends to avoid the interacting object
            // In this example, a cool agent does not seek interactions
            this.manager.controlledBehavior.interactingObject.valence = -1.0f;
            this.manager.controlledBehavior.interactingObject.strength = .25f;

            // interaction with this agent when it is in this state provides a positive reward
            this.manager.controlledBehavior.rewardOnHit = 1;

            // reaction ot the controlled agent when entering in this state
            this.manager.controlledBehavior.moveToOtherPlace();
        }

        public override void update() { 
            // Modulate the reaction
            // the strength of the interaction decreaces over time
            // arbitratry law (linear for sake os simplicity)
            float s = this.manager.controlledBehavior.interactingObject.strength * (1.0f -.005f);
            s = Mathf.Max(.0f, s); // and remains a positive real number
            this.manager.controlledBehavior.interactingObject.strength = s;
            return;
        }
    }

    public class Angry: State {
        MeshRenderer renderer;
        Material material;
        public Angry(StateManager manager) : base("Angry", manager) {
            this.renderer = this.manager.controlledBehavior.gameObject.GetComponent<MeshRenderer>();
            this.material = this.renderer.material;
        }

        public override void activate() {
            base.activate();
            this.material.color = Color.red; // change the agent's appearance 

            // Change the two parameters of the interaction
            // a positive valence means that the agent is attracted by the interacting object
            // In this example, a cool Angry tends to hits its interacting agent
            this.manager.controlledBehavior.interactingObject.valence = 1.0f;
            this.manager.controlledBehavior.interactingObject.strength = 1.0f;

            // interaction with this agent when it is in this state provides a negative reward
            this.manager.controlledBehavior.rewardOnHit = -1;
            this.manager.controlledBehavior.moveToOtherPlace();
        }

        public override void update() {
            float s = this.manager.controlledBehavior.interactingObject.strength;
            if (this.manager.controlledBehavior.interactingObject.Distance > 4.0f) {
                s = this.manager.controlledBehavior.interactingObject.strength * (1.0f -.01f);
            }
            s = Mathf.Max(.0f, s);
            this.manager.controlledBehavior.interactingObject.strength = s;
            if (s < .01)
                this.manager.controlledBehavior.moodChange = true;
            return;
        }
    }

    public class BipolarBehavior: StateManager {
        public BipolarBehavior(SensitiveAgentBehavior behavior) : base(behavior) {
            // nothing specific to do here
        }

        public override State initialize() {            
            this.addState(new Cool(this));
            this.addState(new Angry(this));
            return base.initialize();
        }
        protected override State update() {
            State nextState = this.currentState;
            if (isState("__init__")) nextState = states["Cool"]; 
            else {
                float elapsedTime = Time.time - this.currentState.entryTime;
                //if (elapsedTime > 2.0) {
                if (this.controlledBehavior.moodChange) {
                    string nextStateName;
                    if (isState("Cool")) nextStateName = "Angry";
                    else nextStateName = "Cool"; 
                    nextState = states[nextStateName];
                    this.controlledBehavior.moodChange = false;
                } else if (isState("Angry") && (this.controlledBehavior.interactingObject.strength < .001f) && (this.controlledBehavior.interactingObject.Distance > 3.0f))
                    nextState = states["Cool"];
            }
            return nextState;
        }
    }

    public State currentState;
    StateManager behaviorSelector;
    public KinematicModel kinematicModel;
    public InteractiveObject interactingObject;

    public int rewardOnHit = 0;
    public Text hitsCounter;
    int sumOfRewards = 0;
    int nTouch = 0;

    public float minDistanceToTarget = 1.0f;
    public float areaHalfSize = 10.0f;
    const float shortDistance = 3.0f;
    const float largeAngle = 90.0f; //Mathf.PI / 2.0f;
    bool moodChange;

    // Start is called before the first frame update
    void Start()
    {
        this.interactingObject = GetComponent(typeof(InteractiveObject)) as InteractiveObject;
        DefineKinematicModel();
        this.moodChange = false;
        this.behaviorSelector = new BipolarBehavior(this);
        this.currentState = this.behaviorSelector.initialize();

        GameObject scoreDisplay = GameObject.Find("ScoreValue");
        hitsCounter = scoreDisplay.GetComponent(typeof(Text)) as Text;
        hitsCounter.text = sumOfRewards.ToString() + "/" + nTouch.ToString();
    }

    protected void DefineKinematicModel() {
        this.kinematicModel = gameObject.AddComponent(typeof(KinematicModel)) as KinematicModel;
        this.kinematicModel.DefineLinearVelocity(0.0f, 0.0f, 2.0f); // (min, cur, max)
        this.kinematicModel.DefineAngularVelocity(-90.0f, 0.0f, 90.0f); // (min, cur, max)
        this.kinematicModel.currentTransform = gameObject.transform;
    }

    void reactToAgent() {
        nTouch++;
        sumOfRewards += rewardOnHit;
        hitsCounter.text = sumOfRewards.ToString() + "/" + nTouch.ToString();
        this.moodChange = true; // TODO add some randomness
    }

    void moveToOtherPlace() {
        this.transform.position = new Vector3(Random.Range(-areaHalfSize, areaHalfSize), 0.5f, Random.Range(-areaHalfSize, areaHalfSize));
    }

    protected void SpeedControlledMove(float distanceToTarget, float angle, float targetVelocity) {
        float linearVelocity, angularVelocity;
        float angleInRadians = angle * Mathf.Deg2Rad;
        float interationValence = this.interactingObject.valence;
        float interactionStrength = this.interactingObject.strength;

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
            angularVelocity *= Mathf.Abs(Mathf.Sin(angleInRadians));
        }

        angularVelocity *= interationValence * interactionStrength;
        linearVelocity *= interactionStrength;
        kinematicModel.linearVelocity = linearVelocity;
        kinematicModel.angularVelocity = angularVelocity;
    }

    // Update is called once per frame
    void Update() {

        // --- Perception
        // simple here because only 1 single interacting object
        float targetDistance = interactingObject.Distance;
        float targetAzimuth = interactingObject.Azimuth;
        float targetVelocity = interactingObject.EstimatedLinearVelocity;
 
        // --- Decision 
        if (interactingObject.Distance < minDistanceToTarget) {
            reactToAgent();
        } else {
            SpeedControlledMove(targetDistance, targetAzimuth, targetVelocity);
        }

        // --- Action
        kinematicModel.Turn();
        kinematicModel.MoveForward();

        // --- Internal state update
        kinematicModel.UpdateState();
        this.currentState = this.behaviorSelector.selectState();
        this.currentState.update();
    }
}
