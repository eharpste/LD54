using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{


    public static GameManager Instance { private set; get; }

    List<VehicleBehavior> Vehicles = new List<VehicleBehavior>();
    List<Landing> Landings = new List<Landing>();

    public int score = 0;

    [Header("Prefabs")]
    public GameObject planePrefab;
    public GameObject hoverLanderPrefab;
    public GameObject rocketPrefab;
    public GameObject warningSignPrefab;

    private List<GameObject> pendingArrivals = new List<GameObject>();

    [Header("Time Settings")]
    public int timeCounter = 0;
    public float secondsPerStep = 1f;

    [Header("Control Settings")]
    public bool isShipSelected = false;
    public VehicleBehavior selectedVehicle;
    public LayerMask vehicleMask;

    public List<Task> currentTasks = new List<Task>();
    //These are any arrivals that will be appearing in the next time step
    //public List<TaskSpec> pendingArrivals = new List<TaskSpec>();
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
        [SerializeField]
        public Vector3 entranceLocation;
        [SerializeField]
        public int entranceHeading;
    }


    public void AddVehicle(VehicleBehavior vehicle) {
        Vehicles.Add(vehicle);
        if(vehicle.currentTask != null) {
            currentTasks.Add(vehicle.currentTask);
        }
    }

    public void RemoveVehicle(VehicleBehavior vehicle) {
        Vehicles.Remove(vehicle);
    }

    public void AddLanding(Landing landing) {
        Landings.Add(landing);
    }

    public void RemoveLanding(Landing landing) {
        Landings.Remove(landing);
    }

    private void Awake() {
        Instance = this;
    }

    // Start is called before the first frame update
    void Start() {
        List<TaskSpec> toRemove = new List<TaskSpec>();
        List<TaskSpec> toCreate = new List<TaskSpec>();
        foreach(TaskSpec spec in taskSpecs) {
            if(spec.appearanceTime == 0) {
                toCreate.Add(spec);
                toRemove.Add(spec);
            }
            if(spec.appearanceTime == 1 && spec.task.taskType == Task.TaskType.Arrival || spec.task.taskType == Task.TaskType.Flyby) {
                pendingArrivals.Add(Instantiate(warningSignPrefab, spec.entranceLocation, Quaternion.identity));
            }
        }
        foreach(TaskSpec spec in toRemove) {
            taskSpecs.Remove(spec);
        }
        SpawnTasks(toCreate);
	}

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space)) {
            SimulateStep();
        }

		if (Input.GetMouseButtonDown(0))
		{
			if (!EventSystem.current.IsPointerOverGameObject()) {
				SelectVehicle();
			}
			
		}
	}

    private void SelectVehicle()
    {
		Ray camRay = Camera.main.ScreenPointToRay(Input.mousePosition);
		Debug.DrawRay(camRay.origin, camRay.direction * 1000, Color.white, 2f);

		RaycastHit hitInfo;
		bool hit = Physics.Raycast(camRay, out hitInfo, 9999f, vehicleMask);
        if (hit == false)
        {
            selectedVehicle = null;
			return;
        }

		selectedVehicle = hitInfo.collider.gameObject.GetComponent<VehicleBehavior>();
	}

    public void SimulateStep() {
        foreach(VehicleBehavior vehicleBehavior in Vehicles) {
            if (!vehicleBehavior.Ready) { return; }
        }
        foreach(Landing landing in Landings) {
            if (!landing.Ready) { return; }
        }


        timeCounter++;

		foreach (VehicleBehavior vehicle in Vehicles) {
            vehicle.SimulateNextCommand(secondsPerStep);
        }
        List<TaskSpec> toCreate = new List<TaskSpec>();
        List<TaskSpec> toRemove = new List<TaskSpec>();

        //whipe the current warning signs
        foreach(GameObject sign in pendingArrivals) {
            Destroy(sign);
        }
        pendingArrivals.Clear();

        foreach(TaskSpec spec in taskSpecs){ 
            if(spec.appearanceTime == timeCounter+1) {
                if (spec.task.taskType == Task.TaskType.Arrival || spec.task.taskType == Task.TaskType.Flyby) {
                    pendingArrivals.Add(Instantiate(warningSignPrefab, spec.entranceLocation, Quaternion.identity));
                }
            }
            else if(spec.appearanceTime == timeCounter) {               
                toRemove.Add(spec);
                toCreate.Add(spec);
            }
        }
        foreach(TaskSpec spec in toRemove) {
            taskSpecs.Remove(spec);
        }
        SpawnTasks(toCreate);
    }

    public void SpawnTasks(List<TaskSpec> specs) {
        foreach(TaskSpec spec in specs) {
            switch (spec.task.taskType) {
                case Task.TaskType.Arrival:
                case Task.TaskType.Flyby:
                    GameObject instantiatedPrefab =  spec.task.cargoType switch {
                        Task.CargoType.Passenger => Instantiate(planePrefab, spec.entranceLocation, Quaternion.Euler(0,spec.entranceHeading,0)),
                        Task.CargoType.Cargo => Instantiate(hoverLanderPrefab, spec.entranceLocation, Quaternion.Euler(0,spec.entranceHeading,0)),
                        Task.CargoType.Rocket => Instantiate(rocketPrefab, spec.entranceLocation, Quaternion.Euler(0, spec.entranceHeading, 0)),
                        _ => null,
                    };
                    if (instantiatedPrefab != null) {
                        VehicleBehavior newVehicle;
                        newVehicle = instantiatedPrefab.GetComponent<VehicleBehavior>();
                        newVehicle.currentFuel = spec.task.fuel;
                        newVehicle.currentTask = spec.task;
                        newVehicle.SetCommands(new List<VehicleBehavior.Command>() { VehicleBehavior.Command.Forward });
                    }
                    currentTasks.Add(spec.task);
                    break;
                case Task.TaskType.Departure:
                    currentTasks.Add(spec.task);
                    pendingDepartures.Add(spec.task);
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
        currentTasks.Remove(task);
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
