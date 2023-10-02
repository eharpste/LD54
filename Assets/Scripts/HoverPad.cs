using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoverPad : Landing {
    public Vector3 landingPosition;
    public int landingHeading;
    public int launchHeading;

    private int lastLandingTime = -1;
    public  int unloadingTime = 1;

    override public bool AvailableToLaunch { 
        get {
            return vehicles.Count > 0 && GameManager.Instance.CurrentTime - lastLandingTime > unloadingTime;     
        } 
    }

    public override void LandVehicle(VehicleBehavior vehicle) {
        vehicles.Add(vehicle);
        vehicle.CurrentCommandList.Clear();
        vehicle.CurrentCommandList.Add(VehicleBehavior.Command.Unload);
        StartCoroutine(SettleLander(vehicle));
    }

    IEnumerator SettleLander(VehicleBehavior vehicle) {
        Ready = false;
        Vector3 initialPosition = vehicle.transform.position;
        Quaternion initialRotation = vehicle.transform.rotation;
        Quaternion targetRotation = Quaternion.Euler(0, landingHeading, 0);
        Vector3 globalLandingPosition = this.transform.TransformPoint(landingPosition);

		float startTime = Time.time;
        float duration = 1f;
        while(Time.time - startTime < duration) {
            vehicle.transform.transform.position = Vector3.Lerp(initialPosition, globalLandingPosition, Time.time-startTime / duration);
            vehicle.transform.rotation = Quaternion.Lerp(initialRotation, targetRotation, Time.time - startTime / duration);
            yield return null;
        }
        vehicle.transform.position = globalLandingPosition;
        vehicle.transform.rotation = targetRotation;
        Ready = true;
        lastLandingTime = GameManager.Instance.CurrentTime;
    }

    [ContextMenu("LaunchVehicle")]
    public void LaunchDockedVehicle()
    {
        LaunchVehicle(vehicles[0]);
	}

    public override void LaunchVehicle(VehicleBehavior vehicle) {
        vehicles.Remove(vehicle);
        vehicle.currentFuel = vehicle.maxFuel;
        StartCoroutine(LiftOffLander(vehicle));
    }

    IEnumerator LiftOffLander(VehicleBehavior vehicle) {
        Ready = false;
        Quaternion initialRotation = vehicle.transform.rotation;
        float startTime = Time.time;
        while(Time.time - startTime < 1) {
            vehicle.transform.rotation = Quaternion.Lerp(initialRotation, Quaternion.Euler(0, launchHeading, 0), (Time.time - startTime) / 1);
            yield return null;
        }
        vehicle.transform.rotation = Quaternion.identity;
        vehicle.SetCommands(new List<VehicleBehavior.Command>() { VehicleBehavior.Command.Raise, VehicleBehavior.Command.Forward });
        vehicle.flightState = VehicleBehavior.FlightState.Launching;

        Ready = true;
    }
}
