using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ScoreManager : MonoBehaviour {

	public static int score;

	Text text;

	void Awake() {
		text = GetComponent<Text> ();
		score = 0;
	}

	void Update () {
		score = score < 0 ? 0 : score;
		text.text = "Score: " + score;
	}
}
