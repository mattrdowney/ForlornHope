using UnityEngine;
using System.Collections;

class EMChecker : MonoBehaviour
{
	Transform trans;
	SphereCollider sphere;
	float maxRadius = 20.0f; //should be defined by gravity but Unity won't let me.
	float miss;
	float hit;
	
	int layer = (1 << 9);
	int precision = 14;//2^-4 for maxRadius and 2^-10 for good measure; //23 for mantissa and 4 for log(16) ~ log(maxRadius)
	
	public bool everyOtherFrame = true;
	
	CharacterComplex comp;
	
	void Start ()
	{
		/*for(int i = 0; i < comp.gravity.keys.Length; i++)
		{
			float temp = comp.gravity.keys[i].time;
			if(maxRadius < temp) maxRadius = temp;
		}*/
		GameObject obj = GameObject.Find("/Player");
		trans = obj.GetComponent<Transform>();
		comp  = obj.GetComponent<CharacterComplex>();
		
		sphere = this.gameObject.GetComponent<SphereCollider>();
	}
	
	void FixedUpdate ()
	{	
		if(everyOtherFrame)
		{
			sphere.enabled = true;

			miss = 0f;
			hit = maxRadius;

			//use lower precision then a raycast!!!!!!!!!!!!!
			//also, you don't have to start from scratch in a static environment
			//you can use old collision distance info and the direction/distance moved last frame
			//to calc new approx for collision estimations

			Vector3 pos = trans.position;
			if(Physics.CheckSphere(pos,hit, layer)) //bisection method with maxRadius / (2^(precision+1)) accuracy
			{
				for(int i = 0; i < precision; ++i)
				{
					float check = (miss + hit) / 2;
					
					if(Physics.CheckSphere(pos, check, layer)) hit = check;
					else 									   miss = check;
				}
			}
			else
			{
				comp.grounded = false;
				comp.gravitate = false;
			}
			
			comp.dist = hit;

			sphere.radius = hit + 0.01f;
			sphere.center = trans.position;
		}
		else
		{
			sphere.enabled = false;
		}
		everyOtherFrame = !everyOtherFrame;
	}
	
	void OnCollisionEnter(Collision c)
	{	
		int minIndex = 0;
		float minDist = Vector3.Distance(trans.position, c.contacts[0].point);
		
		for(int i = 1; i < c.contacts.Length; i++)
		{
			float dist = Vector3.Distance(c.contacts[i].point, trans.position);
			
			if(dist < minDist)
			{
				minDist = dist;
				minIndex = i;
			}
		}
		
		if(comp.dist < maxRadius)
		{
			Vector3 normal = (trans.position - c.contacts[minIndex].point).normalized;
			Vector3 forward = Vector3.zero;
			if(normal != comp.normal) forward = comp.PlanarMovement(comp.normal, normal, trans.forward);
			
			if(forward != Vector3.zero) trans.rotation = Quaternion.LookRotation(forward, normal); //rotate the player base

			comp.groundCharge = c.contacts[minIndex].otherCollider.gameObject.GetComponent<Charge>().charge;
			comp.normal = normal;
		}

		float dot = Vector3.Dot(comp.velocity,comp.normal);

		if(comp.dist < 0.6f && dot < 9f)
		{
			if(Mathf.Sign(comp.playerCharge*comp.groundCharge) < 0f) comp.grounded = true; //opposites attract
			else
			{
				comp.Jump();
				//likes repel
			}
		}
		else 							 comp.grounded = false;

		if(comp.dist < 0.9f && -0.1f < dot && dot < 9f) trans.position -= comp.normal*(comp.dist - 0.51f);
		
		if(comp.dist < 0.55f) comp.gravitate = false;
		else 				  comp.gravitate = true;
	}
}