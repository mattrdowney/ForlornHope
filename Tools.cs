using UnityEngine;
using System.Collections;

public class Tools : MonoBehaviour {
    private float movementSpeed=20;
    public float swingMomentum;
    public bool isGrappled=false;
    public Vector3 currentPos, originPos;
    private RaycastHit gPoint, target;
    private Vector3 dist;
    private Quaternion origin,camera_position;
    public static int currentLevel=0;
    public bool onPlatform=false;
    public int playerCharge=0;
    public Transform killBox;
	void Start () 
    {
        originPos = transform.position; //Tracks the player's current position
        currentPos = transform.position; //Tracks the player's position while he is grappled
        origin=transform.rotation; //Tracks the player's original rotation
        camera_position = Camera.main.GetComponent<Camera>().transform.rotation; //Tracks the camera's original position
	}
	void Update () 
    {
        originPos = transform.position;
        Ray ray=Camera.main.ScreenPointToRay(Input.mousePosition);//casts a ray based on the cursor's location
        if(Input.GetMouseButton(1)&&Physics.Raycast(ray, out target, 10)&&!target.transform.gameObject.tag.Equals("Finish"))//Cutter Script
            Destroy(target.transform.gameObject, 3f);
        else if(Input.GetMouseButton(1)&&Physics.Raycast(ray, out target, 10)&&target.transform.gameObject.tag.Equals("Finish"))
            Application.LoadLevel(++currentLevel); //Loads the next level designated in the build settings
        /*
         the float is the time to wait before destroying the object, we can use this time to perform the 
         cutting animation.
         */
        if(!isGrappled && Input.GetMouseButtonDown(0) && Physics.Raycast(ray,out gPoint,100))//Grapple Script
        {
            isGrappled=true;
            dist=gPoint.point;
            //Camera.main.camera.transform.LookAt(dist);
            Debug.DrawRay(dist,-ray.direction*gPoint.distance,Color.green,10f);
            //swingMomentum=20*Time.deltaTime+movementSpeed;
            /*
             * Rotates depending on the player's choice of swing direction at a
             * constant speed of 20
             */
        }
        if(isGrappled && !Input.GetMouseButtonUp(0)) //Grapple Behavior Code
        {
			Vector3 x = dist - transform.position;

			Debug.DrawRay(dist, Vector3.up*10,Color.red);

            currentPos = originPos;
			if (Input.GetKey("right") && Vector3.Dot(x,x) < 10000) //limits the rope length to 100
            {
                transform.RotateAround(dist, Vector3.right, 20 * Time.deltaTime);
                currentPos = transform.position;
            }
            else transform.position=currentPos;

            //Limits the rope length to 100
            if (Input.GetKey("left") && Vector3.Dot(x,x) < 10000)
			{
                transform.RotateAround(dist, Vector3.left, 20 * Time.deltaTime);
                currentPos = transform.position;
            }
            else transform.position = currentPos;

			Vector3 temp = Vector3.zero;

			if (Input.GetKey("down") && Vector3.Dot(x,x) < 10000)
            {
				temp += -(dist  - transform.position).normalized*Time.deltaTime*40f;
            }
            else transform.position = currentPos; //Stops the player from moving through solid objects

			if (Input.GetKey("up") && Vector3.Dot(x,x) < 10000)
			{
				temp += (dist  - transform.position).normalized*Time.deltaTime*40f;//Vector3.MoveTowards(transform.position, dist, 2 * Time.deltaTime);
			}
			else transform.position = currentPos;

			transform.position += temp;

            currentPos = transform.position;
        }
        if(isGrappled && Input.GetMouseButtonUp(0))
        {
            //swingMomentum=0;
            //transform.rotation=Quaternion.Slerp(transform.rotation,origin,1f);//resets the player position once left mouse button is released
            //Camera.main.camera.transform.rotation = camera_position;
            isGrappled=false;
            currentPos = originPos;
        }
        if     (Input.GetKey("1")) playerCharge=0;
        else if(Input.GetKey("2")) playerCharge=1;
	}
    void FixedUpdate()
    {
//        PlatformScript p = new PlatformScript();
//        if(!onPlatform)
//            swingMomentum = movementSpeed;
//        else 
//            swingMomentum=p.platformSpeed+movementSpeed;
//        if(onPlatform && p.charge!=playerCharge)//Moves the player towards the killBox if the charges are different
//            transform.position=Vector3.MoveTowards(transform.position,killBox.position,5f);
    }
}
