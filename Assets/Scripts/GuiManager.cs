using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
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
	public RectTransform lineLocator;

	[Header("Vehicles")]
	public Text VehicleName;
    public Menu ManeuverGui;
	public Menu InspectorGui;
	public Text InspectorTaskFuel;
	public Text InspectorTaskValue;
	public Text InspectorTaskDirection;
	public Text InspectorTaskDescription;

	[Header("Locations")]
	public Menu LocationGui;
	public Text LocationName;
	public RectTransform taskRect;
	public GameObject TaskButton;

	public LineRenderer lineRenderer;

	List<ButtonCommand>	commandButtons = new List<ButtonCommand>();
	List<CommandListElement> commandListElements = new List<CommandListElement>();


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
		Events.UpdateVehicle += UpdateCommandList;
		Events.SelectLocation += ShowLocationGui;
		Events.SelectLocation += updateListOfDepartures;
	}
	private void OnDisable()
	{
		Events.SelectVehicle -= ShowMovementGui;
		Events.UpdateVehicle -= UpdateCommandList;
		Events.SelectLocation -= ShowLocationGui;
		Events.SelectLocation -= updateListOfDepartures;
	}


	private string DirectionFromDestination(Task.Destination destination) {
		return destination switch {
			Task.Destination.North => "NR",
			Task.Destination.South => "ST",
			Task.Destination.East => "ET",
			Task.Destination.West => "WT",
			Task.Destination.Local => "LC",
			Task.Destination.Up => "UP"
		};
	}


	// Update is called once per frame
	void Update()
	{
		TimeLabel.text = GameManager.Instance.CurrentTime.ToString();
		ScoreLabel.text = GameManager.Instance.score.ToString();

		if (GameManager.Instance.selectedVehicle != null)
		{
			InspectorGui.EnableThis();
            VehicleName.text = GameManager.Instance.selectedVehicle.ShipName;
            InspectorTaskFuel.text = GameManager.Instance.selectedVehicle.currentFuel.ToString();
            if (GameManager.Instance.selectedVehicle.CurrentTask!=null)
			{
				InspectorTaskValue.text = GameManager.Instance.selectedVehicle.CurrentTask.value.ToString();
				InspectorTaskDirection.text = GameManager.Instance.selectedVehicle.CurrentTask.destination.ToString();
				InspectorTaskDescription.text = GameManager.Instance.selectedVehicle.CurrentTask.pilotBlurb;
			}
			lineRenderer.enabled = true;
			updateSelectionLine(GameManager.Instance.selectedVehicle.gameObject);

		}
		else
		{
			InspectorGui.DisableAll();
			HideMovementGui();
		}

		if (GameManager.Instance.selectedLocation != null)
		{
			LocationName.text = GameManager.Instance.selectedLocation.name.ToString();
			lineRenderer.enabled = true;
			updateSelectionLine(GameManager.Instance.selectedLocation.gameObject);
		}
		else
		{
			LocationGui.DisableThis();
		}

		if ((GameManager.Instance.selectedVehicle == null) && (GameManager.Instance.selectedLocation == null)) {
			lineRenderer.enabled = false;
		}
	}

	public void updateListOfDepartures()
	{
		for (int i=0; i<taskRect.childCount; i++)
		{
			GameObject.Destroy(taskRect.GetChild(i).gameObject);
		}
		foreach (Task task in GameManager.Instance.selectedLocation.GetTaskList()) {
			GameObject go = GameObject.Instantiate(TaskButton, taskRect);
			Text text = go.GetComponentInChildren<Text>();
			text.text = task.name;
		}
	}

	void updateSelectionLine(GameObject target)
	{
		RectTransform guiElement = lineLocator;
		Transform targetTransform = target.transform;
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

	[ContextMenu("Show Location Gui")]
	void ShowLocationGui()
	{
		LocationGui.EnableThis();
	}

	[ContextMenu("Hide Location Gui")]
	void HideLocationGui()
	{
		LocationGui.DisableThis();
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


	public void SendResetCommandToGameManager()
	{
		GameManager.Instance.ResetGame();
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

	[ContextMenu("Update Command List")]
	public void UpdateCommandList() {
		if(GameManager.Instance.selectedVehicle != null) {

			VehicleBehavior vehicle = GameManager.Instance.selectedVehicle;

			switch (vehicle.commandEdditingState) {
				case VehicleBehavior.CommandEditingState.Editable:
					break;
				case VehicleBehavior.CommandEditingState.Executing:
					break;
				case VehicleBehavior.CommandEditingState.Unavailable:
					break;
			}


			int commandCount = GameManager.Instance.selectedVehicle.CommandQueue.Count;

			for (int i = 0; i < commandListElements.Count; i++) {

				if (i>=commandCount)
				{
					commandListElements[i].Hide();
				}
				else
				{
					commandListElements[i].Show();
					VehicleBehavior.Command command = GameManager.Instance.selectedVehicle.CommandQueue[i];

					bool editable = GameManager.Instance.selectedVehicle.commandEdditingState == VehicleBehavior.CommandEditingState.Editable;
					commandListElements[i].SetCommand(i, command.ToString(), spriteMap[command], editable);
				}
			}
		}
	}



	public void LaunchNextAvailableVehicle()
	{
		GameManager.Instance.selectedLocation.LaunchNextAvailableVehicle();
	}

}
