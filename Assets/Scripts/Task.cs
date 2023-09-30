using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Task", menuName = "ScriptableObjects/Task", order = 1)]
public class Task : ScriptableObject {
    [Tooltip("How many timesteps the player has to start the task before it dissapears")]
    public int deadline;

    [Tooltip("How much is this task worth?")]
    public int value;

    public enum CargoType { Cargo, Passenger, Rocket };
    [Tooltip("What kind of cargo are we carrying? Implicitly, what kind of vehicle is it?")]
    public CargoType cargoType;

    public enum TaskType { Arrival, Departure, Flyby };
    public TaskType taskType;

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

    public int fuel;

    [Header("Used by Departure Tasks")]
    public Destination destination;
    public bool penalizeWrongDeparture = true;

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
}
