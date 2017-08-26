using UnityEngine;
using System.Collections.Generic;

public class CharacterComplex : MonoBehaviour
{
	Transform trans;
	public AnimationCurve gravity; //gravity is 10m/s^2 up to a certain distance after which it fades to zero.
	
	int layer = (1 << 9) | (1 << 10); //bitshifting layers

	public float speed = 10f;
	public float jumpSpeed = 5f;
	public float xSensitivity = 100f;
	public float ySensitivity = 100f;

	public float dist;
	public float playerCharge = -1f;
	public float groundCharge;
	public Vector3 lastNormal = Vector3.up;
	public Vector3 normal = Vector3.up;
	public Vector3 velocity = Vector3.zero;
	public bool gravitate = true;
	public bool grounded = false;
	
	// Use this for initialization
	void Start ()
	{
		trans = this.gameObject.transform;
	}
	
	void Update()
	{
		if(Input.GetButtonDown("Jump"))
		{
			playerCharge *= -1;
			if(grounded && Mathf.Sign(playerCharge*groundCharge) > 0f) //if grounded and likes, repel 
			{
				Jump();
			}
		}
	}
	
	void FixedUpdate ()
	{
		if(gravitate)
		{
			velocity += playerCharge*groundCharge*normal*gravity.Evaluate(dist)*Time.deltaTime; //apply regular gravity
			//float vSquared = 0f;
			//if(Vector3.Dot(Vector3.Project(velocity,lastNormal),normal) > 0.1f) vSquared = Vector3.Dot(velocity,velocity);
			//Mathf.Acos(Vector3.Dot(normal,lastNormal));
			//velocity -= normal*(vSquared/dist)*Time.deltaTime; //apply special gravity v^2/r

			//ideal methodology for special gravity:

			//find angle between normals if angle was not due to a collision then special gravity is applied
			//if it's due to a collision like hitting a wall it should not be applied
			//take the angle, find how far the object moved in a straight line and subtract it.
			//add on the position if the object instead swept out a circular path
		}
		
		trans.Rotate(Vector3.up, Input.GetAxis("Mouse X")*xSensitivity*Time.deltaTime, Space.Self); //rotate camera

		if(grounded) velocity = MoveDir(); //allow movement if grounded
		
		RaycastHit hit;

		/* make sure velocity does not move us inside walls */
		if(velocity != Vector3.zero && Physics.SphereCast(trans.position,0.5f,velocity.normalized, out hit, velocity.magnitude*Time.deltaTime, layer))
		{
			Vector3 forward = Vector3.zero;
			if(normal != hit.normal) forward = PlanarMovement(normal, hit.normal, trans.forward); //forward changes when we switch surfaces

			if(forward != Vector3.zero) trans.rotation = Quaternion.LookRotation(forward, hit.normal); //rotate the player model

			normal = hit.normal; //redefine "up"

			//this might be broken//
			velocity = velocity.normalized*(hit.distance-0.005f)/Time.deltaTime; //only code that ensures we don't move inside walls
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

	public void Jump()
	{
		velocity = MoveDir() + normal*jumpSpeed;
		grounded = false;
		gravitate = true;
	}
}