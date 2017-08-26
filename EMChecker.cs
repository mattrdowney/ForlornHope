using UnityEngine;
using System.Collections;

class EMChecker : MonoBehaviour
{
	Transform trans;
	SphereCollider sphere;
	float maxRadius = 20.0f; //should be defined by gravity but Unity won't let me.
	float miss;
	float hit;

	int layer = (1 << 0);
	int precision = 12;

	bool everyOtherFrame = true;

	CharacterComplex comp;
	EpicCam cam;

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

		obj = GameObject.Find ("/MainCamera");
		cam = obj.GetComponent<EpicCam>();

		obj = this.gameObject;
		sphere = obj.GetComponent<SphereCollider>();
	}

	void FixedUpdate ()
	{
		everyOtherFrame = !everyOtherFrame;

		if(everyOtherFrame)
		{
			sphere.enabled = true;

			miss = 0f;
			hit = maxRadius;

			if(Physics.CheckSphere(trans.position,hit, layer)) //bisection method hitDist / (2^(precision+1)) accuracy
			{
				for(int i = 0; i < precision; ++i)
				{
					float check = (miss + hit) / 2;

					if(Physics.CheckSphere(trans.position, check, layer)) hit = check;
					else 												  miss = check;
				}
			}
			else
			{
				comp.grounded = false;
				comp.gravitate = false;
			}
			sphere.center = trans.position;
			sphere.radius = hit + 0.001f;

			comp.dist = hit;
		}
		else sphere.enabled = false;
	}

	void OnCollisionEnter(Collision c)
	{
		if(everyOtherFrame)
		{
			int minIndex = 0;
			float minDist = Vector3.Distance(trans.position, c.contacts[0].point);

			for(int i = 1; i < c.contacts.Length; i++)
			{
				float dist = Vector3.Distance(c.contacts[i].point,sphere.center);
				
				if(dist < minDist)
				{
					minDist = dist;
					minIndex = i;
				}
			}

			if(comp.dist < maxRadius)
			{
				Vector3 normal = (trans.position - c.contacts[minIndex].point).normalized;
				Vector3 forward = comp.PlanarMovement(comp.normal, normal, trans.forward);
				
				if(forward != Vector3.zero)
				{
					trans.rotation = Quaternion.LookRotation(forward, normal); //rotate the player base
					cam.Rotate(Quaternion.LookRotation(forward, normal)); //rotate the target position for the camera
				}
				comp.normal = normal;
			}
			
			if(comp.dist < 0.7f && Vector3.Dot(comp.velocity,comp.normal) < 9f) comp.grounded = true;
			else 																comp.grounded = false;
			
			if(comp.grounded) trans.position -= comp.normal*(sphere.radius - 0.51f);
			
			if(comp.dist < 0.55f) comp.gravitate = false;
			else 				  comp.gravitate = true;
		}
	}
}
