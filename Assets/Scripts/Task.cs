using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Task", menuName = "ScriptableObjects/Task", order = 1)]
public class Task : ScriptableObject {

    public static Task GenerateRandomTask() {
        Task task = ScriptableObject.CreateInstance<Task>();
        task.cargoType = Random.value > .5f  ? CargoType.Cargo : CargoType.Passenger;
        task.taskType = Random.value > .5f ? TaskType.Departure : TaskType.Arrival;
        task.penalizeWrongDeparture = true;
        task.responsive = true;
        float rand = Random.value;
        switch (task.taskType, task.cargoType) {
            case (TaskType.Departure, CargoType.Cargo):
                task.taskName = "Regular Shipments";
                task.pilotBlurb = "I have the usual load of miscileaneous cargo to get in the air";
                task.value = ((int)Mathf.Round(Random.Range(50, 800)/50))*10;
                rand = Random.value;
                if (rand < .25f) {
                    task.destination = Destination.North;
                }
                else if (rand < .5f) {
                    task.destination = Destination.West;
                }
                else if (rand < .75f) {
                    task.destination = Destination.South;
                }
                else {
                    task.destination = Destination.East;
                }
                task.fuel = 20;
                break;
            case (TaskType.Arrival, CargoType.Cargo):
                task.taskName = "Regular Supplies";
                task.pilotBlurb = "I have the monthly supply shipment inbound";
                task.value = ((int)Mathf.Round(Random.Range(50, 800)/50))*10;
                task.fuel = 20;
                task.shipName = VehicleBehavior.GeneratePlaneName("C");
                task.destination = Destination.Local;
                break;
            case (TaskType.Arrival, CargoType.Passenger):
                task.taskName = "Frequent Fliers";
                task.pilotBlurb = "Got another batch of regulars inbound.";
                task.value = ((int)Mathf.Round(Random.Range(50, 800)/50))*10;
                task.fuel = Random.Range(14, 20);
                task.shipName = VehicleBehavior.GeneratePlaneName("C");
                task.destination = Destination.Local;
                break;
            case (TaskType.Departure, CargoType.Passenger):
                task.taskName = "Hub Connections";
                task.pilotBlurb = "Another group of passengers on their way to make connections in our hub station.";
                task.value = ((int)Mathf.Round(Random.Range(50, 800)/50))*10;
                task.fuel = 20;
                rand = Random.value;
                if (rand < .25f) {
                    task.destination = Destination.North;
                }
                else if (rand < .5f) {
                    task.destination = Destination.West;
                }
                else if (rand < .75f) {
                    task.destination = Destination.South;
                }
                else {
                    task.destination = Destination.East;
                }
                break;
        }
        return task;
    }


    [Tooltip("The Display name of the task.")]
    public string taskName;

    [Tooltip("A short narrative message about the task framed as a message from the pilot.")]
    [TextArea(3,8)]
    public string pilotBlurb;

    [Tooltip("How much is this task worth?")]
    public int value;

    public enum CargoType { Cargo, Passenger, Rocket, LargeHauler };
    [Tooltip("What kind of cargo are we carrying? Implicitly, what kind of vehicle is it?")]
    public CargoType cargoType;

    public enum TaskType { Arrival, Departure, Flyby };
    public TaskType taskType;

    [Tooltip("If False, the ship will not allow commands. Used for Galatic Unlimited ships.")]
    public bool responsive = true;

    /// <summary>
    /// North = Positive Z, heading -90
    /// West = Positive X, heading 0
    /// East = Negative X, heading 180
    /// South = Negative Z, heading 90
    /// </summary>
    public enum Destination {
        Local,
        North, /// North = Positive Z, heading -90
        South, /// South = Negative Z, heading 90
        East,  /// East  = Negative X, heading 180
        West,  /// West  = Positive X, heading 0
        Up     /// Up    = Positive Y, 
    }


    [Header("Used by Arrival and Flyby Tasks")]
    [Tooltip("A narrative name for the ship / type of ship. *Ignored by Departures*")]
    public string shipName;
    [Tooltip("How much fuel does the ship arrive with? *Ignored by Departures*")]
    public int fuel;

    [Header("Used by Departure Tasks")]
    [Tooltip("Where is the ship going? *Ignored by Arrivals*")]
    public Destination destination;
    [Tooltip("Should the player be penalized for departing in the wrong direction? *Ignored by Arrivals*")]
    public bool penalizeWrongDeparture = true;
    [Tooltip("How many timesteps the player has to start the task before it dissapears. *Ignored by Arrivals*")]
    public int deadline;


    public Task() {
        deadline = 0;
        value = 10;
        cargoType = CargoType.Cargo;
        taskType = TaskType.Arrival;
        fuel = 20;
        destination = Destination.Local;
        penalizeWrongDeparture = false;
    }


    public float departureModifier(Destination actualDepature) {
        switch (destination, actualDepature) {
            case (Destination.North, Destination.North):
            case (Destination.South, Destination.South):
            case (Destination.East, Destination.East):
            case (Destination.West, Destination.West):
                return 1.0f;
            case (Destination.North, Destination.East):
            case (Destination.North, Destination.West):
            case (Destination.South, Destination.East):
            case (Destination.South, Destination.West):
            case (Destination.East, Destination.North):
            case (Destination.East, Destination.South):
            case (Destination.West, Destination.North):
            case (Destination.West, Destination.South):
                return 0.5f;
            default:
                return 0.0f;
        }
    }

    public Task Copy() {
        Task task = ScriptableObject.CreateInstance<Task>();
        task.taskName = taskName;
        task.pilotBlurb = pilotBlurb;
        task.value = value;
        task.cargoType = cargoType;
        task.taskType = taskType;
        task.responsive = responsive;
        task.shipName = shipName;
        task.fuel = fuel;
        task.destination = destination;
        task.penalizeWrongDeparture = penalizeWrongDeparture;
        task.deadline = deadline;
        return task;
    }
}
