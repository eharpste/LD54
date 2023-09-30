using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{


    public static GameManager Instance { private set; get; }

    List<VehicleBehavior> Vehicles = new List<VehicleBehavior>();

    public int score = 0;

    [Header("Time Settings")]
    public int timeCounter = 0;
    public float secondsPerStep = 1f;

    [Header("Control Settings")]
    public bool isShipSelected = false;
    public VehicleBehavior selectedVehicle;
    public LayerMask vehicleMask;

    public List<Task> tasks = new List<Task>();
    public List<Task> upcomingTasks = new List<Task>();

    [Header("Task Settings")]
    public List<TaskSpec> taskSpecs = new List<TaskSpec>();

    [System.Serializable]
    public class TaskSpec {
        [SerializeField]
        public int appearanceTime;
        [SerializeField]
        public Task task;
    }
    

    public void AddVehicle(VehicleBehavior vehicle) {
        Vehicles.Add(vehicle);
    }

    public void RemoveVehicle(VehicleBehavior vehicle) {
        Vehicles.Remove(vehicle);
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

		foreach (VehicleBehavior vehicle in Vehicles) {
            vehicle.SimulateNextCommand(secondsPerStep);
        }
    }

    public void ResetGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
	}

    public void LandingScore(VehicleBehavior vehicle) {
        score += vehicle.cargoValue;
    }

    /// <summary>
    /// You get full score for getting a vehicle out the edge it wants, and half score for an adjacent edge. No points for sending it the opposite direction.
    /// </summary>
    /// <param name="vehicle"></param>
    /// <param name="departureEdge"></param>
    public void DepartingScore(VehicleBehavior vehicle, VehicleBehavior.Destination departureEdge) {
        if (vehicle.destination == departureEdge) {
            score += vehicle.cargoValue;
        }
        else {
            switch (vehicle.destination) {
                case VehicleBehavior.Destination.North:
                    if(departureEdge != VehicleBehavior.Destination.South) {
                        score += vehicle.cargoValue / 2;
                    }
                    break;
                case VehicleBehavior.Destination.East:
                    if (departureEdge != VehicleBehavior.Destination.West) {
                        score += vehicle.cargoValue / 2;
                    }
                    break;
                case VehicleBehavior.Destination.South:
                    if (departureEdge != VehicleBehavior.Destination.North) {
                        score += vehicle.cargoValue / 2;
                    }
                    break;
                case VehicleBehavior.Destination.West:
                    if (departureEdge != VehicleBehavior.Destination.East) {
                        score += vehicle.cargoValue / 2;
                    }
                    break;
            }
        }
    }
}
