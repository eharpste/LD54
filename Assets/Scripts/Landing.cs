using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public abstract class Landing : MonoBehaviour {

    protected void Start() {
        GameManager.Instance.AddLanding(this);
        Ready = true;
    }

    public virtual bool Ready { protected set; get; }

    protected List<VehicleBehavior> vehicles = new List<VehicleBehavior>();

    public abstract void LandVehicle(VehicleBehavior vehicle);

    public abstract void LaunchVehicle(VehicleBehavior vehicle);
}
