using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static VehicleBehavior;


[RequireComponent(typeof(BoxCollider), typeof(Rigidbody))]
public abstract class VehicleBehavior : MonoBehaviour {

    private static string[] NAMES = new string[] { "Crunchy", "Dollface", "Starsight", "Cuddly", "Zinger", "Ronin", "Groucho", "Starbreeze", "Hiccup", "Trendy", "Falcore", "Paradise",
    "Anthem", "Castle", "Zodiac", "Meagerie", "Gentleman", "Chai", "Mambo", "Melonball"};

    public static string GeneratePlaneName(string classKey) {
        return string.Format("SL {0}-{1} {2}", classKey, Random.Range(10,99), NAMES[Random.Range(0, NAMES.Length)]);
    }

    public enum Command {
        Idle,
        Forward,
        BankLeft,
        BankRight,
        YawLeft,
        YawRight,
        Climb,
        Dive,
        Raise,
        Lower,
        Boost,
        Unload
    }

    public enum FlightState {
        Grounded,
        Flying,
        Launching
    }

    public enum CommandEditingState {
        Unavailable,
        Editable,
        Executing,
    }

    public string ShipName = string.Empty;
    
    [SerializeField]
    private Task currentTask;
    public Task CurrentTask {
        get {
            return currentTask;
        }
        set {
            currentTask = value;
        }
    }
    //TODO we don't actually want this to be a queue, we want it to be a list that we can loop through
    
    public List<Command> PrevCommandList = new List<Command>();
    [Tooltip("Note that you're usually not supposed to have access to all of these commands in all contexts.")]
    public Command CurrentCommand {
        get {
            if(CommandQueue.Count > 0) {
                return CommandQueue[0];
            }
            else {
                return defaultCommand;
            }
        }
    }
    public List<Command> CommandQueue = new List<Command>();

    public CommandEditingState commandEdditingState = CommandEditingState.Editable;

    public bool LoopCommandList = false;

    //public CommandLoopStyle commandLoopStyle = CommandLoopStyle.Default;
    public Command defaultCommand = Command.Idle;
    //public CommandEditMode comandEditMode = CommandEditMode.EditPassedList;
    public int commandLimit = 3;
    public int currentFuel = 20;
    public int maxFuel = 20;
    public int speed = 2;
    public int boostSpeed = 4;
    public FlightState flightState = FlightState.Flying;
    protected Rigidbody rb;
    protected Collider col;


    // Start is called before the first frame update
    protected virtual void Start() {
        GameManager.Instance.AddVehicle(this);
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
        Ready = true;
    }

    public void AdvanceCommand() {
		if (CommandQueue.Count > 0) {
			CommandQueue.RemoveAt(0);
		}
        else {
			if (LoopCommandList && PrevCommandList.Count > 0)
			{
				CommandQueue.AddRange(PrevCommandList);
			}

			if (flightState == FlightState.Launching)
			{
				flightState = FlightState.Flying;
			}
		}

		if ((commandEdditingState == CommandEditingState.Executing) && (CommandQueue.Count == 0))
		{
			commandEdditingState = CommandEditingState.Editable;
		}

	}

    public void RemoveCommand(int index) {
        switch(commandEdditingState) {
            case CommandEditingState.Editable:
                CommandQueue.RemoveAt(index);
                break;
            //case CommandExecutionState.Defaulting:
            //    RepeatLastCommands();
            //    commandExecutionState = CommandExecutionState.Editing;
            //    goto case CommandExecutionState.Editing;
        }
    }

    public void RepeatLastCommands() {
        switch (commandEdditingState) {
            //case CommandExecutionState.Defaulting:
            case CommandEditingState.Editable:
                CommandQueue.Clear();
                CommandQueue.AddRange(PrevCommandList);
                //commandExecutionState = CommandExecutionState.Editing;
                break;   
        }
    }

    public void SwapCommands(int index1, int index2) {
        switch (commandEdditingState) {
            case CommandEditingState.Editable:
                Command swap = CommandQueue[index1];
                CommandQueue[index1] = CommandQueue[index2];
                CommandQueue[index2] = swap;
                break;
            //case CommandExecutionState.Defaulting:
            //    RepeatLastCommands();
            //    commandExecutionState = CommandExecutionState.Editing;
            //    goto case CommandExecutionState.Editing;
        }

    }

    public void AddCommand(Command command) {
        switch (commandEdditingState) {
            case CommandEditingState.Editable:
                if (CommandQueue.Count < commandLimit) {
                    CommandQueue.Add(command);
                }
                break;
            //case CommandExecutionState.Defaulting:
            //    CommandQueue.Clear();
            //    CommandQueue.Add(command);
            //    commandExecutionState = CommandExecutionState.Editing;
            //    break;
        }
    }

