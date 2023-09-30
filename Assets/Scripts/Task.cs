using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Task : ScriptableObject {
    [Tooltip("How many timesteps the player has to start the task before it dissapears")]
    public int deadline;
}

[CreateAssetMenu(fileName = "New Arrival", menuName = "Tasks/Arrival")]
public class Arrival : Task {
    public Vector3Int appearanceLocation;
    public GameObject vehiclePrefab;
}

[CreateAssetMenu(fileName = "New Departure", menuName = "Tasks/Departure")]
public class Departure: Task {
    public enum DepartureType { Cargo, Passenger };
    public DepartureType departureType;
    public VehicleBehavior.Destination destination;
}

[CreateAssetMenu(fileName = "New Flyby", menuName = "Tasks/Flyby")]
public class Flyby:Task {
    public Vector3Int appearanceLocation;
    public GameObject vehiclePrefab;
    public VehicleBehavior.Destination destination;
}
