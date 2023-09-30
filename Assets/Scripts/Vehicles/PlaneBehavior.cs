using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static VehicleBehavior;

public class PlaneBehavior : VehicleBehavior {

    override protected IEnumerator SimulateCommandCoroutine(float stepTime, Command command) {
        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;
        float starttime = Time.time;
        Vector3 targetPosition = transform.position;
        Quaternion targetRotation= transform.rotation;
        switch (command) {
            case Command.Idle:
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
            case Command.YawLeft:
            case Command.YawRight:
                Debug.LogWarning("Vehicles can't yaw");
                break;
            case Command.Climb:
                targetPosition += (transform.forward + transform.up);
                break;
            case Command.Dive:
                targetPosition += (transform.forward - transform.up);
                break;
            case Command.Boost:
                targetPosition += transform.forward * boostSpeed;
                break;
            default:
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
        fuel -= 1;
        if(fuel <= 0) {
            Crash();
        }
        yield break;
    }

    protected override void OnTriggerEnter(Collider other) {
        base.OnTriggerEnter(other);
        if (other.gameObject.CompareTag("Runway") && destination == Destination.Local) {
            Runway runway = other.gameObject.GetComponent<Runway>();
            if (runway.heading == (int)transform.rotation.eulerAngles.y) {
                Debug.Log("Landed on Runway");
                //TODO this might be agressive if the colliders hit before the plane is actually on the ground
                Land();
            }
            else {
                Debug.LogFormat("{0} hit Runway at {1} but expected {2}", gameObject.name, transform.rotation.eulerAngles.y, runway.heading);
                Crash();
            }
        }
    }
}
