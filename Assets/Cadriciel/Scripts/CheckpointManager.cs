using UnityEngine;
using System.Collections.Generic;
using System;

public class CheckpointManager : MonoBehaviour 
{

	[SerializeField]
	private GameObject _carContainer;

	[SerializeField]
	private int _checkPointCount;
	[SerializeField]
	private int _totalLaps;

	private bool _finished = false;
	
	private Dictionary<CarController,PositionData> _carPositions = new Dictionary<CarController, PositionData>();

	private List<PositionData> rank;

	private class PositionData: IComparable<PositionData>
	{
		public int lap;
		public int checkPoint;
		public int passageOrder;
		public int position;
		public int activeShortcut = -1;
		public bool shortcutPassed = false;
		public CarController car;

		public int Score {
			get {return lap*100000+checkPoint*1000+passageOrder;}
		}


		// Default comparer for Part type.
		public int CompareTo(PositionData comparePart)
		{
			// A null value means that this object is greater.
			if (comparePart == null)
				return 1;
			else
				return comparePart.Score.CompareTo(this.Score);
		}
	}

	void Update() {
		rank.Sort();
		int i = 1;
		foreach (PositionData car in rank) {
			if(car.car.name == "Joueur 1")
				RankManager.rank = i;
			car.car.rank=i;
			i++;
		}
	}

	// Use this for initialization
	void Awake () 
	{
		rank = new List<PositionData> ();
		int i = 0;
		foreach (CarController car in _carContainer.GetComponentsInChildren<CarController>(true))
		{
			_carPositions[car] = new PositionData();
			_carPositions[car].car = car;
			_carPositions[car].passageOrder = i++;
			rank.Add(_carPositions[car]);
		}
	}
	
	public void CheckpointTriggered(CarController car, int checkPointIndex, int passageOrder)
	{

		PositionData carData = _carPositions[car];

		if (!_finished)
		{
			if (checkPointIndex == 0) // First checkpoint
			{
				if (carData.checkPoint == _checkPointCount-1) // Last checkpoint
				{
					carData.checkPoint = checkPointIndex;
					carData.passageOrder = passageOrder;
					carData.lap += 1;
					Debug.Log(car.name + " lap " + carData.lap);
					if (IsPlayer(car))
					{
						GetComponent<RaceManager>().Announce("Tour " + (carData.lap+1).ToString());
					}

					if (carData.lap >= _totalLaps)
					{
						_finished = true;
						GetComponent<RaceManager>().EndRace(car.name.ToLower());
					}
				}
			}
			else if (carData.checkPoint == checkPointIndex-1 || carData.shortcutPassed) //Checkpoints must be hit in order (any checkpoint)
			{
				carData.checkPoint = checkPointIndex;
				carData.passageOrder = passageOrder;
				carData.shortcutPassed = false;
				carData.activeShortcut = -1;
			}
		}


	}

	public void ShortcutTriggered(CarController car, int shortcutIndex, bool isEntry)
	{
		PositionData carData = _carPositions[car];

		if (isEntry) {
			carData.activeShortcut = shortcutIndex;
			carData.shortcutPassed = false;
		} else {
			if (carData.activeShortcut == shortcutIndex) {
				carData.shortcutPassed = true;
				carData.activeShortcut = -1;
			}
		}
	}

	bool IsPlayer(CarController car)
	{
		return car.GetComponent<CarUserControlMP>() != null;
	}

	public Checkpoint GetLastCheckpoint(CarController car) {
		int indexCheckpoint =  _carPositions [car].checkPoint;
		Checkpoint[] checkpoints = GameObject.Find ("Checkpoints").GetComponentsInChildren<Checkpoint> ();
		return checkpoints[indexCheckpoint];
	}
}
