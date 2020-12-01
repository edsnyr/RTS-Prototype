using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerControlSwitch : MonoBehaviour
{

    /*
     * This class switches control between different players for testing purposes.
     */

    public Button switchButton;
    public TextMeshProUGUI buttonText;
    
    ColorBlock cb;
    static PlayerController[] players;
    static int id = 0;

    private void Awake() {
        players = FindObjectsOfType<PlayerController>(); //find all players in scene
        cb = switchButton.colors; //copy of the buttons color settings
        UpdateButton();
        switchButton.onClick.AddListener(() => id = (id + 1) % players.Length); //increment the player controller list when the button is pressed
        switchButton.onClick.AddListener(() => UpdateButton());
    }

    /// <summary>
    /// Displays the color of the player being controlled and their team id.
    /// </summary>
    private void UpdateButton() {
        buttonText.text = "Player " + players[id].team;
        cb.normalColor = players[id].teamColor;
        switchButton.colors = cb;
    }

    /// <summary>
    /// Returns the id of the player currently being controlled.
    /// </summary>
    /// <returns></returns>
    public static int GetPlayerID() {
        return players[id].team;
    }
}
