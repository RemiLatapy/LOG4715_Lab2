using UnityEngine;
using System.Collections;

public class HoldCar : MonoBehaviour {
	public MovingPlatforms mp;

	IEnumerator OnTriggerEnter(Collider col) {
		if(col.transform.CompareTag("PlatformCollider")){
			yield return new WaitForSeconds(2.3f);
			//mp.ChangeTarget ();
		}
	}

	void OnTriggerStay(Collider col) {
		if(col.transform.CompareTag("PlatformCollider")){
			//yield return new WaitForSeconds(2.3f);
			col.transform.parent.parent.parent.parent = gameObject.transform;

		}
	}

	IEnumerator OnTriggerExit(Collider col) {
		if (col.transform.CompareTag ("PlatformCollider")) {
			col.transform.parent.parent.parent.parent = null;
			yield return new WaitForSeconds (2);
			//mp.ChangeTarget ();
		}
	}
}
