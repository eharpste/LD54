using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { private set; get; }

    List<VehicleBehavior> Vehicle = new List<VehicleBehavior>();

    public float secondsPerStep = 1f;

    public void AddVehicle(VehicleBehavior vehicle) {
        Vehicle.Add(vehicle);
    }

    private void Awake() {
        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space)) {
            SimulateStep();
        }
    }

    public void SimulateStep() {
        foreach (VehicleBehavior vehicle in Vehicle) {
            vehicle.SimulateNextCommand(secondsPerStep);
        }
    }
    
}
