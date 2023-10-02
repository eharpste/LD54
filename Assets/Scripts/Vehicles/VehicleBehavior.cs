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

    //public enum CommandLoopStyle {
    //    LoopWholeList,
    //    LoopLast,
    //    Default
    //}

    //public enum CommandEditMode {
    //    NotEditable,
    //    EditOnLoop,
    //    EditPassedList
    //}

    public enum FlightState {
        Grounded,
        Flying,
        Launching
    }

    public enum CommandExecutionState {
        Unavailable,
        Defaulting,
        Executing,
        Editing
    }

    public Task currentTask;
    //TODO we don't actually want this to be a queue, we want it to be a list that we can loop through
    
    public List<Command> PrevCommandList = new List<Command>();
    [Tooltip("Note that you're usually not supposed to have access to all of these commands in all contexts.")]
    public List<Command> CurrentCommandList = new List<Command>();
    public int currentCommand { get; protected set; } = -1;
    public CommandExecutionState commandExecutionState = CommandExecutionState.Defaulting;

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

    public void RemoveCommand(int index) {
        switch(commandExecutionState) {
            case CommandExecutionState.Editing:
                CurrentCommandList.RemoveAt(index);
                break;
            case CommandExecutionState.Defaulting:
                RepeatLastCommands();
                commandExecutionState = CommandExecutionState.Editing;
                goto case CommandExecutionState.Editing;
        }
    }

    public void RepeatLastCommands() {
        switch (commandExecutionState) {
            case CommandExecutionState.Defaulting:
            case CommandExecutionState.Editing:
                CurrentCommandList = PrevCommandList;
                commandExecutionState = CommandExecutionState.Editing;
                break;   
        }
    }

    public void SwapCommands(int index1, int index2) {
        switch (commandExecutionState) {
            case CommandExecutionState.Editing:
                Command swap = CurrentCommandList[index1];
                CurrentCommandList[index1] = CurrentCommandList[index2];
                CurrentCommandList[index2] = swap;
                break;
            case CommandExecutionState.Defaulting:
                RepeatLastCommands();
                commandExecutionState = CommandExecutionState.Editing;
                goto case CommandExecutionState.Editing;
        }

    }

    public void AddCommand(Command command) {
        switch (commandExecutionState) {
            case CommandExecutionState.Editing:
                if (CurrentCommandList.Count < commandLimit) {
                    CurrentCommandList.Add(command);
                }
                break;
            case CommandExecutionState.Defaulting:
                CurrentCommandList.Clear();
                CurrentCommandList.Add(command);
                commandExecutionState = CommandExecutionState.Editing;
                break;
        }
    }

    public void SimulateNextCommand(float stepTime) {
        Command command = defaultCommand;
        switch (commandExecutionState) {
            case CommandExecutionState.Defaulting:
                command = defaultCommand;
                break;
            case CommandExecutionState.Unavailable:
                currentCommand++;
                if(CurrentCommandList.Count == 0) {
                    goto case CommandExecutionState.Defaulting;
                }
                command = CurrentCommandList[currentCommand % CurrentCommandList.Count];
                break;
            case CommandExecutionState.Editing:
                PrevCommandList.Clear();
                PrevCommandList.AddRange(CurrentCommandList);
                currentCommand = -1;
                commandExecutionState = CommandExecutionState.Executing;
                goto case CommandExecutionState.Executing;
            case CommandExecutionState.Executing:
                currentCommand++;
                if (currentCommand < CurrentCommandList.Count) {
                    command = CurrentCommandList[currentCommand];
                }
                else {
                    commandExecutionState = CommandExecutionState.Defaulting;
                    if(flightState == FlightState.Launching) {
                        flightState = FlightState.Flying;
                    }
                }
                break;
        }
        StartCoroutine(SimulateCommandCoroutine(stepTime, command));



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

    public void SetCommands(List<Command> commands) {
        CurrentCommandList = commands;
        commandExecutionState = CommandExecutionState.Executing;
        currentCommand = -1;

	}

    private void OnCollisionEnter(Collision collision) {
        switch(collision.gameObject.tag) {
            case "Terrain":
                if (flightState == FlightState.Flying) {
                    Crash();
                }
                break;
            case "Vehicles":
                Crash();
                collision.gameObject.GetComponent<VehicleBehavior>().Crash();
                break;
            default:
                Debug.LogWarningFormat("Hit something with Tag {0}", collision.gameObject.tag);
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
        CurrentCommandList.Clear();
        GameManager.Instance.ScoreTask(currentTask);
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
        switch (currentTask.taskType) {
            case Task.TaskType.Departure:
            case Task.TaskType.Flyby:
                GameManager.Instance.ScoreTask(currentTask, currentTask.departureModifier(departureDirection));
                break;
            default:
                GameManager.Instance.ScoreTask(currentTask, 0);
                break;
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
