using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class CommandListElement : MonoBehaviour {
    public Image commandImage;
    public Text commandText;
    public Button removeButton;
    public Button swapUp;
    public Button swapDown;

    public int command_id;

    public void SetCommand(int idx, string commandName, Sprite commandSprite, bool editable) {
        commandImage.sprite = commandSprite;
        commandText.text = commandName;
        removeButton.interactable = editable;
        swapUp.interactable = editable;
        swapDown.interactable = editable;
        command_id = idx;


		if (idx == 0) {
            swapUp.gameObject.SetActive(false);
        }
        else {
            swapUp.gameObject.SetActive(true);
        }
        if (idx == 3) {
            swapDown.gameObject.SetActive(false);
        }
        else {
            swapDown.gameObject.SetActive(true);
        }
    }

    public void RemoveCommand()
    {
        GameManager.Instance.selectedVehicle.RemoveCommand(command_id);
        Events.UpdateVehicleEvent();
	}

    public void Raise()
    {
        GameManager.Instance.selectedVehicle.SwapCommands(command_id, command_id - 1);
        Events.UpdateVehicleEvent();
	}

	public void Lower()
	{
        if (GameManager.Instance.selectedVehicle.CommandQueue.Count < 4) return;

		GameManager.Instance.selectedVehicle.SwapCommands(command_id, command_id + 1);
		Events.UpdateVehicleEvent();
	}

	[ContextMenu("Hide")]
    public void Hide()
    {
        CanvasGroup cg = GetComponent<CanvasGroup>();
        cg.interactable = false;
        cg.blocksRaycasts = false;
        cg.alpha = 0;
    }

	[ContextMenu("Show")]
	public void Show()
	{
		CanvasGroup cg = GetComponent<CanvasGroup>();
		cg.interactable = true;
		cg.blocksRaycasts = true;
		cg.alpha = 1;
	}
}
