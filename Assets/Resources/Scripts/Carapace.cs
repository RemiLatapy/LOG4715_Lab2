using UnityEngine;
using System.Collections;

public class Carapace : MonoBehaviour {

	[SerializeField]public float speed=10;
	[SerializeField]public float homingRotation=40f;
	[SerializeField]public int maxRebonds=3;


	private int rebonds;
	private Transform target;
	private Transform carSender;
	private Transform carTouched;

	private bool wallContact=false;
	private bool carContact=false;


	// Use this for initialization
	void Start () {
		//Find the shooter and remove form his parent
		carSender=gameObject.transform.parent;
		gameObject.transform.parent=null;

		rebonds=0;

		//Find Target
		if(tag=="CaraBleu")
			FindBlueTarget();
		if(tag=="CaraRouge")
			FindRedTarget();

	}

	/*Find the Next car and put it as target*/
	void FindRedTarget()
	{
		float distance= Mathf.Infinity;
		GameObject Cars = GameObject.Find("Cars");
		foreach(Transform car in Cars.transform)
		{
			if(car!=carSender)
			{
				float diff = (car.position - transform.position).sqrMagnitude;
				if(diff<distance)
				{
					distance=diff;
					target=car;
				}
			}
		}
		Debug.Log(target.name);
	}

	/*Find the first car and set it as target*/
	void FindBlueTarget(){
		GameObject Cars = GameObject.Find("Cars");
		int rank=8;
		foreach(Transform car in Cars.transform)
		{
			if(car!=carSender)
			{
				int newRank=car.GetComponent<CarController>().rank;
				if(newRank<rank)
				{
					target=car; 
					rank=newRank;
				}
			}
		}
	}

	
	// Update is called once per Fixtime
	void FixedUpdate () {
		//rigidbody.AddForce(transform.forward*speed,ForceMode.VelocityChange);

		switch(tag)
		{
			case "CaraRouge":
			RedHoming();
				break;

			case "CaraBleu":
			BlueHoming();
				break;

			case "CaraVerte":
			GreenHoming();
				break;
		}

	}

	//Called every frame
	void Update()
	{
		if(tag!="CaraBleu" && rebonds>=maxRebonds)
			Destroy(gameObject,0.2f);
	}

	//Called on collision with another
	void OnTriggerEnter(Collider other) {
		if(other.gameObject.CompareTag("WallCollider")||other.gameObject.CompareTag("Obstacle"))
		{
			wallContact=true;
		}
		else if(other.gameObject.CompareTag("CarCollider"))
		{
			carTouched=other.transform;
			carContact=true;
		}
	}

	//Gestion des carapaces vertes
	void GreenHoming()
	{
		//rigidbody.velocity=transform.forward*speed;
		rigidbody.AddForce(transform.forward*speed,ForceMode.VelocityChange);
		if(carContact)
			Destroy(gameObject,0.1f);
		else if(wallContact)
		{
			wallContact=false;
			rebonds++;
			if(rebonds>maxRebonds)
				Destroy(gameObject);
		}
	}
	
	//Gestion des carapces rouges
	void RedHoming()
	{
		rigidbody.velocity=transform.forward*speed;
		if(carContact||wallContact)
		{
			Destroy(gameObject,0.2f);
		}
		GoTowardsTarget(target);
	}

	//Gestion des carapaces bleues
	void BlueHoming()
	{
		rigidbody.velocity=transform.forward*speed;
		if(carContact && (carTouched.position-target.position).sqrMagnitude < 2)
		{
			Destroy (gameObject,0.2f);
		}
		Debug.Log("Distance : "+target.name+"  "+ (target.position-transform.position).sqrMagnitude);
		if((target.position-transform.position).sqrMagnitude < 1000f)
		{
			//Debug.Log ("Red Aiming");
			GoTowardsTarget(target);
		}
		else
		{
			GoTowardsTarget(this.GetComponent<WaypointProgressTracker>().target);
			//Debug.Log ("Blue Aiming");
		}
	}

	void GoTowardsTarget(Transform targetAim)
	{
		Quaternion targetRotation=Quaternion.LookRotation(targetAim.position-transform.position);
		transform.rigidbody.MoveRotation(Quaternion.RotateTowards(transform.rotation,targetRotation,homingRotation));
	}
}
