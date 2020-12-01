using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ActionPanel : MonoBehaviour
{

    /*
     * Controls the action panel in the corner of the screen. If further developed,
     * some or all behavior of each button may be handled by units directly, as
     * they will have unit-specific commands to add to the action panel.
     */

    public Transform panel;
    public ActionButton[] actionButtons;

    UnitSelection unitSelection;
    MoveCommand moveCommand;

    private void Awake() {
        unitSelection = GetComponent<UnitSelection>();
        moveCommand = GetComponent<MoveCommand>();
    }

    private void Start() {
        SetUnitPanelButtons();
    }

    /// <summary>
    /// Sets the panel buttons to command units. Enables Move, Stop, Patrol, and Attack
    /// for selected units.
    /// </summary>
    public void SetUnitPanelButtons() {
        foreach(ActionButton button in actionButtons) {
            button.SetInactive();
        }
        if(actionButtons.Length >= 1 && actionButtons[0] != null) {
            actionButtons[0].SetActive("Move", KeyCode.M);
            actionButtons[0].thisCommand = moveCommand.MoveUnits;
        }
        if(actionButtons.Length >= 2 && actionButtons[1] != null) {
            actionButtons[1].SetActive("Stop", KeyCode.S);
            UnityEvent buttonEvent = actionButtons[1].GetButtonEvent();
            buttonEvent.RemoveAllListeners();
            buttonEvent.AddListener(() => moveCommand.StopUnits());

        }
        if(actionButtons.Length >= 3 && actionButtons[2] != null) {
            actionButtons[2].SetActive("Patrol", KeyCode.P);
            actionButtons[2].thisCommand = moveCommand.PatrolUnits;
        }
        if(actionButtons.Length >= 4 && actionButtons[3] != null) {
            actionButtons[3].SetActive("Attack", KeyCode.A);
            actionButtons[3].thisCommand = moveCommand.AttackUnits;
        }
    }

    /// <summary>
    /// Sets the panel buttons for spawners. Enables Rally, Cancel Rally, and
    /// Spawn for available units in selected spawners.
    /// </summary>
    public void SetSpawnerPanelButtons() {
        foreach(ActionButton button in actionButtons) {
            button.SetInactive();
        }
        if(actionButtons.Length >= 1 && actionButtons[0] != null) {
            actionButtons[0].SetActive("Rally", KeyCode.R);
            actionButtons[0].thisCommand = moveCommand.SetRallyPoint;
        }
        if(actionButtons.Length >= 2 && actionButtons[1] != null) {
            actionButtons[1].SetActive("End Rally", KeyCode.X);
            UnityEvent buttonEvent = actionButtons[1].GetButtonEvent();
            buttonEvent.RemoveAllListeners();
            buttonEvent.AddListener(() => moveCommand.ResetRallyPoint());
        }
        if(actionButtons.Length > 4 && unitSelection.selectedSpawners.Count > 0) {
            for(int i = 4; i < actionButtons.Length; i++) {
                if(unitSelection.selectedSpawners[0].unitPrefabs.Count > (i-4)) {
                    Unit unit = unitSelection.selectedSpawners[0].unitPrefabs[i - 4];
                    Debug.Log(unitSelection.selectedSpawners[0].unitPrefabs[i - 4]);
                    actionButtons[i].SetActive(unit.unitName, unit.hotkey);
                    UnityEvent buttonEvent = actionButtons[i].GetButtonEvent();
                    buttonEvent.RemoveAllListeners();
                    buttonEvent.AddListener(() => unitSelection.selectedSpawners[0].SpawnUnit(unit));
                }
            }
        }

    }
}
