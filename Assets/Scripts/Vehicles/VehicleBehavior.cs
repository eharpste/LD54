using System.Collections;
using System.Collections.Generic;
using System.Transactions;
using TMPro.EditorUtilities;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
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
        Boost,
        Resupply
    }

    public enum CommandLoopStyle {
        LoopWholeList,
        LoopLast,
        Default
    }

    public enum Destination {
        Local,
        North,
        South,
        East,
        West,
        Up
    }

    public enum FlightState {
        Grounded,
        Flying
    }

    //TODO we don't actually want this to be a queue, we want it to be a list that we can loop through
    public List<Command> CommandList = new List<Command>();
    protected int currentCommand = -1;
    public CommandLoopStyle commandLoopStyle = CommandLoopStyle.Default;
    public Command defaultCommand = Command.Idle;
    [Tooltip("Where is the vehicle going? Local means landing here, otherwise what edge should it leave on?")]
    public Destination destination = Destination.Local;
    public int commandLimit = 3;
    public int fuel = 20;
    public int speed = 2;
    public int boostSpeed = 4;
    public int cargoValue;
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
    }

    public void SimulateNextCommand(float stepTime) {
        if (CommandList.Count > 0) {
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
        Destroy(this.gameObject);
    }

    /// <summary>
    /// When a vehicle successfully lands, usually provides some kind of score.
    /// </summary>
    public void Land() {
        //TODO probably override this as some kind of animation or other behavior
        Debug.LogFormat("Landed {0}", this.gameObject.name);
        flightState = FlightState.Grounded;
        CommandList.Clear();
        GameManager.Instance.RemoveVehicle(this);
        GameManager.Instance.LandingScore(this);
    }

    /// <summary>
    /// When a vehicle successfully departs the area, usually provides some kind of score and removes the vehicle.
    /// </summary>
    public void Depart(Destination departureDirection) {
        //TODO probably play some kind of sound or something
        Debug.LogFormat("Departed {0}, in {1}", this.gameObject.name, departureDirection.ToString());
        GameManager.Instance.DepartingScore(this, departureDirection);
        Destroy(this.gameObject);
    }


    protected virtual void OnTriggerEnter(Collider other) {
        if(other.gameObject.CompareTag("Edge") && destination != Destination.Local) {
            Destination departureDirection = other.gameObject.name switch {
                "North" => Destination.North,
                "South" => Destination.South,
                "East" => Destination.East,
                "West" => Destination.West,
                "Up" => Destination.Up,
                _ => Destination.Local
            };
            Depart(departureDirection);
        }
    }

    protected abstract IEnumerator SimulateCommandCoroutine(float secondsPerStep, Command command);

    protected virtual void OnDestroy() {
        GameManager.Instance.RemoveVehicle(this);
    }
}
