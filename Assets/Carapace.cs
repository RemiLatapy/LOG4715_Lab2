using UnityEngine;
using System.Collections;

public class Carapace : MonoBehaviour {

	[SerializeField]public float speed=10;
	[SerializeField]public int maxRebonds=3;
	private int rebonds;
	// Use this for initialization
	void Start () {
		rebonds=0;
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		rigidbody.AddForce(transform.forward*speed,ForceMode.VelocityChange);
	}
	void OnTriggerEnter(Collider other) {
		Debug.Log ("Prout");
		if(other.gameObject.CompareTag("WallCollider")||other.gameObject.CompareTag("Obstacle"))
		{
			Debug.Log ("Wall");
			rebonds++;
		}
		else if(other.gameObject.CompareTag("CarCollider"))
		{
			Debug.Log ("Car");
			Destroy(gameObject,0.1f);
		}
	}
	
	void Update()
	{
		if(rebonds>=maxRebonds)
			Destroy(gameObject,0.2f);
	}

}
