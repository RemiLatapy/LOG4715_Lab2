using UnityEngine;
using System.Collections;

public class OutOfTrack : MonoBehaviour {

	CheckpointManager checkpointManager;

	void Awake () {
		checkpointManager = GameObject.Find ("Game Manager").GetComponent<CheckpointManager>();
	}

	void OnTriggerEnter(Collider other) {
		CarController car = other.GetComponentInParent<CarController> ();
		if (car != null) {
			car.modifyStyleScore(-200, "Out of track ! -200 !");
			Checkpoint lastCheckpoint = checkpointManager.GetLastCheckpoint(car);
			Vector3 respawnPosition = lastCheckpoint.transform.position;
			respawnPosition.y = 3f;
			Quaternion respawnRotation = lastCheckpoint.transform.rotation;

			car.transform.position = respawnPosition;
			car.rigidbody.rotation = respawnRotation;

			car.rigidbody.velocity = Vector3.zero;
			car.rigidbody.angularVelocity = Vector3.zero;
		}
		
	}
}
