using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitSelection : MonoBehaviour
{

    public PlayerController player;
    public ActionPanel actionPanel;
    public RectTransform selectionBox;

    public List<Unit> selectedUnits;
    public List<UnitSpawner> selectedSpawners;

    Vector2 startPos; //starting position of the selection box

    void Awake() {
        selectedUnits = new List<Unit>();
        selectedSpawners = new List<UnitSpawner>();
    }


    void LateUpdate() { //late update to stop wierd interactions with action panel clicks
        if(PlayerControlSwitch.GetPlayerID() == player.team) {
            GetClick();
        }
    }

    private void GetClick() {
        //TODO disable mouse down entirely after a command until mouse up
        if(PlayerController.mouseMode == MouseMode.Standard) {
            if(Input.GetMouseButtonDown(0)) { //if mouse down set box start position
                startPos = Input.mousePosition;
            }

            if(Input.GetMouseButton(0)) { //if held drag box position
                UpdateSelectionBox(Input.mousePosition);
            }

            if(Input.GetMouseButtonUp(0)) { //when released get selection within box
                GetSelection();
            }
        }
    }

    /// <summary>
    /// Redraws the selection box with opposite corners as the start position and current mouse position.
    /// </summary>
    /// <param name="currentPos"></param>
    private void UpdateSelectionBox(Vector2 currentPos) {
        if(!selectionBox.gameObject.activeInHierarchy) //show the selection box
            selectionBox.gameObject.SetActive(true);

        float width = currentPos.x - startPos.x;
        float height = currentPos.y - startPos.y;

        selectionBox.sizeDelta = new Vector2(Mathf.Abs(width), Mathf.Abs(height));
        selectionBox.anchoredPosition = startPos + new Vector2(width / 2, height / 2);
    }

    /// <summary>
    /// Get the new unit or spawner selection. Units take priority over spawners.
    /// </summary>
    private void GetSelection() {
        if(!GetUnitSelection()) { //if no units found in selection
            GetSpawnerSelection();//check if spawners are in selection instead
        }
    }

    /// <summary>
    /// Finds any units in the selection area and adds them to the unit selection list.
    /// </summary>
    /// <returns></returns>
    private bool GetUnitSelection() {
        selectionBox.gameObject.SetActive(false); //disable the selection box, keeps its size/location
        List<Unit> newSelection = new List<Unit>();

        Vector2 min = selectionBox.anchoredPosition - selectionBox.sizeDelta / 2; //get bottom left corner of selection
        Vector2 max = selectionBox.anchoredPosition + selectionBox.sizeDelta / 2; //get top right corner of selection

        foreach(Unit unit in player.units) { //check each unit the player owns if it is in the bounds
            Vector3 screenPos = Camera.main.WorldToScreenPoint(unit.transform.position);
            if(screenPos.x > min.x && screenPos.x < max.x && screenPos.y > min.y && screenPos.y < max.y) {
                newSelection.Add(unit);
            }
        }

        //ensure that a unit under the mouse is captured in selection
        Unit hoveredUnit = GetUnitAtPosition(Input.mousePosition);
        if(hoveredUnit != null && player.units.Contains(hoveredUnit) && !newSelection.Contains(hoveredUnit)) {
            newSelection.Add(hoveredUnit);
        }
        //ensure that a unit under the initial selection position is captured in selection
        hoveredUnit = GetUnitAtPosition(startPos);
        if(hoveredUnit != null && player.units.Contains(hoveredUnit) && !newSelection.Contains(hoveredUnit)) {
            newSelection.Add(hoveredUnit);
        }

        if(newSelection.Count > 0) { //if any units are captured in selection, highlight them
            ClearSelection();
            selectedUnits = newSelection;
            HighlightSelection();
            return true;
        } else { //no units selected, move to checking for spawners
            return false;
        }
    }

    /// <summary>
    /// Finds any spawners in the selection area and adds them to the spawner selection list.
    /// </summary>
    /// <returns></returns>
    private bool GetSpawnerSelection() {
        selectionBox.gameObject.SetActive(false); //disable the selection box, keeps its size/location
        List<UnitSpawner> newSelection = new List<UnitSpawner>();

        Vector2 min = selectionBox.anchoredPosition - selectionBox.sizeDelta / 2; //get bottom left corner of selection
        Vector2 max = selectionBox.anchoredPosition + selectionBox.sizeDelta / 2; //get top right corner of selection

        foreach(UnitSpawner spawner in player.spawners) { //check each spawner the player owns if it is in the bounds
            Vector3 screenPos = Camera.main.WorldToScreenPoint(spawner.transform.position);
            if(screenPos.x > min.x && screenPos.x < max.x && screenPos.y > min.y && screenPos.y < max.y) {
                newSelection.Add(spawner);
            }
        }

        //ensure that a spawner under the mouse is captured in selection
        UnitSpawner hoveredUnit = GetSpawnerAtPosition(Input.mousePosition);
        if(hoveredUnit != null && player.spawners.Contains(hoveredUnit) && !newSelection.Contains(hoveredUnit)) {
            newSelection.Add(hoveredUnit);
        }
        //ensure that a spawner under the initial selection position is captured in selection
        hoveredUnit = GetSpawnerAtPosition(startPos);
        if(hoveredUnit != null && player.spawners.Contains(hoveredUnit) && !newSelection.Contains(hoveredUnit)) {
            newSelection.Add(hoveredUnit);
        }

        if(newSelection.Count > 0) { //if any spawners are captured in selection, highlight them
            ClearSelection();
            selectedSpawners = newSelection;
            HighlightSelection();
            return true;
        } else { //no spawners found, no update to selection
            return false;
        }
    }

    /// <summary>
    /// Finds a unit at the specified position that might have been missed in the selection box,
    /// as its center must be inside the bounds to be selected.
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    private Unit GetUnitAtPosition(Vector2 pos) {
        Ray ray = Camera.main.ScreenPointToRay(pos);
        RaycastHit hit;

        if(Physics.Raycast(ray, out hit, 100, 9)) {
            if(hit.transform.gameObject.TryGetComponent(out Unit unit)) {
                return unit;
            }
        }
        return null;
    }

    /// <summary>
    /// Finds a spawner at the specified position that might have been missed in the selection box,
    /// as its center must be inside the bounds to be selected.
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    private UnitSpawner GetSpawnerAtPosition(Vector2 pos) {
        Ray ray = Camera.main.ScreenPointToRay(pos);
        RaycastHit hit;

        if(Physics.Raycast(ray, out hit, 100, 9)) {
            if(hit.transform.gameObject.TryGetComponent(out UnitSpawner spawner)) {
                return spawner;
            }
        }
        return null;
    }

    /// <summary>
    /// Removes selection circles and clears selection lists.
    /// </summary>
    private void ClearSelection() {
        foreach(Unit unit in selectedUnits) {
            unit.SetSelectionCircle(false);
        }
        foreach(UnitSpawner spawner in selectedSpawners) {
            spawner.SetSelectionCircle(false);
        }
        selectedUnits = new List<Unit>();
        selectedSpawners = new List<UnitSpawner>();
    }

    /// <summary>
    /// Enables selection circles for any selected units or spawners.
    /// </summary>
    private void HighlightSelection() {
        if(selectedUnits.Count > 0) {
            actionPanel.SetUnitPanelButtons();
            foreach(Unit unit in selectedUnits) {
                unit.SetSelectionCircle(true);
            }
        } else if(selectedSpawners.Count > 0) {
            actionPanel.SetSpawnerPanelButtons();
            foreach(UnitSpawner spawner in selectedSpawners) {
                spawner.SetSelectionCircle(true);
            }
        }
    }
}