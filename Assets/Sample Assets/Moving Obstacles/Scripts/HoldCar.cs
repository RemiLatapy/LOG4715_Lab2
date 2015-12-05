using UnityEngine;
using System.Collections;

public class HoldCar : MonoBehaviour {
	public MovingPlatforms mp;

	IEnumerator OnTriggerEnter(Collider col) {
		Debug.Log (col.transform);
		yield return new WaitForSeconds(2.3f);
		col.transform.parent.parent.parent.parent = gameObject.transform;
		mp.ChangeTarget ();
	}

	IEnumerator OnTriggerExit(Collider col) {
		col.transform.parent.parent.parent.parent = null;
		yield return new WaitForSeconds(2);
		mp.ChangeTarget ();
	}
}
