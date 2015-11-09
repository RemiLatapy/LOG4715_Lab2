using UnityEngine;
using System.Collections;

public class DestructionManager : MonoBehaviour {

	void OnTriggerEnter(Collider other) {
		this.GetComponent<Collider>().enabled = false;
		CarController car = other.GetComponentInParent<CarController> ();
		if (car != null) {
			car.modifyStyleScore (500, "BOUM ! +500");
		}
	}
}