    public void SimulateNextCommand(float stepTime) {
        Command command = defaultCommand;
        if(commandEdditingState == CommandEditingState.Editable) {
            PrevCommandList.Clear();
            PrevCommandList.AddRange(CommandQueue);
            commandEdditingState = CommandEditingState.Executing;
        }


        //switch (commandExecutionState) {
        //    case CommandExecutionState.Defaulting:
        //        command = defaultCommand;
        //        break;
        //    case CommandExecutionState.Unavailable:
        //        if(CommandQueue.Count == 0) {
        //            goto case CommandExecutionState.Defaulting;
        //        }
        //        command = CommandQueue[currentCommandIndex % CommandQueue.Count];
        //        break;
        //    case CommandExecutionState.Editing:
        //        PrevCommandList.Clear();
        //        PrevCommandList.AddRange(CommandQueue);
        //        currentCommandIndex = -1;
        //        commandExecutionState = CommandExecutionState.Executing;
        //        goto case CommandExecutionState.Executing;
        //    case CommandExecutionState.Executing:
        //        currentCommandIndex++;
        //        if (currentCommandIndex < CommandQueue.Count) {
        //            command = CommandQueue[currentCommandIndex];
        //        }
        //        else {
        //            commandExecutionState = CommandExecutionState.Defaulting;
        //            if(flightState == FlightState.Launching) {
        //                flightState = FlightState.Flying;
        //            }
        //        }
        //        break;
        //}


        StartCoroutine(SimulateCommandCoroutine(stepTime, CurrentCommand));

        AdvanceCommand();

		//if (CurrentCommandList.Count > 0 || commandLoopStyle == CommandLoopStyle.Default) {
		//    currentCommand++;
		//    Command command = commandLoopStyle switch {
		//        CommandLoopStyle.LoopWholeList => CurrentCommandList[currentCommand % CurrentCommandList.Count],
		//        CommandLoopStyle.LoopLast => currentCommand >= CurrentCommandList.Count ? CurrentCommandList[CurrentCommandList.Count - 1] : CurrentCommandList[currentCommand],
		//        CommandLoopStyle.Default => currentCommand >= CurrentCommandList.Count ? defaultCommand : CurrentCommandList[currentCommand],
		//        _ => CurrentCommandList[currentCommand]
		//    };
		//    StartCoroutine(SimulateCommandCoroutine(stepTime, command));
		//}
	}

    /// <summary>
    /// Sets the entire command queue and forces the vehicle into executing mode. Also clears the previous command list.
    /// </summary>
    /// <param name="commands"></param>
    public void SetCommands(List<Command> commands) {
        PrevCommandList.Clear();
        CommandQueue = commands;
        commandEdditingState = CommandEditingState.Executing;
	}

    private void OnCollisionEnter(Collision collision) {
        switch(collision.gameObject.tag) {
            case "Terrain":
                if (flightState == FlightState.Flying) {
                    Crash();
                }
                break;
            case "Vehicle":
                Crash();
                collision.gameObject.GetComponent<VehicleBehavior>().Crash();
                break;
            default:
                Debug.LogWarningFormat("Hit something with Tag {0}, we should probalby crash?", collision.gameObject.tag);
                Crash();
                break;
        }
    }

    /// <summary>
    /// When a vehicle crashes, could be into anothre vehicle or into the terrain.
    /// </summary>
    public void Crash() {
        //TODO probably play some kind of animation or something
        Debug.LogFormat("Crashed {0}", this.gameObject.name);
        StartCoroutine(RagDollDelay());
    }

    IEnumerator RagDollDelay() {
        rb.useGravity = true;
        rb.isKinematic = false;
        yield return new WaitForSeconds(1);
        Destroy(this.gameObject);
    }

    /// <summary>
    /// When a vehicle successfully lands, usually provides some kind of score.
    /// </summary>
    public void Land(Landing landing=null) {
        //TODO probably override this as some kind of animation or other behavior
        Debug.LogFormat("Landed {0}", this.gameObject.name);
        rb.isKinematic = true;
        flightState = FlightState.Grounded;
        CommandQueue.Clear();
        if (CurrentTask.taskType == Task.TaskType.Arrival) {
            GameManager.Instance.ScoreTask(CurrentTask);
            CurrentTask = null;
        }
        if (landing != null) {
            landing.LandVehicle(this);
        }
        //CommandList.Clear();

        //GameManager.Instance.RemoveVehicle(this);
        
    }

    /// <summary>
    /// When a vehicle successfully departs the area, usually provides some kind of score and removes the vehicle.
    /// </summary>
    public void Depart(Task.Destination departureDirection) {
        //TODO probably play some kind of sound or something
        Debug.LogFormat("Departed {0}, in {1}", this.gameObject.name, departureDirection.ToString());
        if (CurrentTask != null) {
            switch (CurrentTask.taskType) {
                case Task.TaskType.Departure:
                case Task.TaskType.Flyby:
                    GameManager.Instance.ScoreTask(CurrentTask, CurrentTask.departureModifier(departureDirection));
                    break;
                default:
                    GameManager.Instance.ScoreTask(CurrentTask, 0);
                    break;
            }
        }
        else {
            GameManager.Instance.ScoreTask(CurrentTask, 0);
        }
        Destroy(this.gameObject);
    }


    protected virtual void OnTriggerEnter(Collider other) {
        if(other.gameObject.CompareTag("Edge")) {
            Task.Destination departureDirection = other.gameObject.name switch {
                "North" => Task.Destination.North,
                "South" => Task.Destination.South,
                "East" => Task.Destination.East,
                "West" => Task.Destination.West,
                "Up" => Task.Destination.Up,
                _ => Task.Destination.Local
            };
            Depart(departureDirection);
        }
        
    }

    protected abstract IEnumerator SimulateCommandCoroutine(float secondsPerStep, Command command);

    public abstract IEnumerable<Command> GetAvailableCommands();

    public virtual bool Ready { protected set; get; }

    protected virtual void OnDestroy() {
        GameManager.Instance.RemoveVehicle(this);
    }


}
