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

    public override void SimulateStep(float stepTime) {
        AvailableToLaunch = true;
    }


    IEnumerator TaxiIn(VehicleBehavior vehicle) {
        Ready = false;
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
        Ready = true;
    }

    public override void LaunchVehicle(VehicleBehavior vehicle) {
        if(!vehicles.Contains(vehicle)) {
            Debug.LogError("Vehicle not on runway, cannot launch");
            return;
        }
        //TODO animate the vehicle along the launch points
        vehicle.transform.position = LaunchPath[0];
        vehicles.Remove(vehicle);
        StartCoroutine(TaxiOut(vehicle));
    }

    IEnumerator TaxiOut (VehicleBehavior vehicle) {
        Ready = false;
        AvailableToLaunch = false;
        float stepStartTime;
        vehicle.transform.position = TaxiPath[0];
        for(int step = 0; step < LaunchPath.Count; step++) {
            Vector3 initialPos = vehicle.transform.position;
            stepStartTime= Time.time;
            while (Time.time - stepStartTime < 1) {
                vehicle.transform.position = Vector3.Lerp(initialPos, LaunchPath[step], (Time.time - stepStartTime));
                if (step > 1) {
                    vehicle.transform.LookAt(LaunchPath[step]);
                }
                yield return null;
            }
            vehicle.transform.position = LaunchPath[step];
        }
        stepStartTime = Time.time;
        Quaternion initialRot = vehicle.transform.rotation;
        while(Time.time - stepStartTime < .75f) {
            vehicle.transform.rotation = Quaternion.Lerp(initialRot, Quaternion.Euler(0, launchHeading, 0), (Time.time - stepStartTime) / .75f);
        }
        vehicle.transform.rotation = Quaternion.Euler(0, launchHeading, 0);
        vehicle.currentFuel = vehicle.maxFuel;
        vehicle.SetCommands(new List<VehicleBehavior.Command>() { VehicleBehavior.Command.Boost, VehicleBehavior.Command.Climb, VehicleBehavior.Command.Climb });
        vehicle.flightState = VehicleBehavior.FlightState.Launching;
        Ready = true;
    }

}
