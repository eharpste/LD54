using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Runway : Landing
{

    public List<Vector3> TaxiPath = new List<Vector3>();
    public List<Vector3> LaunchPath = new List<Vector3>();
    public int launchHeading;

    [Tooltip("What angle should a plane be facing to land here? In terms of rotation around y.")]
    public int heading;

    public override void LandVehicle(VehicleBehavior vehicle) {
        vehicles.Add(vehicle);
        StartCoroutine(TaxiIn(vehicle));
        //animate the vehicle along the taxi points
    }


    IEnumerator TaxiIn(VehicleBehavior vehicle) {
        for (int step = 0; step < TaxiPath.Count; step++) {
            Vector3 initialPos = vehicle.transform.position;
            float stepStartTime = Time.time;
            float stepDuration = step < 1 ? 1 : 2;
            while(Time.time - stepStartTime < stepDuration) {
                vehicle.transform.position = Vector3.Lerp(initialPos, TaxiPath[step], (Time.time - stepStartTime) / stepDuration);
                if (step > 1) {
                    vehicle.transform.LookAt(TaxiPath[step]);
                }
                yield return null;
            }
            vehicle.transform.position = TaxiPath[step];
        }
    }

    public override void LaunchVehicle(VehicleBehavior vehicle) {
        //TODO animate the vehicle along the launch points
        vehicle.transform.position = LaunchPath[0];
        vehicle.transform.rotation = Quaternion.Euler(0, launchHeading, 0);
        vehicles.Remove(vehicle);
        vehicle.currentFuel = vehicle.maxFuel;
        vehicle.SetCommands(new List<VehicleBehavior.Command>() { VehicleBehavior.Command.Boost, VehicleBehavior.Command.Climb, VehicleBehavior.Command.Climb });
        vehicle.flightState = VehicleBehavior.FlightState.Launching;
    }

}
