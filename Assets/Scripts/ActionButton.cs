using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

public class ActionButton : MonoBehaviour
{
    public TextMeshProUGUI buttonText;
    public TextMeshProUGUI hotkeyText;
    public KeyCode hotkey;
    public Command thisCommand;

    UnityEvent buttonEvent;

    Button button;

    private void Awake() {
        if(buttonEvent == null) {
            buttonEvent = new UnityEvent();
        }
        buttonEvent.AddListener(() => PlayerController.mouseMode = MouseMode.Command);
        buttonEvent.AddListener(() => PlayerController.command = thisCommand);

        button = GetComponent<Button>();
        button.onClick.AddListener(() => buttonEvent.Invoke());
    }

    private void Update() {
        if(Input.GetKeyDown(hotkey)) { //the corresponding key can be pressed or the button on screen
            buttonEvent.Invoke();
        }
    }

    /// <summary>
    /// Set the text on the button.
    /// </summary>
    /// <param name="str"></param>
    public void SetText(string str) {
        buttonText.text = str;
    }

    /// <summary>
    /// Set which key triggers the command and display the key on the button.
    /// </summary>
    /// <param name="keyCode"></param>
    public void SetHotkey(KeyCode keyCode) {
        hotkey = keyCode;
        hotkeyText.text = hotkey.ToString();
    }

    public UnityEvent GetButtonEvent() {
        return buttonEvent;
    }

    /// <summary>
    /// Allows the button to be clicked and assigns the text and hotkey.
    /// </summary>
    /// <param name="str"></param>
    /// <param name="keyCode"></param>
    public void SetActive(string str, KeyCode keyCode) {
        button.interactable = true;
        SetText(str);
        SetHotkey(keyCode);
    }

    /// <summary>
    /// Disabled the button and clears its data.
    /// </summary>
    public void SetInactive() {
        button.interactable = false;
        buttonText.text = "";
        hotkeyText.text = "";
        thisCommand = null;
    }

    public bool IsActive() {
        return button.interactable;
    }
}
