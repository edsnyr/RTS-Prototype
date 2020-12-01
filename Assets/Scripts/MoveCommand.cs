using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveCommand : MonoBehaviour
{

    float lastMoveCommandTime = -1; //tracks time between clicks to determine if a double click occurs

    PlayerController playerController;
    UnitSelection unitSelection;

    void Awake()
    {
        playerController = GetComponent<PlayerController>();
        unitSelection = GetComponent<UnitSelection>();
    }

    /// <summary>
    /// Commands all selected units to move to a specified location. Units can move in formation.
    /// </summary>
    /// <param name="pos"></param>
    public void MoveUnits(Vector3 pos) {
        if(unitSelection.selectedUnits.Count == 1) { //no extra checks necessary if only one unit is selected
            unitSelection.selectedUnits[0].SetDestination(pos);
        } else {
            if(!DoubleClicked() && IsFormationMove(PlayerController.GetGroundPosition(Input.mousePosition))) { //if units should move in formation
                Vector3 originalCenter = GetSelectionCenter(); //get current center of selected units
                foreach(Unit unit in unitSelection.selectedUnits) {
                    Vector3 offset = unit.transform.position - originalCenter; //find distance from original center
                    unit.SetDestination(pos + offset); //go to destination with the offset applied
                }
            } else { //if units are not moving in formation
                foreach(Unit unit in unitSelection.selectedUnits) {
                    unit.SetDestination(pos); //all units go exactly to the specified location
                }
            }
        }
    }

    /// <summary>
    /// All units stop moving by cancelling their commands.
    /// </summary>
    public void StopUnits() {
        foreach(Unit unit in unitSelection.selectedUnits) {
            unit.StopAction();
        }
    }

    /// <summary>
    /// Commands all selected units to patrol from their current location to a new location. Can move in formation.
    /// </summary>
    /// <param name="pos"></param>
    public void PatrolUnits(Vector3 pos) {
        if(unitSelection.selectedUnits.Count == 1) { //no extra checks necessary if only one unit is selected
            unitSelection.selectedUnits[0].SetPatrol(unitSelection.selectedUnits[0].transform.position, pos);
        } else {
            if(IsFormationMove(PlayerController.GetGroundPosition(Input.mousePosition))) { //formation move
                Vector3 originalCenter = GetSelectionCenter();
                foreach(Unit unit in unitSelection.selectedUnits) {
                    Vector3 offset = unit.transform.position - originalCenter;
                    unit.SetPatrol(unit.transform.position, pos + offset);
                }
            } else {
                foreach(Unit unit in unitSelection.selectedUnits) {
                    unit.SetPatrol(unit.transform.position, pos);
                }
            }
        }
    }

    /// <summary>
    /// Commands all units to move to the new location, stopping to attack units along the way. Can formation move.
    /// </summary>
    /// <param name="pos"></param>
    public void AttackUnits(Vector3 pos) {
        if(unitSelection.selectedUnits.Count == 1) { //no extra checks necessary if only one unit is selected
            unitSelection.selectedUnits[0].SetAttackMove(pos);
        } else {
            if(IsFormationMove(PlayerController.GetGroundPosition(Input.mousePosition))) {
                Vector3 originalCenter = GetSelectionCenter();
                foreach(Unit unit in unitSelection.selectedUnits) {
                    Vector3 offset = unit.transform.position - originalCenter;
                    unit.SetAttackMove(pos + offset);
                }
            } else {
                foreach(Unit unit in unitSelection.selectedUnits) {
                    unit.SetAttackMove(pos);
                }
            }
        }
    }

    /// <summary>
    /// Determines if units should keep their current formation when moving.
    /// If a movement command is outside of a square area around selected units,
    /// it is a formation move.
    /// </summary>
    /// <param name="clickedPos"></param>
    /// <returns></returns>
    private bool IsFormationMove(Vector3 clickedPos) {
        //get area of selected units
        Vector3 min = new Vector3();
        Vector3 max = new Vector3();
        foreach(Unit unit in unitSelection.selectedUnits) {
            if(min == null) {
                min = unit.transform.position;
            } else { //check against current min values
                if(unit.transform.position.x < min.x) {
                    min = new Vector3(unit.transform.position.x, min.y, min.z);
                }
                if(unit.transform.position.z < min.z) {
                    min = new Vector3(min.x, min.y, unit.transform.position.z);
                }
            }

            if(max == null) {
                max = unit.transform.position;
            } else { //check against current max values
                if(unit.transform.position.x > max.x) {
                    max = new Vector3(unit.transform.position.x, max.y, max.z);
                }
                if(unit.transform.position.z > max.z) {
                    max = new Vector3(max.x, max.y, unit.transform.position.z);
                }
            }
        }

        //if pos is outside of area, formation move
        if(clickedPos.x > min.x && clickedPos.x < max.x && clickedPos.z > min.z && clickedPos.z < max.z)
            return false;
        else
            return true;
    }

    /// <summary>
    /// Returns the center point of a selection of units by averaging their positions.
    /// </summary>
    /// <returns></returns>
    private Vector3 GetSelectionCenter() {
        float x = 0;
        float z = 0;
        foreach(Unit unit in unitSelection.selectedUnits) {
            x += unit.transform.position.x;
            z += unit.transform.position.z;
        }
        return new Vector3(x / unitSelection.selectedUnits.Count, 1, z / unitSelection.selectedUnits.Count);
    }

    /// <summary>
    /// Override formation move if double clicked
    /// </summary>
    /// <returns></returns>
    private bool DoubleClicked() {
        if(lastMoveCommandTime == -1) { //initialized to -1, has not clicked yet
            lastMoveCommandTime = Time.time;
            return false;
        } else {
            float deltaTime = Time.time - lastMoveCommandTime;
            lastMoveCommandTime = Time.time; //update most recent click
            if(deltaTime < playerController.timeForDoubleClick)
                return true;
            else
                return false;
        }
    }

    /// <summary>
    /// Sets the rally point for all selected spawners.
    /// </summary>
    /// <param name="pos"></param>
    public void SetRallyPoint(Vector3 pos) {
        foreach(UnitSpawner spawner in unitSelection.selectedSpawners) {
            spawner.SetRallyPoint(pos);
        }
    }

    /// <summary>
    /// Resets the rally point for all selected spawners.
    /// </summary>
    public void ResetRallyPoint() {
        foreach(UnitSpawner spawner in unitSelection.selectedSpawners) {
            spawner.ResetRallyPoint();
        }
    }
}
