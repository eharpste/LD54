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
        throw new System.NotImplementedException();
    }

	public override List<Task> GetTaskList()
	{ return null; }

	public override void LaunchNextAvailableVehicle(Task task = null) {
        foreach(Runway runway in Runways) {
            if (runway.AvailableToLaunch) {
                runway.LaunchNextAvailableVehicle(task);
                return;
            }
        }
    }
}
