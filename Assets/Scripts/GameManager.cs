using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{

    public static GameManager Instance { private set; get; }

    List<VehicleBehavior> Vehicles = new List<VehicleBehavior>();
    List<Landing> Landings = new List<Landing>();
    List<Runway> Runways = new List<Runway>();
    List<HoverPad> HoverPads = new List<HoverPad>();
    List<RocketLauncher> RocketLaunchers = new List<RocketLauncher>();

    public int score = 0;

    [Header("Prefabs")]
    public GameObject planePrefab;
    public GameObject hoverLanderPrefab;
    public GameObject rocketPrefab;
    public GameObject largeHaulerPrefab;
    public GameObject warningSignPrefab;

    private List<GameObject> warningMarkers = new List<GameObject>();

    [Header("Time Settings")]
    public int CurrentTime = 0;
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
        public bool randomizeEntrance;
        [SerializeField]
        public Vector3 entranceLocation;
    }

    [ExecuteInEditMode]
    public void RandomizeTaskLocations() {
        Transform[] cardinals = new Transform[] {
            GameObject.Find("Set/Terrain/ground/North").transform,
            GameObject.Find("Set/Terrain/ground/East").transform,
            GameObject.Find("Set/Terrain/ground/South").transform,
            GameObject.Find("Set/Terrain/ground/West").transform
        };
    
        foreach(TaskSpec spec in taskSpecs) {
            switch (spec.task.taskType) {
                case Task.TaskType.Departure:
                    spec.entranceLocation = new Vector3(0, 0, 0);
                    if (spec.task.cargoType == Task.CargoType.Rocket) {
                        spec.entranceLocation = new Vector3(-180, -180, -180);
                    }
                    break;
                case Task.TaskType.Arrival:
                case Task.TaskType.Flyby:
                    if (spec.randomizeEntrance) {
                        int randSide = Random.Range(0, 4);
                        switch (randSide) {
                            case 0: //North
                                spec.entranceLocation = new Vector3(9, Random.Range(3,7), Random.Range(1,8));
                                break;
                            case 1: // East
                                spec.entranceLocation = new Vector3(Random.Range(1, 8), Random.Range(3, 7), 9);
                                break;
                            case 2: // South
                                spec.entranceLocation = new Vector3(0, Random.Range(3, 7), Random.Range(1, 8));
                                break;
                            case 3: // West
                                spec.entranceLocation = new Vector3(Random.Range(1, 8), Random.Range(3, 7), 0);
                                break;
                        }
                    }
                    break;
            }
        }
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
        if(landing.GetType() == typeof(Runway)) {
            Runways.Add((Runway)landing);
        }else if (landing.GetType() == typeof(HoverPad)) {
            HoverPads.Add((HoverPad)landing);
        }
        else if (landing.GetType() == typeof(RocketLauncher)) {
            RocketLaunchers.Add((RocketLauncher)landing);
        }
    }

    public void RemoveLanding(Landing landing) {
        Landings.Remove(landing);
        if (landing.GetType() == typeof(Runway)) {
            Runways.Remove((Runway)landing);
        }
        else if (landing.GetType() == typeof(HoverPad)) {
            HoverPads.Remove((HoverPad)landing);
        }
        else if (landing.GetType() == typeof(RocketLauncher)) {
            RocketLaunchers.Remove((RocketLauncher)landing);
        }
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
                warningMarkers.Add(Instantiate(warningSignPrefab, spec.entranceLocation, Quaternion.identity));
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
        if(Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return)) {
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

        Events.SelectVehicleEvent();
	}

    public void CreateWarning(Vector3 placement) {
        warningMarkers.Add(Instantiate(warningSignPrefab, placement, Quaternion.identity));
    }

    public void SimulateStep() {
        foreach(VehicleBehavior vehicleBehavior in Vehicles) {
            if (!vehicleBehavior.Ready) {
                Debug.LogWarningFormat("{0} is not ready", vehicleBehavior.name);
                return; 
            }
        }
        foreach(Landing landing in Landings) {
            if (!landing.Ready) {
                Debug.LogWarningFormat("{0} is not ready", landing.name);
                return; 
            }
        }


        CurrentTime++;

		foreach (VehicleBehavior vehicle in Vehicles) {
            vehicle.SimulateNextCommand(secondsPerStep);
        }

        foreach(Landing landing in Landings) {
            landing.SimulateStep(secondsPerStep);
        }


        List<TaskSpec> toCreate = new List<TaskSpec>();
        List<TaskSpec> toRemove = new List<TaskSpec>();

        //whipe the current warning signs
        foreach(GameObject sign in warningMarkers) {
            Destroy(sign);
        }
        warningMarkers.Clear();

        foreach(TaskSpec spec in taskSpecs){ 
            if(spec.appearanceTime == CurrentTime+1) {
                if (spec.task.taskType == Task.TaskType.Arrival || spec.task.taskType == Task.TaskType.Flyby) {
                    CreateWarning(spec.entranceLocation);
                }
            }
            else if(spec.appearanceTime == CurrentTime) {               
                toRemove.Add(spec);
                toCreate.Add(spec);
            }
        }
        foreach(TaskSpec spec in toRemove) {
            taskSpecs.Remove(spec);
        }
        SpawnTasks(toCreate);

        //check for expired tasks

        //check for automatic tasks
        List<Task> dispatched = new List<Task>();
        foreach(Task task in pendingDepartures) {
            if(task.cargoType == Task.CargoType.Rocket) {
                foreach(RocketLauncher launcher in RocketLaunchers) {
                    if(launcher.Ready) {
                        GameObject newRocket = Instantiate(rocketPrefab, launcher.transform.position + Vector3.down * 100, Quaternion.identity);
                        CreateWarning(launcher.transform.position + Vector3.up);
                        VehicleBehavior vehicleBehavior = newRocket.GetComponent<VehicleBehavior>();
                        launcher.LaunchVehicle(vehicleBehavior);
                        dispatched.Add(task);
                        break;
                    }
                }
            }
        }

        foreach(Task task in dispatched) {
            pendingDepartures.Remove(task);
        }

        Events.UpdateVehicleEvent();
    }

    public void SpawnTasks(List<TaskSpec> specs) {
        foreach(TaskSpec spec in specs) {
            switch (spec.task.taskType) {
                case Task.TaskType.Arrival:
                case Task.TaskType.Flyby:
                    int heading = 0;
                    /// <summary>
                    /// North = Positive Z, heading -90, x=9
                    /// West = Positive X, heading 0, z=9
                    /// East = Negative X, heading 180, z=0
                    /// South = Negative Z, heading 90, x=0
                    /// </summary>
                    if (spec.entranceLocation.x == 9) { heading = -90; }
                    else if(spec.entranceLocation.x == 0) { heading = 90; }
                    else if(spec.entranceLocation.z == 0) { heading = 180; }
                    else if(spec.entranceLocation.z == 9) { heading = 0; }
                    else { Debug.LogWarningFormat("Don't know how to align EntranceLocation {0} setting identity.", spec.entranceLocation); }


                    GameObject instantiatedPrefab =  spec.task.cargoType switch {
                        Task.CargoType.Passenger => Instantiate(planePrefab, spec.entranceLocation, Quaternion.Euler(0, heading, 0)),
                        Task.CargoType.Cargo => Instantiate(hoverLanderPrefab, spec.entranceLocation, Quaternion.Euler(0,heading,0)),
                        Task.CargoType.Rocket => Instantiate(rocketPrefab, spec.entranceLocation, Quaternion.Euler(0, heading, 0)),
                        Task.CargoType.LargeHauler => Instantiate(largeHaulerPrefab, spec.entranceLocation, Quaternion.Euler(0, heading, 0)),
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
