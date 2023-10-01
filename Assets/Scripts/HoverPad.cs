using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoverPad : Landing {
    public Vector3 landingPosition;
    public int landingHeading;
    public int launchHeading;

    public override void LandVehicle(VehicleBehavior vehicle) {
        vehicles.Add(vehicle);
        vehicle.CommandList.Clear();
        vehicle.CommandList.Add(VehicleBehavior.Command.Unload);
        StartCoroutine(SettleLander(vehicle));
    }

    IEnumerator SettleLander(VehicleBehavior vehicle) {
        Vector3 initialPosition = vehicle.transform.position;
        Quaternion initialRotation = vehicle.transform.rotation;
        Quaternion targetRotation = Quaternion.Euler(0, landingHeading, 0);
        float startTime = Time.time;
        float duration = 1f;
        while(Time.time - startTime < duration) {
            vehicle.transform.transform.position = Vector3.Lerp(initialPosition, landingPosition, Time.time-startTime / duration);
            vehicle.transform.rotation = Quaternion.Lerp(initialRotation, targetRotation, Time.time - startTime / duration);
            yield return null;
        }
        vehicle.transform.position = landingPosition;
        vehicle.transform.rotation = targetRotation;
    }


    public override void LaunchVehicle(VehicleBehavior vehicle) {
        vehicles.Remove(vehicle);
        vehicle.currentFuel = vehicle.maxFuel;
        vehicle.transform.rotation = Quaternion.identity;
        vehicle.CommandList.Clear();
        vehicle.CommandList.Add(VehicleBehavior.Command.Climb);
        vehicle.CommandList.Add(VehicleBehavior.Command.Idle);
        vehicle.defaultCommand = VehicleBehavior.Command.Idle;
        vehicle.commandLoopStyle = VehicleBehavior.CommandLoopStyle.Default;
    }
}
