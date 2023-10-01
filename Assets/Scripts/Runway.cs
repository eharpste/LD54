using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Runway : Landing
{

    public List<Vector3> TaxiPoints = new List<Vector3>();
    public Vector3 launchPoint;
    public int launchHeading;

    [Tooltip("What angle should a plane be facing to land here? In terms of rotation around y.")]
    public int heading;

    public override void LandVehicle(VehicleBehavior vehicle) {
        vehicles.Add(vehicle);
        StartCoroutine(TaxiIn(vehicle));
        //animate the vehicle along the taxi points
    }


    IEnumerator TaxiIn(VehicleBehavior vehicle) {
        for (int step = 0; step < TaxiPoints.Count; step++) {
            Vector3 initialPos = vehicle.transform.position;
            float stepStartTime = Time.time;
            float stepDuration = step < 1 ? 1 : 2;
            while(Time.time - stepStartTime < stepDuration) {
                vehicle.transform.position = Vector3.Lerp(initialPos, TaxiPoints[step], (Time.time - stepStartTime) / stepDuration);
                if (step > 1) {
                    vehicle.transform.LookAt(TaxiPoints[step]);
                }
                yield return null;
            }
            vehicle.transform.position = TaxiPoints[step];
        }
    }

    public override void LaunchVehicle(VehicleBehavior vehicle) {
        vehicle.transform.position = launchPoint;
        vehicle.transform.rotation = Quaternion.Euler(0, launchHeading, 0);
        vehicles.Remove(vehicle);
        vehicle.currentFuel = vehicle.maxFuel;
        vehicle.CommandList.Clear();
        vehicle.CommandList.Add(VehicleBehavior.Command.Boost);
        vehicle.CommandList.Add(VehicleBehavior.Command.Climb);
        vehicle.CommandList.Add(VehicleBehavior.Command.Climb);
        vehicle.defaultCommand = VehicleBehavior.Command.Forward;
        vehicle.commandLoopStyle = VehicleBehavior.CommandLoopStyle.Default;
    }

}
