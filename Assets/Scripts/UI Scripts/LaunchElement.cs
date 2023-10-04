using System.Collections;
using UnityEngine;
using UnityEngine.UI;


[RequireComponent(typeof(CanvasGroup))]
public class LaunchElement : MonoBehaviour
{
	public Text shipName;


	// Use this for initialization
	void Start()
	{

	}

	// Update is called once per frame
	void Update()
	{

	}

	public void LaunchShip()
	{
		GameManager.Instance.selectedLocation.LaunchNextAvailableVehicle();
	}

}