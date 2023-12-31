using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static VehicleBehavior;

public class PlaneBehavior : VehicleBehavior {

    override protected IEnumerator SimulateCommandCoroutine(float stepTime, Command command) {
        if (flightState == FlightState.Grounded) yield break;
        Ready = false;
        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;
        float starttime = Time.time;
        Vector3 targetPosition = transform.position;
        Quaternion targetRotation= transform.rotation;
        switch (command) {
            case Command.Idle:
                if(flightState == FlightState.Flying) {
                    Debug.LogError("Planes can't idle while flying");
                }
                break;
            case Command.Forward:
                targetPosition += transform.forward * speed;
                break;
            case Command.BankLeft:
                targetPosition += (transform.forward - transform.right);
                targetRotation *= Quaternion.Euler(0, -90, 0);
                break;
            case Command.BankRight:
                targetPosition += (transform.forward + transform.right);
                targetRotation *= Quaternion.Euler(0, 90, 0);
                break;
            case Command.Climb:
                targetPosition += (transform.forward + transform.up);
                if (flightState == FlightState.Launching) {
                    targetPosition.y = Mathf.Round(targetPosition.y);
                }
                break;
            case Command.Dive:
                targetPosition += (transform.forward - transform.up);
                break;
            case Command.Boost:
                targetPosition += transform.forward * boostSpeed;
                break;
            default:
                Debug.LogWarningFormat("Planes can't take command: {0}", command);
                break;
        }

        //TODO this should be more than a straight interpolation of position, it should arc around a circle instead
        while (Time.time - starttime < stepTime) {
            rb.MovePosition(Vector3.Lerp(startPos, targetPosition, (Time.time - starttime) / stepTime));
            rb.MoveRotation(Quaternion.Lerp(startRot, targetRotation, (Time.time - starttime) / stepTime));
            //transform.position = Vector3.Lerp(startPos, targetPosition, (Time.time - starttime) / stepTime);
            //transform.rotation = Quaternion.Lerp(startRot, targetRotation, (Time.time - starttime) / stepTime);
            yield return new WaitForEndOfFrame();
        }
        transform.position = targetPosition;
        transform.rotation = targetRotation;
        currentFuel -= 1;
        if(currentFuel <= 0) {
            Crash();
        }
        Ready = true;
        yield break;
    }

    protected override void OnTriggerEnter(Collider other) {
        base.OnTriggerEnter(other);

        if (flightState != FlightState.Flying) return; //leave here, or we'll crash on takeoff

        if (other.gameObject.CompareTag("Runway") && CurrentTask.destination == Task.Destination.Local) {
            Runway runway = other.gameObject.GetComponent<Runway>();
            if (runway.landingHeading == (int)transform.rotation.eulerAngles.y) {
                Debug.Log("Landed on Runway");
                //TODO this might be agressive if the colliders hit before the plane is actually on the ground
                Land(runway);
            }
            else {
                Debug.LogFormat("{0} hit Runway at {1} but expected {2}", gameObject.name, transform.rotation.eulerAngles.y, runway.landingHeading);
                Crash();
            }
        }
    }


    private static List<Command> FLYING_COMMANDS = new List<Command> { Command.Forward, Command.Boost, Command.BankLeft, Command.BankRight, Command.Dive, Command.Climb };
    private static List<Command> GROUND_COMMANDs = new List<Command> { Command.Unload };

    public override IEnumerable<Command> GetAvailableCommands() {
        switch(flightState) {
            case FlightState.Flying:
                return FLYING_COMMANDS.AsReadOnly();
            case FlightState.Grounded:
                return GROUND_COMMANDs.AsReadOnly();
            case FlightState.Launching:
            default:
                return new List<Command>();
        }
    }
}
