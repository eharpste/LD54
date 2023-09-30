using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RocketBehavior : VehicleBehavior {
    override public void SimulateNextCommand(float stepTime) {
        if (CommandList.Count > 0) {
            Vector3 targetPosition = transform.position;
            Quaternion targetOrientation = transform.rotation;
            currentCommand ++;
            currentCommand %= CommandList.Count;
            Command command = CommandList[currentCommand];
            switch (command) {
                case Command.Idle:
                case Command.Forward: 
                case Command.BankLeft:
                case Command.BankRight:
                case Command.YawLeft:
                case Command.YawRight:
                case Command.Climb:
                case Command.Dive:
                case Command.Boost:
                default:
                    targetPosition += transform.up * speed;
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
