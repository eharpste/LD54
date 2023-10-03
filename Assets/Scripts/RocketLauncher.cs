using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class RocketLauncher : Landing  {


    private int startLaunchtime = -1;
    private RocketBehavior currentRocket;

    public Transform scaffolding;
    public Transform door;

    public override void LandVehicle(VehicleBehavior vehicle) {
        return;
    }

    public override void SimulateStep(float stepTime) {
        if (!AvailableToLaunch) {
            //Debug.LogFormat("{0} is going to stage {1}", gameObject.name, GameManager.Instance.CurrentTime - startLaunchtime);
            switch (GameManager.Instance.CurrentTime - startLaunchtime) {
                case 1:                    
                    StartCoroutine(Stage1(stepTime));
                    break;
                case 2:
                    StartCoroutine(Stage2(stepTime));
                    break;
                case 3:
                    StartCoroutine(Stage3(stepTime));
                    break;
                case 4:
                    //Stage 4 is for the rocket to actually launch
                    break;
                case 5:
                    StartCoroutine(Stage5(stepTime));
                    break;
                default:
                    break;
            }
        }
    }

    IEnumerator Stage1(float stepTime) {
        //lower and move the door to the side
        //raise the scffolding 1 step
        //spawn the rocket with just the nose poking out

        Ready= false;
        float startTime = Time.time;
        Vector3 doorStart = door.localPosition;
        Vector3 doorStep1 = doorStart - new Vector3(0, .7f, 0);
        Vector3 doorStep2 = doorStep1 + new Vector3(0, 0, 1.25f);
        Vector3 scaffoldingStart = scaffolding.localPosition;
        Vector2 scaffolingTarget = scaffoldingStart + Vector3.up * 1.8f;
        
        
        Vector3 rocketStart = transform.position - Vector3.up * 2;
        currentRocket.transform.position = rocketStart;
        Vector3 rocketTarget = transform.position - Vector3.up * 1.6f;
        //Debug.LogFormat("moving rocket from {0} to {1}", rocketStart, rocketTarget);
        
        while(Time.time - startTime < stepTime) {
            float t = (Time.time - startTime) / stepTime;
            scaffolding.localPosition = Vector3.Lerp(scaffoldingStart,scaffolingTarget, (Time.time - startTime) / stepTime);
            currentRocket.transform.position = Vector3.Lerp(rocketStart, rocketTarget, (Time.time - startTime) / stepTime);
            if (t < .3f) {
                door.localPosition = Vector3.Lerp(doorStart,doorStep1, t / .3f);
            }
            else {
                door.localPosition = Vector3.Lerp(doorStep1, doorStep2, (t - .3f) / .7f);
            }
            yield return new WaitForEndOfFrame();
        }

        scaffolding.localPosition = scaffolingTarget;
        currentRocket.transform.position = rocketTarget;

        Ready = true;
        yield break;
    }

    IEnumerator Stage2(float stepTime) {
        Ready = false;
        float startTime = Time.time;
        Vector3 rocketStart = currentRocket.transform.position;
        Vector3 rocketTarget = transform.position + Vector3.up * .2f;
        Vector3 scaffoldingStart = scaffolding.localPosition;
        Vector3 scaffoldingTarget = scaffoldingStart + Vector3.up * 1.8f;
        while (Time.time - startTime < stepTime) {
            currentRocket.transform.position = Vector3.Lerp(rocketStart, rocketTarget, (Time.time - startTime) / stepTime);
            scaffolding.localPosition = Vector3.Lerp(scaffoldingStart, scaffoldingTarget, (Time.time - startTime) / stepTime);

            //Debug.LogFormat("Stage2 step, time:{0}, delta:{1}",Time.time, startTime - Time.time);
            yield return new WaitForEndOfFrame();
        }
        currentRocket.transform.position = rocketTarget;
        scaffolding.localPosition = scaffoldingTarget;
        //Debug.LogFormat("{0} Reached end of stage 2", gameObject.name);
        Ready = true;
        yield break;
    }


    IEnumerator Stage3(float stepTime) {
        Ready = false;
        currentRocket.SetCommands(new List<VehicleBehavior.Command>() { VehicleBehavior.Command.Raise, VehicleBehavior.Command.Raise, VehicleBehavior.Command.Raise, VehicleBehavior.Command.Raise });
        currentRocket.commandEdditingState = VehicleBehavior.CommandEditingState.Unavailable;
        currentRocket.flightState = VehicleBehavior.FlightState.Launching;
        Ready = true;
        //Activate the particle system
        yield break;
    }

    IEnumerator Stage5 (float stepTime) {
        Ready = false;
        currentRocket = null;
        float startTime = Time.time;
        Vector3 scaffoldStart = scaffolding.localPosition;
        Vector3 scaffoldTarget = scaffoldStart - Vector3.up * 1.8f * 2;
        Vector3 doorStart = door.localPosition;
        Vector3 doorStep1 = door.localPosition - new Vector3 (0, 0, 1.25f);
        Vector3 doorStep2 = doorStep1 + new Vector3(0, .7f, 0);

        while (Time.time - startTime < stepTime) {
            float t = (Time.time - startTime) / stepTime;
            if (t < .7f) {
                door.localPosition = Vector3.Lerp(doorStart, doorStep1, t / .3f);
            }
            else {
                door.localPosition = Vector3.Lerp(doorStep1, doorStep2, (t - .3f) / .7f);
            }
            scaffolding.localPosition = Vector3.Lerp(scaffoldStart, scaffoldTarget, t);
            yield return new WaitForEndOfFrame();
        }
        AvailableToLaunch = true;
        Ready = true;
    }

    public override void LaunchVehicle(VehicleBehavior vehicle) {
        if(!vehicle.GetType().Equals(typeof(RocketBehavior))) {
            Debug.LogErrorFormat("Trying to launch something that isn't a rocket from a rocket launcher");
        }
        currentRocket = (RocketBehavior)vehicle;
        currentRocket.flightState = VehicleBehavior.FlightState.Grounded;
        AvailableToLaunch = false;
        startLaunchtime = GameManager.Instance.CurrentTime;
    }
}
