using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitSpawner : MonoBehaviour
{
    PlayerController player;
    public int team;
    public SpriteRenderer selectionCircle;

    public List<Unit> unitPrefabs;
    public float spawnRadius = 1;

    Vector3 spawnPosition; //where new units are spawned from
    Vector3 rallyPoint; //where new units are directed to go at spawn

    public void Awake() {
        foreach(PlayerController controller in FindObjectsOfType<PlayerController>()) {
            if(team == controller.team) {
                player = controller;
            }
        }
        spawnPosition = transform.position - new Vector3(0, 0, spawnRadius);
        rallyPoint = spawnPosition;
    }

    /// <summary>
    /// Spawns a unit from the list of possible units, then instructs it to go to the rally point.
    /// Adds the unit to the player's list of units.
    /// </summary>
    /// <param name="unit"></param>
    public void SpawnUnit(Unit unit) {
        if(unitPrefabs.Contains(unit)) {
            Unit spawn = Instantiate(unit);
            spawn.transform.position = spawnPosition;
            spawn.SetDestination(rallyPoint);
            spawn.team = team;
            if(player != null) {
                player.units.Add(spawn);
            }
        }
    }

    /// <summary>
    /// Sets a new rally point.
    /// </summary>
    /// <param name="pos"></param>
    public void SetRallyPoint(Vector3 pos) {
        rallyPoint = pos;
    }

    /// <summary>
    /// Sets the rally point to the spawn point.
    /// </summary>
    public void ResetRallyPoint() {
        rallyPoint = spawnPosition;
    }

    /// <summary>
    /// The selection circle being active indicates that the spawner is selected.
    /// </summary>
    /// <param name="active"></param>
    public void SetSelectionCircle(bool active) {
        selectionCircle.gameObject.SetActive(active);
    }
}
