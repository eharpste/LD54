using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(BoxCollider), typeof(Rigidbody))]
public abstract class VehicleBehavior : MonoBehaviour {

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

    public enum CommandLoopStyle {
        LoopWholeList,
        LoopLast,
        Default
    }

    public enum CommandEditMode {
        NotEditable,
        EditOnLoop,
        EditPassedList
    }

    public enum FlightState {
        Grounded,
        Flying,
        Launching
    }

    public Task currentTask;
    //TODO we don't actually want this to be a queue, we want it to be a list that we can loop through
    [Tooltip("Note that you're usually not supposed to have access to all of these commands in all contexts.")]
    public List<Command> CommandList = new List<Command>();
    protected int currentCommand = -1;
    public CommandLoopStyle commandLoopStyle = CommandLoopStyle.Default;
    public Command defaultCommand = Command.Idle;
    public CommandEditMode comandEditMode = CommandEditMode.EditPassedList;
    public int commandLimit = 3;
    public int currentFuel = 20;
    public int maxFuel = 20;
    public int speed = 2;
    public int boostSpeed = 4;
    public FlightState flightState = FlightState.Flying;
    protected Rigidbody rb;


    // Start is called before the first frame update
    protected void Start() {
        GameManager.Instance.AddVehicle(this);
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    protected void Update() {
        
    }

    public void AddCommand(Command command) {
        if (CommandList.Count < commandLimit) {
            CommandList.Add(command);
        }
        else {
            switch (comandEditMode) {
                case CommandEditMode.NotEditable:
                    break;
                case CommandEditMode.EditOnLoop:
                    if(currentCommand % CommandList.Count == 0) {
                        CommandList.Clear();
                        CommandList.Add(command);
                        currentCommand = 0;
                    }
                    break;
                case CommandEditMode.EditPassedList:
                    if(currentCommand >= CommandList.Count) {
                        CommandList.Clear();
                        CommandList.Add(command);
                    }
                    break;
                default:
                    break;
            }
        }
    }

    public void SimulateNextCommand(float stepTime) {
        if (CommandList.Count > 0 || commandLoopStyle == CommandLoopStyle.Default) {
            currentCommand++;
            Command command = commandLoopStyle switch {
                CommandLoopStyle.LoopWholeList => CommandList[currentCommand % CommandList.Count],
                CommandLoopStyle.LoopLast => currentCommand >= CommandList.Count ? CommandList[CommandList.Count - 1] : CommandList[currentCommand],
                CommandLoopStyle.Default => currentCommand >= CommandList.Count ? defaultCommand : CommandList[currentCommand],
                _ => CommandList[currentCommand]
            };
            StartCoroutine(SimulateCommandCoroutine(stepTime, command));
        }
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
        CommandList.Clear();
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
        if(other.gameObject.CompareTag("Edge") && currentTask.destination != Task.Destination.Local) {
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

    protected virtual void OnDestroy() {
        GameManager.Instance.RemoveVehicle(this);
    }
}
