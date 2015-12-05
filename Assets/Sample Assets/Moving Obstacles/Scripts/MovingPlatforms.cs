using UnityEngine;
using System.Collections;

public class MovingPlatforms : MonoBehaviour {
	public Transform movingPlatform;
	public Transform position1;
	public Transform position2;
	private Vector3 newPosition;
	private string currentState;
	public float smooth;

	// Use this for initialization
	void Start () {
		currentState = "";
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		if (currentState != "" && (movingPlatform.position != position1.position || movingPlatform.position != position2.position)) {
			movingPlatform.position = Vector3.Lerp (movingPlatform.position, newPosition, smooth * Time.deltaTime);
		}
	}

	public void ChangeTarget () {
		if (currentState == "Moving To Position 1") {
			currentState = "Moving To Position 2";
			newPosition = position2.position;
		} else if (currentState == "Moving To Position 2") {
			currentState = "Moving To Position 1";
			newPosition = position1.position;
		} else if (currentState == "") {
			currentState = "Moving To Position 2";
			newPosition = position2.position;
		}
	}
}
