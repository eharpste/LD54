using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonCommand : MonoBehaviour
{
    public VehicleBehavior.Command command;
    public GuiManager guiManager;

    public void SendCommandToGuiManager()
    {
        guiManager.SendCommandToSelectedVehicle(command);

	}
    
}
