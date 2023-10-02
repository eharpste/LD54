using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Events;

public static class Events
{

	public delegate void SelectVehicleDelegate();
	public static event SelectVehicleDelegate SelectVehicle = delegate { };

	public delegate void UpdateVehicleDelegate();
	public static event UpdateVehicleDelegate UpdateVehicle = delegate { };

	public static void SelectVehicleEvent()
	{
		if (SelectVehicle != null)
		{
			Debug.Log("starting select event");
			SelectVehicle();
		}
	}
	public static void UpdateVehicleEvent()
	{
		if (UpdateVehicle != null)
		{
			Debug.Log("starting update event");
			UpdateVehicle();
		}
	}

}
