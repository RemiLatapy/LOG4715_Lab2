using UnityEngine;
using System.Collections;

public class Checkpoint : MonoBehaviour 
{
	[SerializeField]
	protected CheckpointManager _manager;

	[SerializeField]
	protected int _index;

	private int passageOrder = 999;

	void OnTriggerEnter(Collider other)
	{
		if (other as WheelCollider == null)
		{
			CarController car = other.transform.GetComponentInParent<CarController>();
			if (car)
			{
				_manager.CheckpointTriggered(car, _index, passageOrder);
				passageOrder--;
			}
		}
	}
}
