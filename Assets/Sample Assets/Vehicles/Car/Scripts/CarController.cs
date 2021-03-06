﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CarController : MonoBehaviour
{
	// This car component is designed to be used on a gameobject which has wheels attached.
	// The wheels must be child objects, and each have a Wheel script attached, and a WheelCollider component.
	
	// Even though wheelcolliders have their own settings for grip loss, this car script (and its accompanying
	// wheel scripts) modify the settings on the wheelcolliders at runtime, to give a more exaggerated and fun
	// experience, allowing burnouts and drifting behavior in a way that is not readily achievable using
	// constant values on wheelcolliders alone.
	
	// The code priorities fun over realism, and although a gears system is included, it is not used to 
	// 'drive' the engine. Instead, the current revs and gear are calculated retrospectively based
	// on the car's current speed. These gear and rev values can then be read and used by a GUI or Sound component.
	
	
	[SerializeField] private float maxSteerAngle = 28;                              // The maximum angle the car can steer
	[SerializeField] private float steeringResponseSpeed = 200;                     // how fast the steering responds
	[SerializeField] [Range(0, 1)] private float maxSpeedSteerAngle = 0.23f;        // the reduction in steering angle at max speed
	[SerializeField] [Range(0, .5f)] private float maxSpeedSteerResponse = 0.5f;    // the reduction in steer response at max speed
	[SerializeField] private float maxSpeed = 60;                                   // the maximum speed (in meters per second!)
	[SerializeField] private float maxTorque = 35;                                  // the maximum torque of the engine
	[SerializeField] private float minTorque = 10;                                  // the minimum torque of the engine
	[SerializeField] private float brakePower = 40;                                 // how powerful the brakes are at stopping the car

	[SerializeField] private float adjustCentreOfMass = 0.25f;                      // vertical offset for the centre of mass

	[SerializeField] private AirControl airControl;                                 // container for the air control setting which will expose as a foldout in the inspector
	[System.Serializable]
	public class AirControl                                                         // the advanced settings for the car controller
	{
		public bool PreciseAirControl = true;										// Full control
		public bool preserveDirectionWhileInAir = false;							// flag for if the direction of travel to be preserved in the air (helps cars land in the right direction if doing huge jumps!)
		[Range(0, 10f)] public float adjustPitchControl = 3f;						// pitch control
		[Range(0, 10f)] public float adjustRollControl = 3f;						// roll control
	}

	[SerializeField] [Range(1, 10f)] private float jumpHigh = 3f;					// Jump high in meter (independant from mass)
	float minSkidToScore = 0.7f;													// Minimum avg skid to score

	[SerializeField] private Advanced advanced;                                     // container for the advanced setting which will expose as a foldout in the inspector
	
	[System.Serializable]
	public class Advanced                                                           // the advanced settings for the car controller
	{
		[Range(0, 1)] public float burnoutSlipEffect = 0.4f;                        // how much the car wheels will slide when burning out
		[Range(0, 1)] public float burnoutTendency = 0.2f;                          // how likely the car is to burnout 
		[Range(0, 1)] public float spinoutSlipEffect = 0.5f;                        // how easily the car spins out when turning
		[Range(0, 1)] public float sideSlideEffect = 0.5f;                          // how easily the car loses sideways grip 


		public float downForce = 30;                                                // the amount of downforce applied (speed is factored in)
		public int numGears = 5;                                                    // the number of gears
		[Range(0, 1)] public float gearDistributionBias = 0.2f;                     // Controls whether the gears are bunched together towards the lower or higher end of the car's range of speed.
		public float steeringCorrection = 2f;                                       // How fast the steering returns to centre with no steering input
		public float oppositeLockSteeringCorrection = 4f;                           // How fast the steering responds when steer input is in the opposite direction to the current wheel angle
		public float reversingSpeedFactor = 0.3f;                                   // The car's maximum reverse speed, as a proportion of its max forward speed.
		public float skidGearLockFactor = 0.1f;                                     // The car will not automatically change gear if the current skid factor is higher than this value.
		public float accelChangeSmoothing = 2f;                                     // Used to smooth out changes in acceleration input.
		public float gearFactorSmoothing = 5f;                                      // Controls the speed at which revs drop or raise to match new gear, after a gear change.
		[Range(0,1)]public float revRangeBoundary = 0.8f;                           // The amount of the full rev range used in each gear.
	}

	public class Items{
		public const int vide = 0;
		public const int carapaceVerte = 1;
		public const int carapaceRouge = 2;
		public const int carapaceBleue = 3;
		public const int nitro = 4;

	}
	
	private RaceManager raceManager;												// Reference to race manager
	
	private float[] gearDistribution;                                               // Stores the caluclated change point for each gear (0-1 as a normalised amount relative to car's max speed)
	private Wheel[] wheels;                                                         // Stores a reference to each wheel attached to this car.
	private float accelBrake;                                                       // The acceleration or braking input (1 to -1 range)
	private float smallSpeed;                                                       // A small proportion of max speed, used to decide when to start accelerating/braking when transitioning between fwd and reverse motion
	private float maxReversingSpeed;                                                // The maximum reversing speed
	private bool immobilized;                                                       // Whether the car is accepting inputs.


	//Prefabs
	private GameObject carapaceRouge, carapaceVerte, carapaceBleue;

	// publicly read-only props, useful for GUI, Sound effects, etc.
	public int GearNum { get; private set; }                                        // the current gear we're in.
	public float CurrentSpeed { get; private set; } 								// the current speed of the car
	[SerializeField] public float speedBooster = 5f;// the rate of the Booster
	public float CurrentSteerAngle{ get; private set; }			                   // The current steering angle for steerable wheels.
	public float AccelInput { get; private set; }                                   // the current acceleration input
	public float BrakeInput { get; private set; }                                   // the current brake input
	public float GearFactor  { get; private set; }                                  // value between 0-1 indicating where the current revs fall within the current range of revs for this gear
	public float AvgPowerWheelRpmFactor { get; private set; }                       // the average RPM of all wheels marked as 'powered'
	public float AvgSkid { get; private set; }                                      // the average skid factor from all wheels
	public float RevsFactor { get; private set; }                                   // value between 0-1 indicating where the current revs fall between 0 and max revs
	public float SpeedFactor { get;  private set; }                                 // value between 0-1 of the car's current speed relative to max speed
	public float SpeedCarapace{get; set;}
	public int rank{get; set;}														// rank of the car


	// Variables use for picked up objects	
	private int item = 0; // id of picked up item, 0 -> none, 1 -> green projectile, 2 -> red projectile, 3 -> blue projectile, 4 -> nitro
	public RawImage[] itemBox;
	public RawImage[] itemWon;


	// Variables for nitro
	public Slider nitroSlider;
	private bool nitroUsed =false;
	private bool boosterUsed=false;
	public bool Nitro{
		get{return nitroUsed;}
		set{nitroUsed=value;}
	}
	public bool Item{get;set;}
	private float nitroLevel = 0;
	private float nitroFactor = 1;
	// The nitro factor that will be applied to nitroFactor
	[SerializeField] 
	[Range(1, 3)] private float appliedNitroFactor = 2;
	[SerializeField] 
	[Range(0, 2)] private float reduceNitro = 0.7f;
	[SerializeField] 
	[Range(100, 200)] private float nitroSpeed = 160f;
	// Variables use for damages
	private float damagePoints = 0;
	[SerializeField] 
	[Range(0, 2)] private float reduceDamagePoints = 0.02f;
	// Factor for damage against wall
	[SerializeField] 
	[Range(0, 2)] private float wallDamageFactor = 0.7f;
	// Factor for damage against an obstacle
	[SerializeField] 
	[Range(0, 2)] private float obstacleDamageFactor = 0.5f;
	// Factor for damage against a car
	[SerializeField] 
	[Range(0, 2)] private float carDamageFactor = 0.3f;
	private float damageFactor = 1;
	// The damage factor that will be applied to damageFactor
	private float zoneFactor = 1;
	[SerializeField] 
	[Range(1, 3)] private float appliedDamageFactor = 2;

	
	// Variables for turn indicators
	public RawImage leftArrow;
	public RawImage rightArrow;

	public RawImage bam;

	// Variables for speedometer
	public RawImage speedOMeterDial;
	public RawImage speedOMeterPointer;
	private float rotationAngleStart;
	
	public int NumGears {					// the number of gears set up on the car
		get { return advanced.numGears; }
	}						
	
	
	// the following values are provided as read-only properties,
	// and are required by the Wheel script to compute grip, burnout, skidding, etc
	public float MaxSpeed
	{
		get { return maxSpeed; }
	}
	
	
	public float MaxTorque
	{
		get { return maxTorque; }
	}
	
	
	public float BurnoutSlipEffect
	{
		get { return advanced.burnoutSlipEffect; }
	}
	
	
	public float BurnoutTendency
	{
		get { return advanced.burnoutTendency; }
	}
	
	
	public float SpinoutSlipEffect
	{
		get { return advanced.spinoutSlipEffect; }
	}
	
	
	public float SideSlideEffect
	{
		get { return advanced.sideSlideEffect; }
	}
	
	
	public float MaxSteerAngle
	{
		get { return maxSteerAngle; }
	}

	public float AdjustPitch {
		get { return airControl.adjustPitchControl; }
	}

	public float AdjustRoll	{
		get { return airControl.adjustRollControl; }
	}

	public bool PreserveDirectionWhileInAir {
		get { return airControl.preserveDirectionWhileInAir; }
	}

	public bool PreciseAirControl {
		get { return airControl.PreciseAirControl; }
	}
	
	// variables added due to separating out things into functions!
	bool anyOnGround = true;
	float curvedSpeedFactor;
	bool reversing;
	float targetAccelInput; // target accel input is our desired acceleration input. We smooth towards it later
	private float rubberbandingFactor = 1;									// Factor apply to increase or decrease speed


	void Awake ()
	{
		// get a reference to all wheel attached to the car.
		wheels = GetComponentsInChildren<Wheel>();
		
		raceManager = GameObject.Find ("Game Manager").GetComponent<RaceManager>();
		
		SetUpGears();
		
		// deactivate and reactivate the gameobject - this is a workaround
		// to a bug where changes to wheelcolliders at runtime are not 'taken'
		// by the rigidbody unless this step is performed :(
		gameObject.SetActive(false);
		gameObject.SetActive(true);
		
		// a few useful speeds are calculated for use later:
		smallSpeed = maxSpeed*0.05f;
		maxReversingSpeed = maxSpeed * advanced.reversingSpeedFactor;

		carapaceBleue=Resources.Load("Prefabs/CarapaceBleu") as GameObject;
		carapaceRouge=Resources.Load("Prefabs/CarapaceRouge") as GameObject;
		carapaceVerte=Resources.Load("Prefabs/CarapaceVerte") as GameObject;
	}
	
	void Start()
	{
		this.transform.FindChild ("Fire").renderer.enabled = false;
		this.transform.FindChild ("Smoke").renderer.enabled = false;
		this.transform.FindChild ("NitroEffects1").renderer.enabled = false;
		this.transform.FindChild ("NitroEffects2").renderer.enabled = false;

		if(this.IsPlayer())
		{
			this.leftArrow.enabled = false;
			this.rightArrow.enabled = false;
			this.bam.enabled = false;
			nitroSlider.value = nitroLevel;
			HideItemBox();
			for (int i = 0; i < itemWon.Length; i++) {
				itemWon [i].enabled = false;
			}
		}
		if(IsPlayer())
			StartCoroutine(DoABarrelRoll());
	}
	
	void OnEnable()
	{
		// set adjusted centre of mass.
		rigidbody.centerOfMass = Vector3.up * adjustCentreOfMass;
	}

	void FixedUpdate(){
		if(IsPlayer())
			AddStylePoints();
		SpeedOMeter ();
		ManageDamagePoints ();
		if(Item) UseItem();

		if (nitroUsed) {
			NitroUse ();
		}
		else if(boosterUsed)
		{
			StartNitroUse();
			if(boosterUsed)
			{
				CurrentSpeed=nitroSpeed*10;
			}
		}
		else
		{
			StopNitroUse();
		}
	}

	public void ManageDamagePoints()
	{
		// Damage points are restored through the time
		if (damagePoints > 30) 
		{
			damagePoints -= reduceDamagePoints;
		}

		// No damageFactor by default
		damageFactor = 1;
		if(damagePoints < 50 && damagePoints >= 30)
		{
			Texture2D someTexture = Resources.Load("textures/skyCar_body_dff_damage1") as Texture2D;
			this.transform.Find("SkyCar/vehicle_skyCar_body_paintwork").renderer.materials[1].SetTexture("_MainTex", someTexture);
		}
		else if(damagePoints < 70 && damagePoints >= 50)
		{
			this.transform.FindChild ("Smoke").renderer.enabled = false;
			Texture2D someTexture = Resources.Load("textures/skyCar_body_dff_damage2") as Texture2D;
			this.transform.Find("SkyCar/vehicle_skyCar_body_paintwork").renderer.materials[1].SetTexture("_MainTex", someTexture);
		}
		else if(damagePoints < 100 && damagePoints >= 70)
		{
			Texture2D someTexture = Resources.Load("textures/skyCar_body_dff_damage2") as Texture2D;
			this.transform.Find("SkyCar/vehicle_skyCar_body_paintwork").renderer.materials[1].SetTexture("_MainTex", someTexture);
			this.transform.FindChild ("Smoke").renderer.enabled = true;
			this.transform.FindChild ("Fire").renderer.enabled = false;
		}
		else if(damagePoints >= 100)
		{
			damageFactor = appliedDamageFactor;
			Texture2D someTexture = Resources.Load("textures/skyCar_body_dff_damage2") as Texture2D;
			this.transform.Find("SkyCar/vehicle_skyCar_body_paintwork").renderer.materials[1].SetTexture("_MainTex", someTexture);
			this.transform.FindChild ("Fire").renderer.enabled = true;
			this.transform.FindChild ("Smoke").renderer.enabled = true;
		}
	}

	public void SpeedOMeter()
	{
		if (IsPlayer ()) {
			float speedFactor = CurrentSpeed / MaxSpeed;
			float rotationAngle;
			if (CurrentSpeed >= 0) {
				rotationAngle = Mathf.Lerp (0, 180, speedFactor);
			} else {
				rotationAngle = Mathf.Lerp (0, 180, -speedFactor);
			}
			speedOMeterPointer.transform.RotateAround (new Vector3 (speedOMeterPointer.transform.position.x, speedOMeterPointer.transform.position.y, speedOMeterPointer.transform.position.z), new Vector3 (0, 0, 1), rotationAngleStart-rotationAngle);
			rotationAngleStart = rotationAngle;
		}
	}
	
	
	public void Move (float steerInput, float accelBrakeInput)
	{
		// lose control of engine if immobilized
		if (immobilized) accelBrakeInput = 0;
		
		ConvertInputToAccelerationAndBraking (accelBrakeInput);
		CalculateSpeedValues ();
		HandleGearChanging ();
		CalculateGearFactor ();
		ProcessWheels (steerInput);
		ApplyDownforce ();
		CalculateRevs();
		PreserveDirectionInAir();
	}

	void UseItem(){
		Item=false;
		switch(item)
		{
			case Items.carapaceVerte:
				ThrowShell(Instantiate(carapaceVerte,transform.position, transform.rotation) as GameObject);
				break;

			case Items.carapaceRouge:
				ThrowShell(Instantiate (carapaceRouge, transform.position, transform.rotation) as GameObject);
				break;

			case Items.carapaceBleue:
				ThrowShell(Instantiate(carapaceBleue, transform.position, transform.rotation) as GameObject);
				break;

			default: break;
		}
		item=Items.vide;
		HideItemBox ();

	
	}
	void ThrowShell(GameObject carapace)
	{
		carapace.transform.parent=gameObject.transform;
		carapace.transform.position+=transform.forward.normalized*8;
		carapace.rigidbody.velocity=Vector3.Scale (transform.forward.normalized,transform.rigidbody.velocity)*2;
	}
	void ConvertInputToAccelerationAndBraking (float accelBrakeInput)
	{
		// move.Z is the user's fwd/back input. We need to convert it into acceleration and braking.
		// this differs based on if the car is currently moving forward or backward.
		// change is based slightly away from the zero value (by "smallspeed") so that for example when
		// the car transitions from reversing to moving forwards, the car does not need to come to a complete
		// rest before starting to accelerate.
		
		reversing = false;
		if (accelBrakeInput > 0) {
			if (CurrentSpeed > -smallSpeed) {
				
				CalculateRubberbandingFactor ();
				// pressing forward while moving forward : accelerate!
				targetAccelInput = accelBrakeInput * rubberbandingFactor * (nitroUsed?10:1);// * (boosterUsed?speedBooster:1);
				//targetAccelInput = accelBrakeInput * rubberbandingFactor;
				BrakeInput = 0;
			}
			else {
				// pressing forward while movnig backward : brake!
				BrakeInput = accelBrakeInput;
				targetAccelInput = 0;
			}
		}
		else {
			/*if(boosterUsed)
			{
				targetAccelInput = rubberbandingFactor *speedBooster;
			}
			else */if (CurrentSpeed > smallSpeed) {
				// pressing backward while moving forward : brake!
				BrakeInput = -accelBrakeInput;
				targetAccelInput = 0;
			}
			else {
				// pressing backward while moving backward : accelerate (in reverse direction)
				BrakeInput = 0;
				targetAccelInput = accelBrakeInput;
				reversing = true;
			}
		}
		// smoothly move the current accel towards the target accel value.
		AccelInput = Mathf.MoveTowards (AccelInput, targetAccelInput, Time.deltaTime * advanced.accelChangeSmoothing);
	}
	
	void CalculateRubberbandingFactor ()
	{
		// calcul rubberbanding factor with adjustment
		float adjustRubberbanding = raceManager.AdjustRubberbanding;
		rubberbandingFactor = Mathf.Lerp (1f/adjustRubberbanding, adjustRubberbanding, (rank-1f)/(RaceManager.numberOfCars-1f));
	}
	
	void CalculateSpeedValues ()
	{
		// current speed is measured in the forward direction of the car (sliding sideways doesn't count!)
		CurrentSpeed = transform.InverseTransformDirection (rigidbody.velocity).z;
		// speedfactor is a normalized representation of speed in relation to max speed:
		float speed = ((reversing ? maxReversingSpeed : maxSpeed) / (damageFactor*zoneFactor)) * nitroFactor;
		
		SpeedFactor = Mathf.InverseLerp (0, speed, Mathf.Abs (CurrentSpeed));
		curvedSpeedFactor = reversing ? 0 : CurveFactor (SpeedFactor);

	}
	
	void HandleGearChanging ()
	{
		// change gear, when appropriate (if speed has risen above or below the current gear's range, as stored in the gearDistribution array)
		if (!reversing) {
			if (SpeedFactor < gearDistribution [GearNum] && GearNum > 0)
				GearNum--;
			if (SpeedFactor > gearDistribution [GearNum + 1] && AvgSkid < advanced.skidGearLockFactor && GearNum < advanced.numGears - 1)
				GearNum++;
		}
	}
	
	void CalculateGearFactor ()
	{
		// gear factor is a normalised representation of the current speed within the current gear's range of speeds.
		// We smooth towards the 'target' gear factor, so that revs don't instantly snap up or down when changing gear.
		var targetGearFactor = Mathf.InverseLerp (gearDistribution [GearNum], gearDistribution [GearNum + 1], Mathf.Abs (AvgPowerWheelRpmFactor));
		GearFactor = Mathf.Lerp (GearFactor, targetGearFactor, Time.deltaTime * advanced.gearFactorSmoothing);
	}
	
	void ProcessWheels (float steerInput)
	{
		// Process each wheel:
		// we accumulate some averages of all wheels into these vars:
		AvgPowerWheelRpmFactor = 0;
		AvgSkid = 0;
		var numPowerWheels = 0;
		anyOnGround = false;
		foreach (var wheel in wheels) {
			var wheelCollider = wheel.wheelCollider;
			if (wheel.steerable) {
				// apply steering to this wheel. The actual steering change applied is based on the steering range, current speed, 
				// and whether the wheel is currently pointing in the direction that steering is being applied
				var currentSteerSpeed = Mathf.Lerp (steeringResponseSpeed, steeringResponseSpeed * maxSpeedSteerResponse, curvedSpeedFactor);
				var currentMaxAngle = Mathf.Lerp (maxSteerAngle, maxSteerAngle * maxSpeedSteerAngle, curvedSpeedFactor);
				// auto-correct steering to centre if no steering input:
				if (steerInput == 0) {
					currentSteerSpeed *= advanced.steeringCorrection;
				}
				// increase steering speed if steering input is in opposite direction to current wheel direction (for faster response)
				if (Mathf.Sign (steerInput) != Mathf.Sign (CurrentSteerAngle)) {
					currentSteerSpeed *= advanced.oppositeLockSteeringCorrection;
				}
				// modify the actual steer angle of the wheel by these calculated values:
				CurrentSteerAngle = Mathf.MoveTowards (CurrentSteerAngle, steerInput * currentMaxAngle, Time.deltaTime * currentSteerSpeed);
				wheelCollider.steerAngle = CurrentSteerAngle;
			}
			// acumulate skid amount from this wheel, for averaging later
			AvgSkid += wheel.SkidFactor;
			if (wheel.powered) {
				// apply power to wheels marked as powered:
				// available torque drops off as we approach max speed
				var currentMaxTorque = Mathf.Lerp (maxTorque, (SpeedFactor < 1) ? minTorque : 0, reversing ? SpeedFactor : curvedSpeedFactor);
				wheelCollider.motorTorque = AccelInput * currentMaxTorque;
				// accumulate RPM from this wheel, for averaging later
				AvgPowerWheelRpmFactor += wheel.Rpm / wheel.MaxRpm;
				numPowerWheels++;
			}
			// apply curent brake torque to wheel
			wheelCollider.brakeTorque = BrakeInput * brakePower;
			// if any wheel is on the ground, the car is considered grounded
			if (wheel.OnGround) {
				anyOnGround = true;
			}
		}
		// average the accumulated wheel values
		AvgPowerWheelRpmFactor /= numPowerWheels;
		AvgSkid /= wheels.Length;
	}
	
	void ApplyDownforce ()
	{
		// apply downforce
		if (anyOnGround) {
			rigidbody.AddForce (-transform.up * curvedSpeedFactor * advanced.downForce);
		}
	}
	
	void CalculateRevs ()
	{
		// calculate engine revs (for display / sound)
		// (this is done in retrospect - revs are not used in force/power calculations)
		var gearNumFactor = GearNum / (float)NumGears;
		var revsRangeMin = ULerp (0f, advanced.revRangeBoundary, CurveFactor (gearNumFactor));
		var revsRangeMax = ULerp (advanced.revRangeBoundary, 1f, gearNumFactor);
		RevsFactor = ULerp (revsRangeMin, revsRangeMax, GearFactor);
	}
	
	void PreserveDirectionInAir()
	{
		// special feature which allows cars to remain roughly pointing in the direction of travel
		if (!anyOnGround && PreserveDirectionWhileInAir && rigidbody.velocity.magnitude > smallSpeed) {
			rigidbody.MoveRotation (Quaternion.Slerp (rigidbody.rotation, Quaternion.LookRotation (rigidbody.velocity), Time.deltaTime));
			rigidbody.angularVelocity = Vector3.Lerp (rigidbody.angularVelocity, Vector3.zero, Time.deltaTime);
		}
	}

	void AddStylePoints ()
	{
		// Add style point when jump
		if (!anyOnGround && rigidbody.velocity.magnitude > 4f) {
			ScoreManager.score++;
		}

		// Add style point when skid
		if (AvgSkid > minSkidToScore) {
			ScoreManager.score++;
		}
	}

	public void modifyStyleScore (int points, string message)
	{
		if (IsPlayer ()) {
			ScoreManager.score += points;
			StartCoroutine (raceManager.DisplayText (message, 1500));
		}
	}
	
	IEnumerator DoABarrelRoll() {
		float barrelProgress = 0;
		while (true) {
			if(anyOnGround) {
				barrelProgress = 0;
			}
			while (!anyOnGround) {
				barrelProgress += Mathf.Rad2Deg * transform.InverseTransformVector(rigidbody.angularVelocity).z * Time.deltaTime;
				if (barrelProgress < -340 || barrelProgress > 340) {
					barrelProgress = 0;
					ScoreManager.score += 1000;
					StartCoroutine(raceManager.DisplayText("Barrel Roll ! +1000 !", 1000));
				}
				yield return null;
			}
			yield return null;
		}
	}

	public void AirOrientation (float pitch, float roll)
	{
		if (!anyOnGround) {
			if (PreciseAirControl) {
				Vector3 targetLocalAngularVelocity = new Vector3 (
				pitch * AdjustPitch, 
				transform.InverseTransformDirection (rigidbody.angularVelocity).y,
				-roll * AdjustRoll);
				rigidbody.angularVelocity = transform.TransformVector (targetLocalAngularVelocity);
			} else {
				rigidbody.AddRelativeTorque (pitch * AdjustPitch * 50, 0, -roll * AdjustRoll * 50);
			}
		}
	}
	
	public void Jump ()
	{
		// Calculate jump force to respect jump high.
		if (anyOnGround) {
			float jumpForce = Mathf.Sqrt (2f * jumpHigh * Physics.gravity.magnitude) * rigidbody.mass;
			rigidbody.AddForce (new Vector3 (0, jumpForce, 0), ForceMode.Impulse);
		}
	}	
	
	// simple function to add a curved bias towards 1 for a value in the 0-1 range
	float CurveFactor (float factor)
	{
		return 1 - (1 - factor)*(1 - factor);
	}
	
	// unclamped version of Lerp, to allow value to exceed the from-to range
	float ULerp (float from, float to, float value)
	{
		return (1.0f - value)*from + value*to;
	}
	
	
	void SetUpGears()
	{
		// the gear distribution is a range of normalized values marking out where the gear changes should occur
		// over the normalized range of speeds for the car.
		// eg, if the bias is centred, 5 gears would be evenly distributed as 0-0.2, 0.2-0.4, 0.4-0.6, 0.6-0.8, 0.8-1
		// with a low bias, the gears are clumped towards the lower end of the speed range, and vice-versa for high bias.
		
		gearDistribution = new float[advanced.numGears + 1];
		for (int g = 0; g <= advanced.numGears; ++g)
		{
			float gearPos = g / (float)advanced.numGears;
			
			float lowBias = gearPos*gearPos*gearPos;
			float highBias = 1 - (1 - gearPos) * (1 - gearPos) * (1 - gearPos);
			
			if (advanced.gearDistributionBias < 0.5f)
			{
				gearPos = Mathf.Lerp(gearPos, lowBias, 1 - (advanced.gearDistributionBias * 2));
			} else {
				gearPos = Mathf.Lerp(gearPos, highBias, (advanced.gearDistributionBias - 0.5f) * 2);
			}
			
			gearDistribution[g] = gearPos;
		}
	}
	
	
	void OnDrawGizmosSelected()
	{
		// visualise the adjusted centre of mass in the editor
		Gizmos.color = Color.cyan;
		Gizmos.DrawWireSphere(rigidbody.position + Vector3.up * adjustCentreOfMass, 0.2f);
	}
	
	// Immobilize can be called from other objects, if the car needs to be made uncontrollable
	// (eg, from asplosion!)
	public void Immobilize ()
	{
		immobilized = true;
	}
	
	// Reset is called via the ObjectResetter script, if present.
	public void Reset()
	{
		immobilized = false;
	}
	
	bool IsPlayer()
	{
		return this.GetComponent<CarUserControlMP>() != null;
	}
	
	void OnTriggerEnter(Collider other) 
	{
		// If we pick up an object and we don't already have one
		if (other.gameObject.CompareTag ("Pick Up") && (item == Items.vide || item == Items.nitro))
		{
			// Destroy the object
			Destroy (other.gameObject);
			
			randomizeItem();
			
			if(item == Items.nitro) {
				nitroLevel = 100;
			}
			// If the car is the player, display informations
			if(this.IsPlayer()){

				switch(item){
				case Items.carapaceVerte :
					// Display the won item for a certain amount of time
					StartCoroutine(ShowWonItem(3f, Items.carapaceVerte-1));
					ShowItemBox(Items.carapaceVerte);
					break;
				case Items.carapaceRouge :
					// Display the won item for a certain amount of time
					StartCoroutine(ShowWonItem(3f, Items.carapaceRouge-1));
					ShowItemBox(Items.carapaceRouge);
					break;
				case Items.carapaceBleue :
					// Display the won item for a certain amount of time
					StartCoroutine(ShowWonItem(3f, Items.carapaceBleue-1));
					ShowItemBox(Items.carapaceBleue);
					break;
				case Items.nitro : 
					StartCoroutine(ShowWonItem(3f, Items.nitro-1));
					nitroSlider.value = nitroLevel;
					break;
				}

			}
		}
		else if (IsPlayer() && (other.gameObject.CompareTag ("TurnIndicatorLeft")))
		{
			this.leftArrow.enabled = true;
			this.rightArrow.enabled = false;
		}
		else if (IsPlayer() && (other.gameObject.CompareTag ("TurnIndicatorRight")))
		{
			this.rightArrow.enabled = true;
			this.leftArrow.enabled = false;
		}
		else if (IsPlayer() && (other.gameObject.CompareTag ("StopIndicator")))
		{
			this.rightArrow.enabled = false;
			this.leftArrow.enabled = false;
		}
	}
	void OnTriggerStay(Collider other)
	{
		bool water=false;
		if (other.gameObject.CompareTag ("SpeedBoost"))
		{
			if(other.gameObject.name=="SpeedBoost 2")
			{
				Debug.Log (other.gameObject.name);
				this.rigidbody.AddForce(Quaternion.AngleAxis(0, other.transform.forward)  *Vector3.forward* speedBooster ,ForceMode.Acceleration);
				StartNitroUse(); 
				nitroUsed=false;
			}
			else
			{

				this.rigidbody.AddForce(Quaternion.AngleAxis(90, other.transform.forward)  *Vector3.forward* speedBooster ,ForceMode.Acceleration);
				StartNitroUse(); 
				nitroUsed=false;
			}
		}

		else if(other.gameObject.CompareTag("Water"))
		{
			zoneFactor=appliedDamageFactor;
			water=true;
		}
		else if(!water && other.gameObject.CompareTag("Road"))
        {
			zoneFactor=1;
		}
	}
	
	void OnTriggerExit(Collider other)
	{
		if (other.gameObject.CompareTag ("SpeedBoost"))
		{
			boosterUsed=false;
			StopNitroUse();
		}
		else if(other.gameObject.CompareTag("Road"))
		{
			zoneFactor=appliedDamageFactor;
		}
		else if(other.gameObject.CompareTag("Water"))
		{
			zoneFactor=1;
		}
	}

	private void HideItemBox(){
		for (int i = 0; i < itemBox.Length; i++) {
				itemBox [i].enabled = false;
		}
	}

	// Display the item box with the won item in it
	private void ShowItemBox(int item){
		if(item > 0){
			itemBox[0].enabled = true;
			for (int i = 1; i < itemBox.Length; i++) {
				if (i == item) {
					itemBox [i].enabled = true;
				} else
					itemBox [i].enabled = false;
			}
		}
	}

	// Display the won item for a certain amount of time
	IEnumerator ShowWonItem (float delay, int item) {
		for (int i = 0; i < itemWon.Length; i++) {
			if (i == item) {
				itemWon [i].enabled = true;
			} else
				itemWon [i].enabled = false;
		}
		yield return new WaitForSeconds(delay);
		for (int i = 0; i < itemWon.Length; i++) {
			itemWon [i].enabled = false;
		}

	}



	void OnCollisionEnter(Collision col)
	{
		//Transform parent = col.collider.transform.parent;
		Transform tranform = col.collider.transform.parent;
		if (transform != null) {
			switch (tranform.gameObject.tag) {
			case "WallCollider":
				if(IsPlayer ()) {
					StartCoroutine(ShowBAM(1f));
				}
				applyDamage (wallDamageFactor, CurrentSpeed);
				break;
			case "Obstacle":
				applyDamage (obstacleDamageFactor, CurrentSpeed);
				if(IsPlayer ()) {
					StartCoroutine(ShowBAM(1f));
				}
				break;
			case "Player":
				if(IsPlayer ()) {
					StartCoroutine(ShowBAM(1f));
				}
				applyDamage (carDamageFactor, CurrentSpeed);
				break;
			}
		}
	}
	
	void applyDamage(float damagePoints, float speed)
	{
		// Calculate the damage points in function of the speed of the impact
		this.damagePoints += Mathf.FloorToInt(damagePoints*Mathf.Abs(speed));
	}

	IEnumerator ShowBAM (float delay) {
		this.bam.enabled = true;
		yield return new WaitForSeconds(delay);
		this.bam.enabled = false;
		
	}
	
	void randomizeItem ()
	{
		// Divise in three categories for rubberbanding
		int rankThird = Mathf.FloorToInt (3.1f * rank / RaceManager.numberOfCars);
		switch (rankThird) {
		case 1:
			// green or red
			item = Mathf.RoundToInt(Random.Range (1F, 2F));
			break;
		case 2:
			// red or nitro
			item = Mathf.RoundToInt(Random.Range (1F, 3F));
			item = item == Items.carapaceBleue ? Items.nitro : Items.carapaceRouge;
			break;
		case 3:
			// red or blue or nitro => plus de chance d'avoir une bleu, c'est ben fun
			item = Mathf.RoundToInt(Random.Range (1.8F, 4.2F));
			break;
		}
		item=Items.carapaceVerte;
	}
	
	// When nitro is used, set maxSpeed and maxTorque to nitro values which are bigger
	// So the car can drive faster
	public void NitroUse() {
		if (nitroLevel > 0) {
			StartNitroUse();
			nitroLevel -= reduceNitro;
			if(IsPlayer()) nitroSlider.value = nitroLevel;
		}
		else {
			StopNitroUse();
		}
	}
	public void StartNitroUse(){
		nitroUsed=true;
		this.transform.FindChild ("NitroEffects1").renderer.enabled = true;
		this.transform.FindChild ("NitroEffects2").renderer.enabled = true;
		// If nitro is used multiply the velocity by 2
		nitroFactor = appliedNitroFactor;
	}
	
	public void StopNitroUse () {
		nitroUsed=false;
		this.transform.FindChild ("NitroEffects1").renderer.enabled = false;
		this.transform.FindChild ("NitroEffects2").renderer.enabled = false;
		// Restore normal speed
		nitroFactor = 1;
	}
}
