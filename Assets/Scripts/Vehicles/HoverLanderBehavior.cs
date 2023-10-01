using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoverLanderBehavior : VehicleBehavior {

    protected override IEnumerator SimulateCommandCoroutine(float secondsPerStep, Command command) {
        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;
        float starttime = Time.time;
        Vector3 targetPosition = transform.position;
        Quaternion targetRotation = transform.rotation;
        switch (command) {
            case Command.Idle:
                break;
            case Command.Forward:
                targetPosition += transform.forward * speed;
                break;
            case Command.YawLeft:
                targetRotation *= Quaternion.Euler(0, -90, 0);
                break;
            case Command.YawRight:
                targetRotation *= Quaternion.Euler(0, 90, 0);
                break;
            case Command.Raise:
                targetPosition += transform.up;
                if (flightState == FlightState.Launching) { 
                    targetPosition.y = Mathf.Round(targetPosition.y);
                }
                break;
            case Command.Lower:
                targetPosition -= transform.up;
                break;
            case Command.Boost:
                targetPosition += transform.forward * boostSpeed;
                break;
            default:
                Debug.LogWarningFormat("HoverLanders can't take command: {0}", command);
                break;
        }

        //TODO this should be more than a straight interpolation of position, it should arc around a circle instead
        while (Time.time - starttime < secondsPerStep) {
            rb.MovePosition(Vector3.Lerp(startPos, targetPosition, (Time.time - starttime) / secondsPerStep));
            rb.MoveRotation(Quaternion.Lerp(startRot, targetRotation, (Time.time - starttime) / secondsPerStep));
            yield return new WaitForEndOfFrame();
        }
        transform.position = targetPosition;
        transform.rotation = targetRotation;
        currentFuel -= 1;
        if (currentFuel <= 0) {
            Crash();
        }
        yield break;
    }

    protected override void OnTriggerEnter(Collider other) {
        base.OnTriggerEnter(other);
        if(other.gameObject.CompareTag("LandingPad") && currentTask.destination == Task.Destination.Local) {
            //TODO eventually we'll want to do something with the pad itself
            HoverPad pad = other.gameObject.GetComponent<HoverPad>();
            Land(pad);
        }
    }

    private static List<Command> FLIGHT_COMMANDS = new List<Command> {Command.Idle, Command.Forward, Command.Boost, Command.YawLeft, Command.YawRight, Command.Raise, Command.Lower};
    private static List<Command> GROUND_COMMANDS = new List<Command> { Command.Idle, Command.Unload };

    public override IEnumerable<Command> GetAvailableCommands() {
        switch (flightState) {
            case FlightState.Flying:
                return FLIGHT_COMMANDS.AsReadOnly();
            case FlightState.Grounded:
                return GROUND_COMMANDS.AsReadOnly();
            case FlightState.Launching:
            default:
                return new List<Command>();
        }
    }
}
