using UnityEngine;
using System.Collections;

public class WompScript : MonoBehaviour {
	private float startPointX;
	private float startPointY;
	[SerializeField]
	private bool horizontal;
	private float fallSpeed;
	bool reverse;

	// Use this for initialization
	void Start () {
		fallSpeed = 2f;
		startPointX = rigidbody.transform.position.x;
		startPointY = rigidbody.transform.position.y;
		reverse = !horizontal;
	}
	
	// Update is called once per frame
	void Update () {
		if (horizontal) {
			if (!reverse) {
				this.rigidbody.velocity = new Vector3 (5, 0, 0);
				reverse = rigidbody.transform.position.x >= startPointX + 25f;
			} else {
				this.rigidbody.velocity = new Vector3 (-5, 0, 0);
				reverse = !(rigidbody.transform.position.x <= startPointX);
			}
		} else {
			if (!reverse) {
				this.rigidbody.velocity = new Vector3 (0, -fallSpeed, 0);
				fallSpeed += 2;
				reverse = rigidbody.transform.position.y <= startPointY;
			} else {
				this.rigidbody.velocity = new Vector3 (0, 5, 0);
				if(rigidbody.transform.position.y >= startPointY + 10f) {
					reverse = false;
					fallSpeed = 2f;
				}
			}
		}
	}
}
