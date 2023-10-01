using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class zoomCam : MonoBehaviour
{
    CinemachineVirtualCamera cam;

	// Start is called before the first frame update
	void Start()
    {
        cam = GetComponent<CinemachineVirtualCamera>();
    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager.Instance.selectedVehicle != null)
        {
            cam.LookAt = GameManager.Instance.selectedVehicle.transform;

		}
    }
}
