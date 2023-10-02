using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Task", menuName = "ScriptableObjects/Task", order = 1)]
public class Task : ScriptableObject {

    [Tooltip("The Display name of the task.")]
    public string taskName;



    [Tooltip("A short narrative message about the task framed as a message from the pilot.")]
    [TextArea(3,8)]
    public string pilotBlurb;

    [Tooltip("How much is this task worth?")]
    public int value;

    public enum CargoType { Cargo, Passenger, Rocket };
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
}
