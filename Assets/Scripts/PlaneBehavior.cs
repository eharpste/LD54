using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaneBehavior : MonoBehaviour
{

    public enum Command {
        Idle,
        Forward,
        BankLeft,
        BankRight,
        YawLeft,
        YawRight,
        Climb,
        Dive,
        Boost,
    }


    Queue<Command> CommandQuere  = new Queue<Command>();
    public int commandLimit = 3;
    public int fuel = 20;
    public int speed = 2;
    public int boostSpeed = 4;

    public void AddCommand(Command command) {
        if (CommandQuere.Count < commandLimit) {
            CommandQuere.Enqueue(command);
        }
    }

    public void SimulateNextCommand(float stepTime) {
        if (CommandQuere.Count > 0) {
            Vector3 targetPosition = transform.position;
            Quaternion targetOrientation = transform.rotation;
            Command command = CommandQuere.Dequeue();
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
                    Debug.LogError("Planes can't yaw");
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
        yield break;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
