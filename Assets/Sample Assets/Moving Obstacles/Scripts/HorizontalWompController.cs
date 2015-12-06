using UnityEngine;
using System.Collections;

public class HorizontalWompController : MonoBehaviour {
	private float startPointX;
	private float startPointZ;
	[SerializeField]
	private float speed;
	[SerializeField]
	private bool zAxis;
	[SerializeField]
	private bool oppositeDirection;
	bool reverse;
	
	
	// Use this for initialization
	void Start () {
		startPointX = rigidbody.transform.position.x;
		startPointZ = rigidbody.transform.position.z;
		reverse = false;
	}
	
	// Update is called once per frame
	void Update () {
		if (!reverse) {
			if (!zAxis) {
				if(oppositeDirection){
					this.rigidbody.velocity = new Vector3 (-speed, 0, 0);
					reverse = rigidbody.transform.position.x <= startPointX - 25f;
				}
				else {
					this.rigidbody.velocity = new Vector3 (speed, 0, 0);
					reverse = rigidbody.transform.position.x >= startPointX + 25f;
				}
			} else if (zAxis) {
				if(oppositeDirection){
					this.rigidbody.velocity = new Vector3 (0, 0, -speed);
					reverse = rigidbody.transform.position.z <= startPointZ - 25f;
				}
				else {
					this.rigidbody.velocity = new Vector3 (0, 0, speed);
					reverse = rigidbody.transform.position.z >= startPointZ + 25f;
				}
			}
		} else {
			if (!zAxis) {
				if(oppositeDirection){
					this.rigidbody.velocity = new Vector3 (speed, 0, 0);
					reverse = !(rigidbody.transform.position.x >= startPointX);
				}
				else {
					this.rigidbody.velocity = new Vector3 (-speed, 0, 0);
					reverse = !(rigidbody.transform.position.x <= startPointX);
				}

			} else if (zAxis) {
				if(oppositeDirection){
					this.rigidbody.velocity = new Vector3 (0, 0, speed);
					reverse = !(rigidbody.transform.position.z >= startPointZ);
				}
				else {
					this.rigidbody.velocity = new Vector3 (0, 0, -speed);
					reverse = !(rigidbody.transform.position.z <= startPointZ);;
				}

			}
		}
	}
}
