using UnityEngine;
using System.Collections;

public class Shortcut : Checkpoint 
{
	[SerializeField]
	private bool _entry;

	void OnTriggerEnter(Collider other)
	{
		if (other as WheelCollider == null)
		{
			CarController car = other.transform.GetComponentInParent<CarController>();
			if (car)
			{
				_manager.ShortcutTriggered(car, _index, _entry);
			}
		}
	}
}
