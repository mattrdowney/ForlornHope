using UnityEngine;
using System.Collections;

public class PlatformScript : MonoBehaviour
{
    public Transform origin, dest, rotationPoint;
    public float platformSpeed;
    private bool pivot=false;
    public int charge;
    private Quaternion rotationOrigin;
    void Start()
    {
        rotationOrigin=transform.rotation; //sets the platform's original rotation
    }
	void FixedUpdate() 
    {
        if(rotationPoint!=null) //If a rotation point exists have the platform rotate around that point
        {
            transform.RotateAround(rotationPoint.position, Vector3.right, platformSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotationOrigin, 1f);//ensures the platform always is flat
        }
        else if(origin!=null&&dest!=null)
        {
            if(transform.position==dest.position)
                pivot = true;
            if(transform.position==origin.position)
                pivot = false;
            if(pivot)
                transform.position = Vector3.MoveTowards(transform.position, origin.position, platformSpeed); //Go the other way when you reach your origin
            else
                transform.position = Vector3.MoveTowards(transform.position, dest.position, platformSpeed); //Go the other way when you reach your dest
        }
        else
            Debug.Log("Stationary Platform");
	}
}
