// creation: 24-aug-2018 pierre.chevaillier@enib.fr

using UnityEngine;

public class KBPositionController : MonoBehaviour {

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
        Vector3 move = new Vector3();
        Vector3 rotation = new Vector3();

        if (Input.GetKey(KeyCode.UpArrow))
            move.z += 0.01f;
        if (Input.GetKey(KeyCode.DownArrow))
            move.z -= 0.01f;

        if (Input.GetKey(KeyCode.LeftArrow))
            rotation.y -= 1.0f;
        if (Input.GetKey(KeyCode.RightArrow))
            rotation.y += 1.0f;

        transform.Translate(Vector3.forward * move.z, Space.Self);
        transform.Rotate(Vector3.up, rotation.y, Space.Self);
	}
}
