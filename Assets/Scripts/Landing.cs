using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public abstract class Landing : MonoBehaviour {


    protected List<VehicleBehavior> vehicles = new List<VehicleBehavior>();

    public abstract void LandVehicle(VehicleBehavior vehicle);

    public abstract void TakeOffVehicle(VehicleBehavior vehicle);
}
