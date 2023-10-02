using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HangarBehaivor : MonoBehaviour
{

    List<VehicleBehavior> vehicles = new List<VehicleBehavior>();

    public void AddVehicle(VehicleBehavior vehicle) {
        vehicles.Add(vehicle);
    }

    public void RemoveVehicle(VehicleBehavior vehicle) {
        vehicles.Remove(vehicle);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
