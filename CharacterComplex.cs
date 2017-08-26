using UnityEngine;
using System.Collections.Generic;

public class CharacterComplex : MonoBehaviour
{
	Rigidbody rigid;
	Transform trans;
	public AnimationCurve gravity; //gravity is 10m/s^2 up to a certain distance after which it fades to zero.
	
	EpicCam cam;
	
	int layer = (1 << 0); //1 shifted zero units left, aka 1, the default layer and no others
						  //there are 32 zeros/ones for the 32 layers, each one is a boolean indicating
						  //whether collisions are possible between the layers
	
	float speed = 10f;
	float xSensitivity = 100f;
	float ySensitivity = 100f;

	public float dist;
	public Vector3 lastNormal = Vector3.up;
	public Vector3 normal = Vector3.up;
	public Vector3 velocity = Vector3.zero;
	public bool gravitate = true;
	public bool grounded = false;
	
	// Use this for initialization
	void Start ()
	{
		GameObject obj = GameObject.Find("/MainCamera");
		cam = (EpicCam) obj.GetComponent("EpicCam");
		
		rigid = this.rigidbody;
		trans = this.gameObject.transform;
	}
	
	void Update()
	{
		if(grounded && Input.GetButtonDown("Jump"))
		{
			velocity += normal*10f;
			grounded = false;
			gravitate = true;
		}
	}
	
	void FixedUpdate ()
	{
		if(gravitate) velocity -= normal*gravity.Evaluate(dist)*Time.deltaTime; //apply gravity
		
		trans.Rotate(Vector3.up, Input.GetAxis("Mouse X")*xSensitivity*Time.deltaTime, Space.Self); //rotate camera

		if(grounded) velocity = MoveDir(); //allow movement if grounded
		
		RaycastHit hit;

		/* make sure velocity does not move us inside walls */
		if(velocity != Vector3.zero && Physics.SphereCast(trans.position,0.5f,velocity.normalized, out hit, velocity.magnitude*Time.deltaTime, layer))
		{
			Vector3 forward = PlanarMovement(normal, hit.normal, trans.forward); //forward changes when we switch surfaces

			if(forward != Vector3.zero)
			{
				Quaternion q = Quaternion.LookRotation(forward, hit.normal);
				trans.rotation = q; //rotate the player model
				cam.Rotate(q); //rotate the desired position for the camera (where the camera should move to)
			}

			normal = hit.normal; //redefine "up"

			//this might be broken//
			velocity = velocity.normalized*(hit.distance-0.005f); //only code that ensures we don't move inside walls
		}

		lastNormal = normal;

		trans.Translate(velocity*Time.deltaTime, Space.World); //move player
	}
	
	Vector3 MoveDir() //find movement vector along current plane
	{
		Vector3 input = new Vector3(Input.GetAxis("Horizontal"),0,Input.GetAxis("Vertical")).normalized;
		
		Vector3 dir = trans.rotation*input; //move respective to player rotation
		
		return dir*speed;
	}
	
	public Vector3 PlanarMovement (Vector3 normal, Vector3 next_normal, Vector3 vect) //find new forward based on old info 
	{
		Vector3 intersection = Vector3.Cross(normal, next_normal).normalized; //the component along the intersection will be the same.
		
		Vector3 oldBinormal  = Vector3.Cross(intersection,      normal).normalized; //the directions normal to the intersection and "up"
		Vector3 newBinormal  = Vector3.Cross(intersection, next_normal).normalized;
		
		vect = Vector3.Project(vect, intersection) /*the old component*/ + newBinormal*Vector3.Dot(vect, oldBinormal); /*the new component*/
		
		return vect;
	}
}