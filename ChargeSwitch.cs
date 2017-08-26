using UnityEngine;
using System.Collections;

public class ChargeSwitch : MonoBehaviour
{
	public GameObject target;
	public int targetCharge;

	void givePower()
	{
		if(targetCharge == 0) target.layer = LayerMask.NameToLayer("Neutral");
		else 				  target.layer = LayerMask.NameToLayer("Polar");

		Charge      charge = target.GetComponent<Charge>();
		if(!charge) charge = target.AddComponent<Charge>();

		charge.charge = targetCharge;
	}
}