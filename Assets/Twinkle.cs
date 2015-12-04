using UnityEngine;
using System.Collections;

public class Twinkle : MonoBehaviour {

	public int delay = 10;

	void Awake()
	{
		StartCoroutine ("ToggleVisibility");
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public IEnumerator ToggleVisibility()
	{
		while (true) {
			transform.Translate(new Vector3(-2f,0,0));
			yield return new WaitForSeconds (delay/10f);
			transform.Translate(new Vector3(2f,0,0));
			yield return new WaitForSeconds (delay/10f);
		}
	}
}
