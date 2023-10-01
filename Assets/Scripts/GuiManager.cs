using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;



public class GuiManager : MonoBehaviour
{

    public Text TimeLabel;
	public Text ScoreLabel;
	public Text VehicleName;
	public RectTransform lineLocator;

    public Menu ManeuverGui;

	public Menu InspectorGui;

	public LineRenderer lineRenderer;


	// Start is called before the first frame update
	void Start()
    {
		//lineRenderer = GetComponent<LineRenderer>();
	}

    // Update is called once per frame
    void Update()
    {
        TimeLabel.text = GameManager.Instance.timeCounter.ToString();
		ScoreLabel.text = GameManager.Instance.score.ToString();

		if (GameManager.Instance.selectedVehicle != null)
		{
			InspectorGui.EnableAll();
			VehicleName.text = GameManager.Instance.selectedVehicle.gameObject.name;
			updateSelectionLine();
		}
		else
		{
			InspectorGui.DisableAll();
		}

		//if (gameManager.isShipSelected && !ManeuverGui.is_Enabled())
		//      {
		//          ManeuverGui.EnableAll();
		//      }

		//if (!gameManager.isShipSelected && ManeuverGui.is_Enabled())
		//{
		//	ManeuverGui.DisableAll();
		//}
	}

	void updateSelectionLine()
	{
		RectTransform guiElement = lineLocator;
		Transform targetTransform = GameManager.Instance.selectedVehicle.gameObject.transform;
		Vector3[] corners = new Vector3[4];
		guiElement.GetWorldCorners(corners);
		Vector3 frontPos = new Vector3(guiElement.position.x, guiElement.position.y, Camera.main.nearClipPlane);

		//Vector3 pos = Camera.main.ViewportToWorldPoint(frontPos);
		Vector3 pos = Camera.main.ScreenToWorldPoint(frontPos);
		//objectB.position = Camera.main.WorldToViewportPoint(pos);

		lineRenderer.SetPosition(0, pos);
		lineRenderer.SetPosition(1, targetTransform.position);
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
