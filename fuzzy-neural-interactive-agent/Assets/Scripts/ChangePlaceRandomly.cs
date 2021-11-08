// creation:   -jul-2018 pierre.chevaillier@b-com.com
// revision: 24-aug-2018 pierre.chevaillier@enib.fr InteractiveObject
// revision: 16-sep-2021 pierre.chevaillier@enib.fr number of hits display

using UnityEngine;
using UnityEngine.UI;

public class ChangePlaceRandomly : MonoBehaviour {
    InteractiveObject interactingObject;

    public float minDistanceToTarget = 1.0f;
    public float areaHalfSize = 5.0f;
    public float distanceFromInteractingObject;

    public Text hitsCounter;
    int nTouch = 0;

	// Use this for initialization
	void Start () {
        interactingObject = GetComponent(typeof(InteractiveObject)) as InteractiveObject;
        GameObject scoreDisplay = GameObject.Find("ScoreValue");
        hitsCounter = scoreDisplay.GetComponent(typeof(Text)) as Text;
        hitsCounter.text = nTouch.ToString();
    }

    void reactToAgent() {
        nTouch++;
        hitsCounter.text = nTouch.ToString();
        this.transform.position = RandomPlace();
    }

    void OnCollisionEnter(Collision collision)
    {
        this.transform.position = RandomPlace();
    }

    protected Vector3 RandomPlace() {
        return new Vector3(Random.Range(-areaHalfSize, areaHalfSize), 0.5f, Random.Range(-areaHalfSize, areaHalfSize));
    }

	// Update is called once per frame
	void Update () {
        distanceFromInteractingObject = interactingObject.Distance;
        if (distanceFromInteractingObject < minDistanceToTarget) {
            reactToAgent();
        }
	}
}
