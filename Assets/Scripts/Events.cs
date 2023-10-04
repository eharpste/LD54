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

	public delegate void SelectLocationDelegate();
	public static event SelectLocationDelegate SelectLocation = delegate { };

	public static void SelectVehicleEvent()
	{
		if (SelectVehicle != null)
		{
			SelectVehicle();
		}
	}
	public static void UpdateVehicleEvent()
	{
		if (UpdateVehicle != null)
		{
			UpdateVehicle();
		}
	}

	public static void SelectLocationEvent()
	{
		if (SelectLocation != null)
		{
			SelectLocation();
			Debug.Log("location");
		}
	}
}
