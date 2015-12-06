using UnityEngine;
using System.Collections;

public class VerticalWompController : MonoBehaviour {
	private float startPointY;
	[SerializeField]
	private float speed;
	[SerializeField]
	private float fallSpeed;
	bool reverse;
	private float initializedFallSpeed;

	// Use this for initialization
	void Start () {
		startPointY = rigidbody.transform.position.y;
		initializedFallSpeed = fallSpeed;
		reverse = true;
	}
	
	// Update is called once per frame
	void Update () {
		if (!reverse) {
			this.rigidbody.velocity = new Vector3 (0, -fallSpeed, 0);
			fallSpeed += 2;
			reverse = rigidbody.transform.position.y <= startPointY;
		} else {
			this.rigidbody.velocity = new Vector3 (0, speed, 0);
			if(rigidbody.transform.position.y >= startPointY + 10f) {
				reverse = false;
				fallSpeed = initializedFallSpeed;
			}
		}
	}
}
