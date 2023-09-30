using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class GuiManager : MonoBehaviour
{

    public Text TimeLabel;

    public Menu ManeuverGui;


    // Start is called before the first frame update
    void Start()
    {

	}

    // Update is called once per frame
    void Update()
    {
        TimeLabel.text = GameManager.Instance.timeCounter.ToString();

		//if (gameManager.isShipSelected && !ManeuverGui.is_Enabled())
  //      {
  //          ManeuverGui.EnableAll();
  //      }

		//if (!gameManager.isShipSelected && ManeuverGui.is_Enabled())
		//{
		//	ManeuverGui.DisableAll();
		//}
	}

	[ContextMenu("Show Manuever Gui")]
	void ShowMovementGui()
    {
        ManeuverGui.EnableAll();
	}

    [ContextMenu("Hide Manuever Gui")]
	void HideMovementGui()
	{
		ManeuverGui.DisableAll();
	}

	public void SendCommandToSelectedVehicle(VehicleBehavior.Command newCommand)
	{
		VehicleBehavior vehicle = GameManager.Instance.selectedVehicle;

		if (vehicle == null)
		{
			Debug.LogWarning("No vehicle is selected by the gamemanager. Commands are unheeded ;_;");
			return;
		}

		vehicle.AddCommand(newCommand);

	}
}
