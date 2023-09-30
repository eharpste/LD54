using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaneBehavior : VehicleBehavior {

    override public void SimulateNextCommand(float stepTime) {
        if (CommandList.Count > 0) {
            Vector3 targetPosition = transform.position;
            Quaternion targetOrientation = transform.rotation;
            currentCommand++;
            currentCommand %= CommandList.Count;
            Command command = CommandList[currentCommand];
            switch (command) {
                case Command.Idle:
                case Command.Forward:
                    targetPosition += transform.forward * speed;
                    break;
                case Command.BankLeft:
                    targetPosition += (transform.forward - transform.right);
                    targetOrientation = Quaternion.Euler(0, -90, 0);
                    break;
                case Command.BankRight:
                    targetPosition += (transform.forward + transform.right);
                    targetOrientation = Quaternion.Euler(0, 90, 0);
                    break;
                case Command.YawLeft:
                case Command.YawRight:
                    Debug.LogError("Vehicle can't yaw");
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
            StartCoroutine(SimulateCommand(stepTime, targetPosition, targetOrientation));
        }
    }

    IEnumerator SimulateCommand(float stepTime, Vector3 targetPos, Quaternion targetRot) {
        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;
        float starttime = Time.time;
        while (Time.time - starttime < stepTime) {
            transform.position = Vector3.Lerp(startPos, targetPos, (Time.time - starttime) / stepTime);
            transform.rotation = Quaternion.Lerp(startRot, targetRot, (Time.time - starttime) / stepTime);
            yield return new WaitForEndOfFrame();
        }
        transform.position = targetPos;
        transform.rotation = targetRot;
        fuel -= 1;
        yield break;
    }
}
