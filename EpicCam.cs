using UnityEngine;
using System.Collections;

public class EpicCam : MonoBehaviour
{
	Transform trans;
	Transform target;
	Transform orbit;
	Vector3 oldOrbit;
	Vector3 v3 = new Vector3(0f,0.258819f,-0.965925f);

	int layer = (1 << 0);

	float critical_angle = 30f;

	bool b = true;

	void Start ()
	{
		GameObject obj = this.gameObject;
		trans = obj.transform;
		
		obj = GameObject.Find("/Player");
		orbit = obj.transform;
		
		obj = GameObject.Find("/Player/CameraTarget");
		target = obj.transform;
		
		oldOrbit = orbit.position;
	}

	void LateUpdate ()
	{
		Vector3 delta = (orbit.position - oldOrbit);


		if(delta != Vector3.zero)
		{
			trans.position += delta;
			b = true;
		}
		
		if(trans.rotation != target.rotation || b)
		{
			float angle = Quaternion.Angle(trans.rotation,target.rotation);

			if(angle < 20f)
			{
				trans.rotation = Quaternion.Slerp(trans.rotation,target.rotation,Time.deltaTime*120f/angle);
			}
			else
			{
				trans.rotation = Quaternion.Slerp(trans.rotation,target.rotation,Time.deltaTime*6f);
			}

			Vector3 direction = trans.rotation*v3; 

			float distance = 3f;

			RaycastHit hit;

			if(Physics.Raycast(orbit.position, direction, out hit, 3f, layer)) distance = hit.distance - 0.1f;
			else                                                               distance = 3f;
			
			trans.position = orbit.position + direction*distance;
		}
		
		oldOrbit = orbit.position;
		b = false;
	}
	
	public void Rotate(Quaternion q)
	{
		target.rotation = q*Quaternion.Euler(30f,0f,0f);
		target.position = orbit.position + trans.rotation*v3*5f;
	}
}
