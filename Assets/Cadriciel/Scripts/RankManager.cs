using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class RankManager : MonoBehaviour {
	
	public static int rank;
	
	Text text;
	
	void Awake() {
		text = GetComponent<Text> ();
		rank = 0;
	}
	
	void Update () {
		text.text = rank + "/" + RaceManager.numberOfCars;
	}
}
