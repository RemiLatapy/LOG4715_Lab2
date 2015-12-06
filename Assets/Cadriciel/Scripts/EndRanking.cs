using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class EndRanking : MonoBehaviour {
	
	Text text;
	CheckpointManager checkpointManager;

	void Awake() {
		checkpointManager = GameObject.Find ("Game Manager").GetComponent<CheckpointManager>();
		text = GetComponent<Text> ();
		text.enabled = false;
	}

	public void UpdateFinalRanking()
	{
		text.text = checkpointManager.RankingFormated;
	}
}
