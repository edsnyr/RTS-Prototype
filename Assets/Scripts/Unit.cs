using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Status { Idle, Moving, AttackMoving, Attacking, Patrolling, Holding };

public abstract class Unit : MonoBehaviour
{
    public string unitName;
    public KeyCode hotkey; //the key to be pressed to create this unit from a spawner
    public int team; //numerical indication of what team the unit belongs to
    public HealthSlider health;
    public int maxHealth = 10;
    public CapsuleCollider attackRangeCollider; //the unit can attack other units within this collider
    public int attackRange = 5;
    public int attackDamage = 1;
    public float timeBetweenAttacks = 1f;

    public float moveSpeed = 3f;
    public SpriteRenderer selectionCircle;

    public Status status = Status.Idle;

    public List<Unit> unitsInRange; //list of units within the attack range collider

    IEnumerator currentAction; //what action the unit is currently doing
    IEnumerator holdingAction; //what action is on hold until the current action completes

    void Awake()
    {
        unitsInRange = new List<Unit>();
        health.transform.SetParent(FindObjectOfType<Canvas>().transform); //temporary; health slider should be created by the entity that spawns a unit and tied together then
        health.InitializeSlider(transform, maxHealth);
        attackRangeCollider.radius = attackRange; //sets the appropriate radius on the attack range collider
    }

    /// <summary>
    /// The selection circle being active indicates the unit is currently selected.
    /// </summary>
    /// <param name="active"></param>
    public void SetSelectionCircle(bool active) {
        selectionCircle.gameObject.SetActive(active);
    }

    /// <summary>
    /// Instructs the unit to move to the specified location.
    /// </summary>
    /// <param name="pos"></param>
    public void SetDestination(Vector3 pos) {
        StopAction();
        status = Status.Moving;
        currentAction = MoveToPosition(new Vector3(pos.x, transform.position.y, pos.z), status);
        StartCoroutine(currentAction);
    }

    /// <summary>
    /// Instructs the unit to move between its starting location and a specified location.
    /// </summary>
    /// <param name="orig"></param>
    /// <param name="dest"></param>
    public void SetPatrol(Vector3 orig, Vector3 dest) {
        StopAction();
        status = Status.Patrolling;
        currentAction = PatrolPositions(orig, dest);
        StartCoroutine(currentAction);
    }

    /// <summary>
    /// Instructs the unit to move to a specified location, stopping to attack any unit in its path.
    /// </summary>
    /// <param name="pos"></param>
    public void SetAttackMove(Vector3 pos) {
        StopAction();
        status = Status.AttackMoving;
        currentAction = MoveToAttack(new Vector3(pos.x, transform.position.y, pos.z), status);
        StartCoroutine(currentAction);
    }

    /// <summary>
    /// The unit moves toward a specified position until it gets there or recieves a new command.
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="commandStatus"></param>
    /// <returns></returns>
    private IEnumerator MoveToPosition(Vector3 pos, Status commandStatus) {
        while(transform.position != pos && status == commandStatus) {
            transform.position = Vector3.MoveTowards(transform.position, pos, moveSpeed * Time.deltaTime);
            yield return null;
        }
    }

    /// <summary>
    /// The unit moves between two positions until it recieves new commands.
    /// </summary>
    /// <param name="orig"></param>
    /// <param name="dest"></param>
    /// <returns></returns>
    private IEnumerator PatrolPositions(Vector3 orig, Vector3 dest) {
        //TODO while patrolling, look for attack targets
        while(status == Status.Patrolling) {
            yield return StartCoroutine(MoveToPosition(new Vector3(dest.x, transform.position.y, dest.z), status));
            yield return StartCoroutine(MoveToPosition(new Vector3(orig.x, transform.position.y, orig.z), status));
        }
    }

    /// <summary>
    /// The unit moves toward a specified position, attacking units it finds on the way.
    /// If its target dies, returns to this coroutine.
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="commandStatus"></param>
    /// <returns></returns>
    private IEnumerator MoveToAttack(Vector3 pos, Status commandStatus) {
        while(transform.position != pos && status == commandStatus) {
            for(int i = unitsInRange.Count - 1; i >= 0; i--) { //in case killed units are not properly removed from the in range list
                if(!unitsInRange[i].gameObject.activeSelf) {
                    unitsInRange.Remove(unitsInRange[i]);
                }
            }
            if(unitsInRange.Count > 0) { //if there are units in range, attack the first one
                SetAttack(unitsInRange[0]);
            }
            transform.position = Vector3.MoveTowards(transform.position, pos, moveSpeed * Time.deltaTime); //otherwise, move toward target destination
            yield return null;
        }
    }

    /// <summary>
    /// Attack a unit until it recieves new orders. If the target moves out of its attack range, it moves to follow.
    /// </summary>
    /// <param name="unit"></param>
    /// <returns></returns>
    private IEnumerator AttackUnit(Unit unit) {
        float attackCooldown = 0; //timer, allows an attack to occur if 0
        while(status == Status.Attacking) {
            if(unit.gameObject.activeSelf) { //units become inactive when their health runs out
                if(Vector3.Distance(unit.transform.position, transform.position) > attackRange) { //if the unit is out of range, get closer
                    transform.position = Vector3.MoveTowards(transform.position, unit.transform.position, moveSpeed * Time.deltaTime);
                } else if(attackCooldown <= 0) { //if the cooldown allows an attack, deal damage and reset cooldown
                    unit.health.TakeDamage(attackDamage);
                    attackCooldown = timeBetweenAttacks;
                }
                if(attackCooldown > 0) {
                    attackCooldown -= Time.deltaTime;
                }
                yield return new WaitForEndOfFrame();
            } else { //if the unit is dead, return to attack move
                status = Status.AttackMoving;
                currentAction = holdingAction; //holding action carries the attack move command with original destination
                StartCoroutine(currentAction); //MoveToAttack
                break; //leave coroutine
            }
        }
    }

    /// <summary>
    /// Stops the current action.
    /// </summary>
    public void StopAction() {
        status = Status.Idle;
        if(currentAction != null)
            StopCoroutine(currentAction);
    }

    /// <summary>
    /// Holds the current MoveToAttack command, and sets the unit to attack the specified unit.
    /// </summary>
    /// <param name="unit"></param>
    private void SetAttack(Unit unit) {
        holdingAction = currentAction;
        StopAction();
        status = Status.Attacking;
        currentAction = AttackUnit(unit);
        StartCoroutine(currentAction);
    }

    /// <summary>
    /// Collects information of units in the range collider. 
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerEnter(Collider other) {
        if(other.CompareTag("Unit")) {
            Unit unit = other.GetComponent<Unit>();
            if(unit.team != team && !unitsInRange.Contains(unit)) {
                unitsInRange.Add(unit);
            }
        }
    }

    /// <summary>
    /// Removes units leaving the range collider.
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerExit(Collider other) {
        if(other.CompareTag("Unit")) {
            Unit unit = other.GetComponent<Unit>();
            if(unit.team != team && unitsInRange.Contains(unit)) {
                unitsInRange.Remove(unit);
            }
        }
    }
}
