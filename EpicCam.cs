using UnityEngine;
using System.Collections;

public class EpicCam : MonoBehaviour
{
	Transform orbit;
	Transform pivot1;
	Transform pivot2;
	Transform trans;
	
	float pitch = 0f;
	float oldPitch = 0f;

	float lerpDist = 3f;

	int layer = (1 << 9) | (1 << 10);

	void Start ()
	{
		GameObject obj = GameObject.Find("/Player");
		orbit = obj.transform;

		obj = GameObject.Find("/CameraPivot");
		pivot1 = obj.transform;

		obj = GameObject.Find("/CameraPivot/ExtraPivot");
		pivot2 = obj.transform;

		obj = GameObject.Find("/CameraPivot/ExtraPivot/MainCamera");
		trans = obj.transform;
	}

	void LateUpdate ()
	{	
		pivot1.position = orbit.position; //sync up object positions every frame
		
		if(pivot1.rotation != orbit.rotation)
		{
			float angle = Quaternion.Angle(pivot1.rotation,orbit.rotation);

			//slowly track the "orientation" of the player with pivot1
			if(angle < 20f) pivot1.rotation = Quaternion.Slerp(pivot1.rotation,orbit.rotation,Time.deltaTime*120f/angle);
			else 			pivot1.rotation = Quaternion.Slerp(pivot1.rotation,orbit.rotation,Time.deltaTime*6f); //notice 120/20 = 6
		}

		pitch -= Input.GetAxis("Mouse Y")*50f*Time.deltaTime; //rotate camera up/down with mouse movement
		pitch = Mathf.Clamp(pitch,-60f,60f); //make sure the camera can't rotate more than 80 degrees up or down

		if(pitch != oldPitch)
		{
			//pivot2 tracks the head movements (up/down) of the player
			pivot2.localRotation = Quaternion.Euler(pitch,0f,0f); //remember everything is relative with local!
			oldPitch = pitch;
		}

		//trans places the camera 3 or less meters behind the final rotation
		//check for anything that would collide with the camera, place the camera in front of it if necessary
		RaycastHit hit;
		float distance = 3f;
		if(Physics.Raycast(orbit.position, trans.rotation*Vector3.back, out hit, 3f, layer)) distance = hit.distance - 0.1f;

		lerpDist = Mathf.Lerp (lerpDist,distance,5*Time.deltaTime);// move away slowly
		if(lerpDist > distance) lerpDist = distance; // move in instantaneously

		trans.localPosition = trans.localRotation*Vector3.back*lerpDist; //remember everything is relative with local
	}
}
