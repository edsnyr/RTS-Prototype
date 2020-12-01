using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MouseMode { Standard, Command }
public delegate void Command(Vector3 pos);

public class PlayerController : MonoBehaviour
{

    public int team;
    public Color teamColor;

    public float timeForDoubleClick = 0.2f;
    public List<Unit> units;
    public List<UnitSpawner> spawners;
    public static LayerMask groundMask; //ground layer to determine mouse location raycasts

    public static Command command; //applies movement commands to mouse behavior

    public static MouseMode mouseMode = MouseMode.Standard; //whether a command is loaded from the action panel

    UnitSelection unitSelection;
    MoveCommand moveCommand;

    void Awake()
    {
        groundMask = LayerMask.GetMask("Ground");
        moveCommand = GetComponent<MoveCommand>();
        unitSelection = GetComponent<UnitSelection>();
        command = moveCommand.MoveUnits;
    }



    private void Update() {
        if(PlayerControlSwitch.GetPlayerID() == team) { //only execute if tester is in control of this player
            GetClick();
        }
    }

    /// <summary>
    /// Determines if a mouse click has occurred and execute or cancel the appropriate command.
    /// </summary>
    private void GetClick() {
        if(Input.GetMouseButtonDown(0)) {
            if(mouseMode == MouseMode.Command) {
                command(GetGroundPosition(Input.mousePosition)); //perform selected action (move, attack, patrol, etc.)
                mouseMode = MouseMode.Standard; //action performed, return to standard mouse mode
            }
            //unit selection script handles standard left mouse clicks by default
        } else if(Input.GetMouseButtonDown(1)) {
            if(mouseMode == MouseMode.Command) {
                mouseMode = MouseMode.Standard; //cancel command
            } else if(mouseMode == MouseMode.Standard) {
                moveCommand.MoveUnits(GetGroundPosition(Input.mousePosition)); //standard right click is always to move selected units
            }
        }
    }

    /// <summary>
    /// Finds the point where the mouse is over the ground.
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    public static Vector3 GetGroundPosition(Vector3 pos) {
        Ray ray = Camera.main.ScreenPointToRay(pos);
        if(Physics.Raycast(ray, out RaycastHit hit, 100, groundMask)) {
            return hit.point;
        }

        return Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }
}
