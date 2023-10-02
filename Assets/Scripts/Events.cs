using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Events
{

	public delegate void SelectVehicleDelegate();
	public static event SelectVehicleDelegate SelectVehicle = delegate { };


	public static void SelectVehicleEvent()
	{
		if (SelectVehicle != null)
		{
			Debug.Log("starting select event");
			SelectVehicle();
		}
	}


}
