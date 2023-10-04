using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HangarBehaivor : Landing {

    public List<Runway> Runways = new List<Runway>();

    public override bool AvailableToLaunch { 
        get {
            foreach(Runway runway in Runways) {
                if (runway.AvailableToLaunch) {
                    return true;
                }
            }
            return false;
        }
    }

    public void AddVehicle(VehicleBehavior vehicle) {
        this.vehicles.Add(vehicle);
        vehicle.flightState = VehicleBehavior.FlightState.Grounded;
        vehicle.transform.position = Runways[0].LaunchPath[0];
    }

    public override void LandVehicle(VehicleBehavior vehicle) {
        return;
    }

    public override void LaunchVehicle(VehicleBehavior vehicle, Task task = null) {
        Debug.Log("Launch Vehicle in Hanager");
        foreach (Runway runway in Runways) {
            if (runway.AvailableToLaunch) {
                runway.LaunchVehicle(vehicle, task);
                return;
            }
        }
    }

    public override List<Task> GetTaskList() {
       return GameManager.Instance.GetPendingDepatures(Task.CargoType.Passenger);
    }

	public override void LaunchNextAvailableVehicle(Task task = null) {
        if(vehicles.Count == 0) {
            return;
        }
        foreach(Runway runway in Runways) {
            if (runway.AvailableToLaunch) {
                VehicleBehavior vehicle = vehicles[0];
                vehicles.RemoveAt(0);
                runway.LaunchVehicle(vehicle, task);
                return;
            }
        }
    }
}
