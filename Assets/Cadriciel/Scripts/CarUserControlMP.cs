using UnityEngine;

[RequireComponent(typeof(CarController))]
public class CarUserControlMP : MonoBehaviour
{
	private CarController car;  // the car controller we want to use

	[SerializeField]
	private string vertical = "Vertical";

	[SerializeField]
	private string horizontal = "Horizontal";

	private bool jump;

	void Awake ()
	{
		// get the car controller
		car = GetComponent<CarController>();
	}

	void Update () {
		#if CROSS_PLATFORM_INPUT
		if (CrossPlatformInput.GetButtonDown("Jump")) jump = true;
		#else
		if (Input.GetButtonDown("Jump")) jump = true;
		#endif
	}
	
	
	void FixedUpdate()
	{
		// pass the input to the car!
		#if CROSS_PLATFORM_INPUT
		float h = CrossPlatformInput.GetAxis(horizontal);
		float v = CrossPlatformInput.GetAxis(vertical);
		#else
		float h = Input.GetAxis(horizontal);
		float v = Input.GetAxis(vertical);
		#endif
		car.Move(h,v);
		if (jump) {
			car.Jump ();
			jump = false;
		}
	}
}
