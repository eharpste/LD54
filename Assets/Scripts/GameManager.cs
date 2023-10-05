using System.Collections;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class GameManager : MonoBehaviour
{

    public enum ChallengeListSetting {
        Loop,
        Generate,
        End
            
    }

    public static GameManager Instance { private set; get; }

    List<VehicleBehavior> Vehicles = new List<VehicleBehavior>();
    List<Landing> Landings = new List<Landing>();
    List<Runway> Runways = new List<Runway>();
    List<HoverPad> HoverPads = new List<HoverPad>();
    List<RocketLauncher> RocketLaunchers = new List<RocketLauncher>();

    public int score = 0;
    public bool verboseDebug = false;

    [Header("Time Settings")]
    public int CurrentTime = 0;
    public float secondsPerStep = 1f;

    [Header("Control Settings")]
    public bool isShipSelected = false;
    public VehicleBehavior selectedVehicle;
    public Landing selectedLocation;
    public LayerMask vehicleMask;
    public LayerMask LandingLocationMask;

    /// <summary>
    /// North = Positive Z, heading -90, x=9
    /// West = Positive X, heading 0, z=9
    /// East = Negative X, heading 180, z=0
    /// South = Negative Z, heading 90, x=0
    /// </summary>
    [Header("Randomizer Settings")]
    public int ceiling = 6;
    public int floor = 3;

    [Tooltip("What is the x coordinate of the north edge.")]
    public int northX = 9;
    [Tooltip("What is the x coordinate of the south edge.")]
    public int southX = 0;
    [Tooltip("What is the z coordinate of the east edge.")]
    public int eastZ = 0;
    [Tooltip("What is the z coordinate of the west edge.")]
    public int westZ = 9;

    [Tooltip("What y rotation should something have when it enters the North edge.")]
    public int northHeading = -90;
    [Tooltip("What y rotation should something have when it enters the South edge.")]
    public int southHeading = 90;
    [Tooltip("What y rotation should something have when it enters the East edge.")]
    public int eastHeading = 180;
    [Tooltip("What y rotation should something have when it enters the West edge.")]
    public int westHeading = 0;

    [Header("Prefabs")]
    public GameObject planePrefab;
    public GameObject hoverLanderPrefab;
    public GameObject rocketPrefab;
    public GameObject largeHaulerPrefab;
    public GameObject warningSignPrefab;

    private List<GameObject> warningMarkers = new List<GameObject>();

    [Header("Task Collections")]
    [SerializeField]
    private List<Task> inboundPassengerTasks = new List<Task>();
    [SerializeField]
    private List<Task> inboundCargoTasks = new List<Task>();
    [SerializeField]
    private List<Task> outboundPassengerTasks = new List<Task>();
    [SerializeField]
    private List<Task> outboundCargoTasks = new List<Task>();
    [SerializeField]
    private List<Task> flybyTasks = new List<Task>();
    [SerializeField]
    private Task largeHaulerTask;



	//public List<Task> currentTasks = new List<Task>();
	//These are any arrivals that will be appearing in the next time step
	//public List<TaskSpec> pendingArrivals = new List<TaskSpec>();
	//These are the departurs that haven't been handed to a vehicle yet.
	private List<Task> pendingDepartures = new List<Task>();

    [HideInInspector]
    public ChallengeListSetting challengeListSetting = ChallengeListSetting.Loop;

    [HideInInspector]
    public List<TaskFrame> Challenges = new List<TaskFrame>();

    [System.Serializable]
    public class TaskFrame {
        [SerializeField]
        string name { get {
                return string.Format("{0}-{1}-{2}-{3}-{4}-{5}-{6}",
                    inboundCargo, inboundPassenger,
                    outboundCargo, outboundPassenger,
                    flybys, rockets, haulers);
            } 
        }

        [SerializeField]
        public int inboundCargo = 0;
        [SerializeField]
        public int inboundPassenger = 0;
        [SerializeField]
        public int outboundCargo = 0;
        [SerializeField]
        public int outboundPassenger = 0;
        [SerializeField]
        public int flybys = 0;
        [SerializeField]
        public int rockets = 0;
        [SerializeField]
        public int haulers = 0;
    }

    private struct PositionedTask {
        public Task task;
        public Vector3 position;
        public int heading;

        public PositionedTask(Task task, Vector3 position, int heading) {
            this.task = task;
            this.position = position;
            this.heading = heading;
        }
    }

    private List<PositionedTask> VehiclesToSpawn = new List<PositionedTask>();

   /* [Header("Task Settings")]
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
    }*/

    /*[ExecuteInEditMode]
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
    }*/


    public void AddVehicle(VehicleBehavior vehicle) {
        Vehicles.Add(vehicle);
        if(vehicle.CurrentTask != null) {
            //currentTasks.Add(vehicle.currentTask);
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
        if (Challenges.Count > 0) {
            SpawnTasks(Challenges[0]);
        }
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
				SelectOnScreen();
			}
			
		}
	}

    private void SelectOnScreen()
    {
        Ray camRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        //Debug.DrawRay(camRay.origin, camRay.direction * 1000, Color.white, 2f);

		RaycastForVehicles(camRay);
		RaycastforLocations(camRay);

	}

    private bool RaycastforLocations(Ray camRay)
    {
        RaycastHit hitLocInfo;
        bool hitLocation = Physics.Raycast(camRay, out hitLocInfo, 9999f, LandingLocationMask);
        if (hitLocation == true)
        {
			selectedVehicle = null;
			selectedLocation = hitLocInfo.collider.gameObject.GetComponent<Landing>();
            Events.SelectLocationEvent();
            return true;
        }
        else
        {
            selectedLocation = null;
            return false;
        }
    }

    private bool RaycastForVehicles(Ray camRay)
    {
        RaycastHit hitInfo;
        bool hitVehicle = Physics.Raycast(camRay, out hitInfo, 9999f, vehicleMask);
        if (hitVehicle == true)
        {
			selectedVehicle = hitInfo.collider.gameObject.GetComponent<VehicleBehavior>();
            Events.SelectVehicleEvent();
            return true;
        }
        else
        {
            selectedVehicle = null;
            return false;
        }
    }

    public void CreateWarning(Vector3 placement) {
        warningMarkers.Add(Instantiate(warningSignPrefab, placement, Quaternion.identity));
    }

    public void SimulateStep() {
        foreach (VehicleBehavior vehicleBehavior in Vehicles) {
            if (!vehicleBehavior.Ready) {
                Debug.LogWarningFormat("{0} is not ready", vehicleBehavior.name);
                return;
            }
        }
        foreach (Landing landing in Landings) {
            if (!landing.Ready) {
                Debug.LogWarningFormat("{0} is not ready", landing.name);
                return;
            }
        }

        StartCoroutine(SimulateStepCoroutine());
    }

    public IEnumerator SimulateStepCoroutine() {
        //Advance time
        CurrentTime++;

        //Simulate All Vehicles
        foreach (VehicleBehavior vehicle in Vehicles) {
            vehicle.SimulateNextCommand(secondsPerStep);
        }

        //Simulate All Landings
        foreach (Landing landing in Landings) {
            landing.SimulateStep(secondsPerStep);
        }


        bool ready = false;
        while (!ready) {
            ready = true;
            foreach (VehicleBehavior vehicle in Vehicles) {
                if (!vehicle.Ready) {
                    ready = false;
                }
            }
            foreach (Landing landing in Landings) {
                if (!landing.Ready) {
                    ready = false;
                }
            }
            yield return null;
        }

        Events.UpdateVehicleEvent();


        //Clear the Warning Signs
        foreach (GameObject sign in warningMarkers) {
            Destroy(sign);
        }
        warningMarkers.Clear();

        //Spawn Pending Vehicles
        foreach(PositionedTask placement in VehiclesToSpawn) {
            GameObject instantiatedPrefab = placement.task.cargoType switch {
                Task.CargoType.Passenger => Instantiate(planePrefab, placement.position, Quaternion.Euler(0, placement.heading, 0)),
                Task.CargoType.Cargo => Instantiate(hoverLanderPrefab, placement.position, Quaternion.Euler(0, placement.heading, 0)),
                Task.CargoType.LargeHauler => Instantiate(largeHaulerPrefab, placement.position, Quaternion.Euler(0, placement.heading, 0)),
                _ => null,
            };
            if (instantiatedPrefab != null) {
                VehicleBehavior newVehicle;
                newVehicle = instantiatedPrefab.GetComponent<VehicleBehavior>();
                newVehicle.currentFuel = placement.task.fuel;
                newVehicle.CurrentTask = placement.task;
                newVehicle.ShipName = placement.task.shipName;
                newVehicle.SetCommands(new List<VehicleBehavior.Command>() { VehicleBehavior.Command.Forward });
                Events.UpdateVehicleEvent();
                VerboseDebug("Launching {0} at {1} with landingHeading {2}", newVehicle.name, newVehicle.transform.position, newVehicle.transform.eulerAngles.y);
            }
        }

        VehiclesToSpawn.Clear();

        //Spawn New Tasks
        if (CurrentTime < Challenges.Count) {
            SpawnTasks(Challenges[CurrentTime]);
        }
        else {
            switch (challengeListSetting) {
                case ChallengeListSetting.Loop:
                    SpawnTasks(Challenges[CurrentTime % Challenges.Count]);
                    break;
                case ChallengeListSetting.Generate:
                    Debug.LogWarning("Reached the end of the challenge list in generate mode but haven't implemented this yet");
                    break;
            }
        }
    }

    public List<Task> GetPendingDepatures(Task.CargoType cargoType) {
        List<Task> result = new List<Task>();
        foreach (Task task in pendingDepartures) {
            if (task.cargoType == cargoType) {
                result.Add(task);
            }
        }
        return result;
    }


    private void VerboseDebug(string template, params object[] args) {
        if (verboseDebug) {
            Debug.LogFormat(template, args);
        }

        //Events.UpdateVehicleEvent();
    }

    public void SpawnTasks(TaskFrame tasks) { 
        for(int i = 0; i < tasks.inboundPassenger; i++) {
            //create a random arrival passenger task
            //make a random entrance location
            if (inboundPassengerTasks.Count == 0) break;
            Task task = inboundPassengerTasks[Random.Range(0, inboundPassengerTasks.Count)];
            int edge = Random.Range(0, 4);
            Vector3 spawnPosition = Vector3.zero;
            int heading = 0;
            switch (edge) {
                //North
                case 0:
                    spawnPosition = new Vector3(northX, Random.Range(floor,ceiling), Random.Range(Mathf.Min(westZ,eastZ) + 1, Mathf.Max(westZ,eastZ)));
                    heading = northHeading;
                    break;
                //South
                case 1:
                    spawnPosition = new Vector3(southX, Random.Range(floor, ceiling), Random.Range(Mathf.Min(westZ, eastZ) + 1, Mathf.Max(westZ, eastZ)));
                    heading = southHeading;
                    break;
                //East
                case 2:
                    spawnPosition = new Vector3(Random.Range(Mathf.Min(northX, southX) + 1, Mathf.Max(northX, southX)), Random.Range(floor, ceiling), eastZ);
                    CreateWarning(spawnPosition);
                    heading = eastHeading;
                    break;
                //West
                case 3:
                    spawnPosition = new Vector3(Random.Range(Mathf.Min(northX, southX) + 1, Mathf.Max(northX, southX)), Random.Range(floor, ceiling), westZ);
                    CreateWarning(spawnPosition);
                    heading = westHeading;
                    break;
            }
            CreateWarning(spawnPosition);
            VehiclesToSpawn.Add(new PositionedTask(task, spawnPosition, heading));
            VerboseDebug("Spawning {0} at {1} with landingHeading {2}", task.cargoType, spawnPosition, heading);
        }
        for (int i = 0; i < tasks.inboundCargo; i++) {
            if (inboundCargoTasks.Count == 0) break;
            Task task = inboundCargoTasks[Random.Range(0, inboundCargoTasks.Count)];
            int edge = Random.Range(0, 4);
            Vector3 spawnPosition = Vector3.zero;
            int heading = 0;
            switch (edge) {
                //North
                case 0:
                    spawnPosition = new Vector3(northX, Random.Range(floor, ceiling), Random.Range(Mathf.Min(westZ, eastZ) + 1, Mathf.Max(westZ, eastZ)));
                    heading = northHeading;
                    break;
                //South
                case 1:
                    spawnPosition = new Vector3(southX, Random.Range(floor, ceiling), Random.Range(Mathf.Min(westZ, eastZ) + 1, Mathf.Max(westZ, eastZ)));
                    heading = southHeading;
                    break;
                //East
                case 2:
                    spawnPosition = new Vector3(Random.Range(Mathf.Min(northX, southX) + 1, Mathf.Max(northX, southX)), Random.Range(floor, ceiling), eastZ);
                    heading = eastHeading;
                    break;
                //West
                case 3:
                    spawnPosition = new Vector3(Random.Range(Mathf.Min(northX, southX) + 1, Mathf.Max(northX, southX)), Random.Range(floor, ceiling), westZ);
                    heading = westHeading;
                    break;
            }
            CreateWarning(spawnPosition);
            VehiclesToSpawn.Add(new PositionedTask(task, spawnPosition, heading));
            VerboseDebug("Spawning {0} at {1} with landingHeading {2}", task.cargoType, spawnPosition, heading);
        }
        for (int i = 0; i < tasks.outboundPassenger; i++) {
            //create a random departure passenger task
            if(outboundPassengerTasks.Count == 0) break;
            Task task = outboundPassengerTasks[Random.Range(0, outboundPassengerTasks.Count)];
            pendingDepartures.Add(task);
            VerboseDebug("Adding {0} to pending departures", task.cargoType);
        }
        for (int i = 0; i < tasks.outboundCargo; i++) {
            if(outboundCargoTasks.Count == 0) break;
            //create a random departure cargo task
            Task task = outboundCargoTasks[Random.Range(0, outboundCargoTasks.Count)];
            pendingDepartures.Add(task);
            VerboseDebug("Adding {0} to pending departures", task.cargoType);
        }
        for(int i = 0; i < tasks.flybys; i++) {
            Task task = flybyTasks[Random.Range(0, flybyTasks.Count)];
            int edge = task.destination switch {
                Task.Destination.South => 0,
                Task.Destination.North => 1,
                Task.Destination.East => 3,
                Task.Destination.West => 2,
                _ => 0
            };
            Vector3 spawnPosition = Vector3.zero;
            int heading = 0;
            switch (edge) {
                //North
                case 0:
                    spawnPosition = new Vector3(northX, Random.Range(floor, ceiling), Random.Range(Mathf.Min(westZ, eastZ) + 1, Mathf.Max(westZ, eastZ)));
                    heading = northHeading;
                    break;
                //South
                case 1:
                    spawnPosition = new Vector3(southX, Random.Range(floor, ceiling), Random.Range(Mathf.Min(westZ, eastZ) + 1, Mathf.Max(westZ, eastZ)));
                    heading = southHeading;
                    break;
                //East
                case 2:
                    spawnPosition = new Vector3(Random.Range(Mathf.Min(northX, southX) + 1, Mathf.Max(northX, southX)), Random.Range(floor, ceiling), eastZ);
                    heading = eastHeading;
                    break;
                //West
                case 3:
                    spawnPosition = new Vector3(Random.Range(Mathf.Min(northX, southX) + 1, Mathf.Max(northX, southX)), Random.Range(floor, ceiling), westZ);
                    heading = westHeading;
                    break;
            }
            //create a random flyby task
            CreateWarning(spawnPosition);
            VehiclesToSpawn.Add(new PositionedTask(task, spawnPosition, heading));
            VerboseDebug("Spawning {0} at {1} with landingHeading {2}", task.cargoType, spawnPosition, heading);
        }
        for (int i = 0; i < tasks.rockets; i++) {
            if (rocketPrefab == null) {
                Debug.LogError("No rocket prefab assigned to game manager");
                break;
            }
            //create a random rocket task
            foreach (RocketLauncher launcher in RocketLaunchers) {
                if (launcher.AvailableToLaunch) {
                    GameObject newRocket = Instantiate(rocketPrefab, launcher.transform.position + Vector3.down * 100, Quaternion.identity);
                    CreateWarning(launcher.transform.position + Vector3.up);
                    VehicleBehavior vehicleBehavior = newRocket.GetComponent<VehicleBehavior>();
                    launcher.LaunchVehicle(vehicleBehavior);
                    break;
                }
            }
        }
        for (int i = 0; i < tasks.haulers; i++) {
            if(largeHaulerTask == null) {
                Debug.LogError("No large hauler prefab assigned to game manager");
                break;
            }
            //create a random hauler task
            int edge = Random.Range(0, 4);
            Vector3 spawnPosition1 = Vector3.zero;
            Vector3 spawnPosition2 = Vector3.zero;
            Vector3 offset = Vector3.zero;
            int heading = 0;
            switch (edge) {
                case 0:
                    spawnPosition1 = new Vector3(northX, Random.Range(floor+1, Mathf.Max(floor+1, ceiling-1)), Random.Range(Mathf.Min(westZ, eastZ) + 3, Mathf.Max(westZ, eastZ)-2));
                    spawnPosition2 = spawnPosition1 + new Vector3(0, 0, 1);
                    offset = new Vector3(-.5f, 0, 0);
                    heading = northHeading;
                    break;
                case 1:
                    spawnPosition1 = new Vector3(southX, Random.Range(floor + 1, Mathf.Max(floor + 1, ceiling - 1)), Random.Range(Mathf.Min(westZ, eastZ) + 3, Mathf.Max(westZ, eastZ) - 2));
                    spawnPosition2 = spawnPosition1 + new Vector3(0, 0, 1);
                    offset = new Vector3(.5f, 0, 0);
                    heading = southHeading;
                    break;
                case 2:
                    spawnPosition1 = new Vector3(Random.Range(Mathf.Min(northX, southX) + 3, Mathf.Max(northX, southX) - 2), 
                                                 Random.Range(floor + 1, Mathf.Max(floor + 1, ceiling - 1)), 
                                                 eastZ);
                    spawnPosition2 = spawnPosition1 + new Vector3(1, 0, 0);
                    offset = new Vector3(0, 0, .5f);
                    heading = eastHeading;
                    break;
                case 3:
                    spawnPosition1 = new Vector3(Random.Range(Mathf.Min(northX, southX) + 3, Mathf.Max(northX, southX) - 2),
                                                 Random.Range(floor + 1, Mathf.Max(floor + 1, ceiling - 1)),
                                                 westZ);
                    spawnPosition2 = spawnPosition1 + new Vector3(1, 0, 0);
                    offset = new Vector3(0, 0, -.5f);
                    heading = westHeading;
                    break;
            }

            CreateWarning(spawnPosition1);
            CreateWarning(spawnPosition2);
            VehiclesToSpawn.Add(new PositionedTask(largeHaulerTask, (spawnPosition1 + spawnPosition2) / 2 + offset, heading));
            VerboseDebug("Spawning {0} at {1} with landingHeading {2}", largeHaulerTask.cargoType, (spawnPosition1 + spawnPosition2) / 2 + offset, heading);
        }




        
        //List<TaskSpec> specs) {
        //foreach(TaskSpec spec in specs) {
        //    switch (spec.task.taskType) {
        //        case Task.TaskType.Arrival:
        //        case Task.TaskType.Flyby:
        //            int heading = 0;
        //            /// <summary>
        //            /// North = Positive Z, heading -90, x=9
        //            /// West = Positive X, heading 0, z=9
        //            /// East = Negative X, heading 180, z=0
        //            /// South = Negative Z, heading 90, x=0
        //            /// </summary>
        //            if (spec.entranceLocation.x == 9) { heading = -90; }
        //            else if(spec.entranceLocation.x == 0) { heading = 90; }
        //            else if(spec.entranceLocation.z == 0) { heading = 180; }
        //            else if(spec.entranceLocation.z == 9) { heading = 0; }
        //            else { Debug.LogWarningFormat("Don't know how to align EntranceLocation {0} setting identity.", spec.entranceLocation); }


        //            GameObject instantiatedPrefab =  spec.task.cargoType switch {
        //                Task.CargoType.Passenger => Instantiate(planePrefab, spec.entranceLocation, Quaternion.Euler(0, heading, 0)),
        //                Task.CargoType.Cargo => Instantiate(hoverLanderPrefab, spec.entranceLocation, Quaternion.Euler(0,heading,0)),
        //                Task.CargoType.Rocket => Instantiate(rocketPrefab, spec.entranceLocation, Quaternion.Euler(0, heading, 0)),
        //                Task.CargoType.LargeHauler => Instantiate(largeHaulerPrefab, spec.entranceLocation, Quaternion.Euler(0, heading, 0)),
        //                _ => null,
        //            };
        //            if (instantiatedPrefab != null) {
        //                VehicleBehavior newVehicle;
        //                newVehicle = instantiatedPrefab.GetComponent<VehicleBehavior>();
        //                newVehicle.currentFuel = spec.task.fuel;
        //                newVehicle.currentTask = spec.task;
        //                newVehicle.SetCommands(new List<VehicleBehavior.Command>() { VehicleBehavior.Command.Forward });
        //            }
        //            currentTasks.Add(spec.task);
        //            break;
        //        case Task.TaskType.Departure:
        //            currentTasks.Add(spec.task);
        //            pendingDepartures.Add(spec.task);
        //            break;
        //    }
        //}
    }

    public void ResetGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
	}


    public void ScoreTask(Task task, float modifier=1.0f) {
        if (task == null) {
            Debug.LogWarning("Trying to score a null task.");
            return;
        }
        score += (int)(task.value * modifier);
        //currentTasks.Remove(task);
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
