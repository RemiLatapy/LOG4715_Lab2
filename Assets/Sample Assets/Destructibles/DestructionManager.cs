using UnityEngine;
using System.Collections;

public class DestructionManager : MonoBehaviour {

	void OnTriggerEnter(Collider other) {
		CarController car = other.GetComponentInParent<CarController> ();
		if (car != null) {
			this.GetComponent<Collider>().enabled = false;
			car.modifyStyleScore (500, "BOUM ! +500");
		}
	}
}
