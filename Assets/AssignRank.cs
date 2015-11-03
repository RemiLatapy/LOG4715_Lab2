using UnityEngine;
using System.Collections;

public class AssignRank : MonoBehaviour {

	// Use this for initialization
	void Start () {

		int i = 1;
		foreach (CarController car in this.GetComponentsInChildren<CarController>(true))
		{
			car.rank=i;
			i++;
		}
	
	}
}
