using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonSwap : MonoBehaviour
{
    public enum Direction { Up, Down };
    public int index;
    public Direction direction;

    public GuiManager guiManager;

    public void SwapAction() {
        if (direction == Direction.Up) {
            guiManager.SwapCommands(index, index +1);
        } else {
            guiManager.SwapCommands(index, index-1);
        }
    }
    
}
