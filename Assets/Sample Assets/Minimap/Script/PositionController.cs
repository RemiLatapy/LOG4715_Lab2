using UnityEngine;
using System.Collections;

public class PositionController : MonoBehaviour {
	public Transform attachedCar;

	// Use this for initialization
	void Start () {
		this.transform.position = attachedCar.position;
	}
	
	// Update is called once per frame
	void Update () {
		this.transform.position = attachedCar.position;
	}
}
