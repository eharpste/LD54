using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { private set; get; }

    List<VehicleBehavior> Vehicle = new List<VehicleBehavior>();

    [Header("Time Settings")]
    public int timeCounter = 0;
    public float secondsPerStep = 1f;

    [Header("Control Settings")]
    public bool isShipSelected = false;
    public VehicleBehavior selectedVehicle;
    public LayerMask vehicleMask;

    public void AddVehicle(VehicleBehavior vehicle) {
        Vehicle.Add(vehicle);
    }

    private void Awake() {
        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {

	}

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space)) {
            SimulateStep();
        }

		if (Input.GetMouseButtonDown(0))
		{
			SelectVehicle();
		}
	}

    private void SelectVehicle()
    {
		Ray camRay = Camera.main.ScreenPointToRay(Input.mousePosition);
		Debug.DrawRay(camRay.origin, camRay.direction * 1000, Color.white, 2f);

		RaycastHit hitInfo;
		bool hit = Physics.Raycast(camRay, out hitInfo, 9999f, vehicleMask);
		if (hit == false) return;

		selectedVehicle = hitInfo.collider.gameObject.GetComponent<VehicleBehavior>();
	}

    public void SimulateStep() {
        timeCounter++;

		foreach (VehicleBehavior vehicle in Vehicle) {
            vehicle.SimulateNextCommand(secondsPerStep);
        }
    }

    public void ResetGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
	}
}
