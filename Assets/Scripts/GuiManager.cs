using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class GuiManager : MonoBehaviour
{
    GameManager gameManager;

    public Text TimeLabel;
    public bool shipSelected = false;


    // Start is called before the first frame update
    void Start()
    {
        gameManager = GameManager.Instance;

	}

    // Update is called once per frame
    void Update()
    {
        TimeLabel.text = gameManager.timeCounter.ToString();
	}
}
