using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PressSpaceManager : MonoBehaviour {

	Text text;
	public int delay = 10;
	
	void Awake() {
		text = GetComponent<Text> ();
		text.enabled = false;
	}
	
	public void UpdateFinalTime(string formatedTime)
	{
		text.text = formatedTime;
	}

	public IEnumerator ToggleVisibility()
	{
		while (true) {
			text.text = "";
			yield return new WaitForSeconds (delay/10f);
			text.text = "Appuyez sur Espace pour recommencer";
			yield return new WaitForSeconds (delay/10f);
		}
	}
}
