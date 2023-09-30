using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{


    public static GameManager Instance { private set; get; }

    List<VehicleBehavior> Vehicles = new List<VehicleBehavior>();

    public int score = 0;

    [Header("Prefabs")]
    public GameObject planePrefab;
    public GameObject hoverLanderPrefab;
    public GameObject rocketPrefab;

    [Header("Time Settings")]
    public int timeCounter = 0;
    public float secondsPerStep = 1f;

    [Header("Control Settings")]
    public bool isShipSelected = false;
    public VehicleBehavior selectedVehicle;
    public LayerMask vehicleMask;

    public List<Task> tasks = new List<Task>();
    //These are any arrivals that will be appearing in the next time step
    public List<Task> pendingArrivals = new List<Task>();
    //These are the departurs that haven't been handed to a vehicle yet.
    public List<Task> pendingDepartures = new List<Task>();

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
        if(vehicle.currentTask != null) {
            tasks.Add(vehicle.currentTask);
        }
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

        tasks.AddRange(pendingArrivals);
        pendingArrivals.Clear();
        foreach(TaskSpec spec in taskSpecs) {
            if(spec.appearanceTime == timeCounter+1) {
                if (spec.task.taskType == Task.TaskType.Arrival || spec.task.taskType == Task.TaskType.Flyby) {
                    pendingArrivals.Add(spec.task);
                }
            }
        }
    }

    public void SpawnTasks(List<Task> tasks) {
        foreach(Task task in tasks) {
            switch (task.taskType) {
                case Task.TaskType.Arrival:
                case Task.TaskType.Flyby:
                    GameObject instantiatedPrefab =  task.cargoType switch {
                        Task.CargoType.Passenger => Instantiate(planePrefab, task.appearanceLocation, Quaternion.identity),
                        Task.CargoType.Cargo => Instantiate(hoverLanderPrefab, task.appearanceLocation, Quaternion.identity),
                        Task.CargoType.Rocket => Instantiate(rocketPrefab, task.appearanceLocation, Quaternion.identity),
                        _ => null,
                    };
                    if (instantiatedPrefab != null) {
                        VehicleBehavior newVehicle;
                        newVehicle = instantiatedPrefab.GetComponent<VehicleBehavior>();
                        newVehicle.currentFuel = task.fuel;
                        newVehicle.currentTask = task;
                    }
                    break;
                case Task.TaskType.Departure:
                    pendingDepartures.Add(task);
                    break;
            }
        }
    }

    public void ResetGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
	}


    public void ScoreTask(Task task, float modifier=1.0f) {
        score += (int)(task.value * modifier);
        tasks.Remove(task);
    }



    ///// <summary>
    ///// You get full score for getting a vehicle out the edge it wants, and half score for an adjacent edge. No points for sending it the opposite direction.
    ///// </summary>
    ///// <param name="vehicle"></param>
    ///// <param name="departureEdge"></param>
    //public void DepartingScore(VehicleBehavior vehicle, VehicleBehavior.Destination departureEdge) {
    //    if (vehicle.destination == departureEdge) {
    //        score += vehicle.currentTask.value;
    //    }
    //    else {
    //        switch (vehicle.destination) {
    //            case VehicleBehavior.Destination.North:
    //                if(departureEdge != VehicleBehavior.Destination.South) {
    //                    score += vehicle.currentTask.value / 2;
    //                }
    //                break;
    //            case VehicleBehavior.Destination.East:
    //                if (departureEdge != VehicleBehavior.Destination.West) {
    //                    score += vehicle.currentTask.value / 2;
    //                }
    //                break;
    //            case VehicleBehavior.Destination.South:
    //                if (departureEdge != VehicleBehavior.Destination.North) {
    //                    score += vehicle.currentTask.value / 2;
    //                }
    //                break;
    //            case VehicleBehavior.Destination.West:
    //                if (departureEdge != VehicleBehavior.Destination.East) {
    //                    score += vehicle.currentTask.value / 2;
    //                }
    //                break;
    //        }
    //    }
    //}
}
