using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CommandListElement : MonoBehaviour {
    public Image commandImage;
    public Text commandText;
    public Button removeButton;
    public Button swapUp;
    public Button swapDown;

    public void SetCommand(int idx, string commandName, Sprite commandSprite, bool editable) {
        commandImage.sprite = commandSprite;
        commandText.text = commandName;
        removeButton.interactable = editable;
        swapUp. interactable = editable;
        swapDown.interactable = editable;

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

}
