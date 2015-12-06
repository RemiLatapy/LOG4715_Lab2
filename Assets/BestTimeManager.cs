using UnityEngine;
using UnityEngine.UI;

public class BestTimeManager : MonoBehaviour {

	Text text;

	void Awake() {
		text = GetComponent<Text> ();
		text.enabled = false;
	}

	public void UpdateFinalTime(string formatedTime)
	{
		text.text = "Meilleur temps\n" + formatedTime;
	}
}
