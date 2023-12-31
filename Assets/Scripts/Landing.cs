using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public abstract class Landing : MonoBehaviour {

    protected virtual void Start() {
        GameManager.Instance.AddLanding(this);
        Ready = true;
    }

    public virtual void SimulateStep(float stepTime) { }

    public virtual bool Ready { protected set; get; }

    public virtual bool AvailableToLaunch { protected set; get; } = true;

    public List<VehicleBehavior> vehicles = new List<VehicleBehavior>();

    public abstract void LandVehicle(VehicleBehavior vehicle);

    public abstract void LaunchVehicle(VehicleBehavior vehicle, Task task=null);

    public abstract List<Task> GetTaskList();

	public virtual void LaunchNextAvailableVehicle(Task task = null) {
        if(vehicles.Count == 0) {
            Debug.LogWarningFormat("{0} trying to launch with no vehicles", gameObject.name);
            return;
        }


		LaunchVehicle(vehicles[0], task);
	}
}
