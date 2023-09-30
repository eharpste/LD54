using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RocketBehavior : VehicleBehavior {

    override protected IEnumerator SimulateCommandCoroutine(float stepTime, Command command) {
        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;
        Vector3 targetPosition = transform.position + transform.up * speed;
        float starttime = Time.time;
        while (Time.time - starttime < stepTime) {
            rb.MovePosition(Vector3.Lerp(startPos, targetPosition, (Time.time - starttime) / stepTime));
            yield return new WaitForEndOfFrame();
        }
        transform.position = targetPosition;
        fuel -= 1;
        yield break;
    }
}