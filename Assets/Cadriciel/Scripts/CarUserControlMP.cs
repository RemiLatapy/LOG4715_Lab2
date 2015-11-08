using UnityEngine;

[RequireComponent(typeof(CarController))]
public class CarUserControlMP : MonoBehaviour
{
	private CarController car;  // the car controller we want to use

	[SerializeField]
	private string vertical = "Vertical";

	[SerializeField]
	private string horizontal = "Horizontal";

	[SerializeField]
	private string verticalOrientation = "VerticalOrientation";

	private bool jump;
	private bool nitro;
	private bool useItem;
	
	void Awake ()
	{
		// get the car controller
		car = GetComponent<CarController>();
	}

	void Update () {
		#if CROSS_PLATFORM_INPUT
		if (CrossPlatformInput.GetButtonDown("Jump")) jump = true;
		if (CrossPlatformInput.GetButton("Nitro")) car.Nitro = true;
		if (CrossPlatformInput.GetButtonUp("Nitro")) car.Nitro = false;
		if (CrossPlatformInput.GetButtonDown("Fire1")) car.Item = true;
		#else
		if (Input.GetButtonDown("Jump")) jump = true;
		if (Input.GetButton("Nitro")) nitro = true;
		if (Input.GetButtonUp("Nitro")) nitro = false
		#endif
	}
	
	
	void FixedUpdate()
	{
		// pass the input to the car!
		#if CROSS_PLATFORM_INPUT
		float h = CrossPlatformInput.GetAxis(horizontal);
		float v = CrossPlatformInput.GetAxis(vertical);
		float v_o = CrossPlatformInput.GetAxis(verticalOrientation);
		#else
		float h = Input.GetAxis(horizontal);
		float v = Input.GetAxis(vertical);
		float v_o = Input.GetAxis(verticalOrientation);
		#endif
		if (jump) {
			car.Jump ();
			jump = false;
		}
		car.Move(h,v);
		car.Orient (h, v_o);
	}
}
