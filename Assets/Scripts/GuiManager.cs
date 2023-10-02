using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;



public class GuiManager : MonoBehaviour
{
	[System.Serializable]
	public class SpriteEntry {
		[SerializeField]
		public VehicleBehavior.Command command;
		[SerializeField]
        public Sprite sprite;
	}

	public List<SpriteEntry> spriteAtlas = new List<SpriteEntry>();
	private Dictionary<VehicleBehavior.Command, Sprite> spriteMap = new Dictionary<VehicleBehavior.Command, Sprite>();


    public Text TimeLabel;
	public Text ScoreLabel;
	public Text VehicleName;
	public RectTransform lineLocator;

    public Menu ManeuverGui;

	public Menu InspectorGui;
	public Text InspectorTaskFuel;
	public Text InspectorTaskValue;
	public Text InspectorTaskDescription;

	public LineRenderer lineRenderer;

	List<ButtonCommand>	commandButtons = new List<ButtonCommand>();

	List<CommandListElement> commandListElements = new List<CommandListElement>();

	public delegate void SelectVehicle();
	public static event SelectVehicle OnSelectVehicle = delegate { };

	public static void OnSelectVehicleEvent()
	{
		if (OnSelectVehicle != null)
		{
			Debug.Log("starting dialogue delegate");
			OnSelectVehicle();
		}
	}

	// Start is called before the first frame update
	void Start()
    {
		foreach(SpriteEntry entry in spriteAtlas) {
			spriteMap.Add(entry.command, entry.sprite);
		}

		GetComponentsInChildren(true, commandListElements);
        //lineRenderer = GetComponent<LineRenderer>();
        GetComponentsInChildren(true, commandButtons);
        foreach (ButtonCommand buttonCommand in commandButtons) {
			buttonCommand.guiManager = this;
		}

		foreach(ButtonRemove button in GetComponentsInChildren<ButtonRemove>()) {
            button.guiManager = this;
        }

        foreach (ButtonSwap button in GetComponentsInChildren<ButtonSwap>()) {
            button.guiManager = this;
        }

		
    }

	private void OnEnable()
	{
		Events.SelectVehicle += ShowMovementGui;
	}
	private void OnDisable()
	{
		Events.SelectVehicle -= ShowMovementGui;
	}


	// Update is called once per frame
	void Update()
    {
        TimeLabel.text = GameManager.Instance.CurrentTime.ToString();
		ScoreLabel.text = GameManager.Instance.score.ToString();

		if (GameManager.Instance.selectedVehicle != null)
		{
			InspectorGui.EnableAll();
			VehicleName.text = GameManager.Instance.selectedVehicle.currentTask.shipName;
			InspectorTaskFuel.text = GameManager.Instance.selectedVehicle.currentFuel.ToString();
			InspectorTaskValue.text = GameManager.Instance.selectedVehicle.currentTask.value.ToString();
			InspectorTaskDescription.text = GameManager.Instance.selectedVehicle.currentTask.pilotBlurb;
			lineRenderer.enabled = true;
			updateSelectionLine();

		}
		else
		{
			lineRenderer.enabled = false;
			InspectorGui.DisableAll();
			HideMovementGui();
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

		foreach (ButtonCommand buttonCommand in commandButtons)
		{
			buttonCommand.gameObject.SetActive(false);
		}
		foreach (VehicleBehavior.Command command in GameManager.Instance.selectedVehicle.GetAvailableCommands())
		{
			foreach (ButtonCommand buttonCommand in commandButtons)
			{
				if (buttonCommand.command == command)
				{
					buttonCommand.gameObject.SetActive(true);
				}
			}
		}
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

	public void SendSimulateCommandToGameManager()
	{
		GameManager.Instance.SimulateStep();
	}

	public void RemoveSelectedCommand(int index) {
		if(GameManager.Instance.selectedVehicle != null) {
            VehicleBehavior vehicle = GameManager.Instance.selectedVehicle;
            vehicle.RemoveCommand(index);
        }
		
	}

    public void SwapCommands(int index, int v) {
		if (GameManager.Instance.selectedVehicle != null) {
			GameManager.Instance.selectedVehicle.SwapCommands(index, v);
		}
    }

	public void UpdateCommandList() {
		if(GameManager.Instance.selectedVehicle != null) {
			for(int i = 0; i < GameManager.Instance.selectedVehicle.CurrentCommandList.Count; i++) {
				VehicleBehavior.Command command = GameManager.Instance.selectedVehicle.CurrentCommandList[i];

				bool editable = GameManager.Instance.selectedVehicle.commandExecutionState == VehicleBehavior.CommandExecutionState.Editing || GameManager.Instance.selectedVehicle.commandExecutionState == VehicleBehavior.CommandExecutionState.Defaulting;
				commandListElements[i].SetCommand(i, command.ToString(), spriteMap[command], editable);

			}
		}
	}

}
