using UnityEngine;
using System.Collections;

public class Carapace : MonoBehaviour {

	[SerializeField]public float speed=10;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		rigidbody.AddForce(transform.forward*speed,ForceMode.VelocityChange);
		/*Vector3 pos=transform.position;
		pos.y=Terrain.activeTerrain.SampleHeight(transform.position);
		transform.position=pos;*/

	}

}
