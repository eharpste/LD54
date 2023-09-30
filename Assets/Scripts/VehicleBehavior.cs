using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class VehicleBehavior : MonoBehaviour {

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

    //TODO we don't actually want this to be a queue, we want it to be a list that we can loop through
    public List<Command> CommandList = new List<Command>();
    protected int currentCommand = 0;
    public int commandLimit = 3;
    public int fuel = 20;
    public int speed = 2;
    public int boostSpeed = 4;

    // Start is called before the first frame update
    protected void Start() {
        GameManager.Instance.AddVehicle(this);
        
    }

    // Update is called once per frame
    protected void Update() {
        
    }

    public void AddCommand(Command command) {
        if (CommandList.Count < commandLimit) {
            CommandList.Add(command);
        }
    }

    public abstract void SimulateNextCommand(float secondsPerStep);
}
