using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using System;

public class RaceManager : MonoBehaviour 
{
	[SerializeField]
	private GameObject _carContainer;

	[SerializeField]
	private GUIText _announcement;

	[SerializeField] [Range(1f, 3f)] private float adjustRubberbanding = 1.5f;			// Param to increase or decrease the rubberbanding effect

	[SerializeField]
	private int _timeToStart;

	[SerializeField]
	private int _endCountdown;

	public static int numberOfCars;

	float timer;
	bool raceIsRunning = true;

	// Use this for initialization
	void Awake () 
	{
		raceIsRunning = true;
		CarActivation(false);
		numberOfCars = _carContainer.transform.childCount;
	}

	void Update()
	{
		restartRace ();
	}
	
	void Start()
	{
		StartCoroutine(StartCountdown());
	}

	IEnumerator StartCountdown()
	{
		int count = _timeToStart;
		do 
		{
			_announcement.text = count.ToString();
			yield return new WaitForSeconds(1.0f);
			count--;
		}
		while (count > 0);
		_announcement.text = "Partez!";
		CarActivation(true);
		timer = Time.time;
		yield return new WaitForSeconds(1.0f);
		_announcement.text = "";
	}

	public void EndRace()
	{
		CarActivation(false);
		raceIsRunning = false;
		TimeSpan timeSpan = TimeSpan.FromSeconds(Time.time - timer);
		string timeText = string.Format("{0:D2}:{1:D2}", timeSpan.Minutes, timeSpan.Seconds);


		EndRanking endRanking = GameObject.Find ("Canvas End/Ranking").GetComponent<EndRanking>();
		BestTimeManager bestTimeManager = GameObject.Find ("Canvas End/BestTime").GetComponent<BestTimeManager>();
		PressSpaceManager pressSpaceManager = GameObject.Find ("Canvas End/PressSpace").GetComponent<PressSpaceManager>();

		endRanking.UpdateFinalRanking ();
		bestTimeManager.UpdateFinalTime (timeText);
		pressSpaceManager.StartCoroutine ("ToggleVisibility");
		
		SwitchCanvas ();
	}

	public void restartRace()
	{
		if(!raceIsRunning && CrossPlatformInput.GetButtonDown("Jump"))
			Application.LoadLevel("courseFlo2");
	}

	public void Announce(string announcement, float duration = 2.0f)
	{
		StartCoroutine(AnnounceImpl(announcement,duration));
	}

	IEnumerator AnnounceImpl(string announcement, float duration)
	{
		_announcement.text = announcement;
		yield return new WaitForSeconds(duration);
		_announcement.text = "";
	}

	public void CarActivation(bool activate)
	{
		foreach (CarAIControl car in _carContainer.GetComponentsInChildren<CarAIControl>(true))
		{
			car.enabled = activate;
		}
		
		foreach (CarUserControlMP car in _carContainer.GetComponentsInChildren<CarUserControlMP>(true))
		{
			car.enabled = activate;
		}

	}

	public float AdjustRubberbanding {
		get {
			return adjustRubberbanding;
		}
	}

	public IEnumerator DisplayText(string text, int timeMS)
	{
		_announcement.text = text;
		yield return new WaitForSeconds(timeMS/1000f);
		_announcement.text = "";
	}

	static void SwitchCanvas ()
	{
		GameObject canvas = GameObject.Find ("Canvas");
		canvas.SetActive (false);
		GameObject.Find ("Canvas End/Ranking").GetComponent<Text>().enabled = true;
		GameObject.Find ("Canvas End/BestTime").GetComponent<Text>().enabled = true;
		GameObject.Find ("Canvas End/PressSpace").GetComponent<Text>().enabled = true;
	}
}
