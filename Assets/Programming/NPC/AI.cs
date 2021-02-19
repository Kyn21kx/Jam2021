using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI : MonoBehaviour {

    public enum States {
        Patrol,
        Chase,
        PostAttck,
        Nutz,
        Loving
    }

    #region Variables
    public bool lover;
    public bool ranged;
    [SerializeField]
    private GameObject projectile;
    private Transform playerRef;
    public Transform CurrentTarget { get; private set; }
    public NPMovement movRef;
    [SerializeField]
    private Vector2 nodePosition;
    public States currState;
    [SerializeField]
    private float detectionRange;
    [SerializeField]
    private float actionRange;
    [SerializeField]
    private LayerMask targetMask;
    [SerializeField]
    private float conversionTimer;
    [SerializeField]
    private float auxStunTime;
    private float rangedAttCooldown;
    [SerializeField]
    private float projMaxDistance, projSpeed;
    public Matching MatchRef { get; private set; }
    public LoverCombat LoverCombatRef { get; private set; }
    public bool OpenForMatch { get; private set; }
    public bool CanAttack { get; private set; }
    public bool Stunned { get; private set; }
    #endregion

    private void Start() {
        Initialize();
    }

    private void Initialize() {
        MatchRef = GetComponent<Matching>();
        LoverCombatRef = GetComponent<LoverCombat>();
        movRef = GetComponent<NPMovement>();
        var player = GameObject.FindGameObjectWithTag("Player");
        playerRef = player.transform;
        currState = States.Patrol;
        CurrentTarget = null;
        CanAttack = true;
        SetNewLHValues();
        nodePosition = movRef.TargetPosHolder.position;
    }

    private void Update() {
        StunManager();
        //FSM
        switch (currState) {
            case States.Patrol:
                StartCoroutine(Patrol());
                break;
            case States.Chase:
                if (!lover)
                    Chase();
                else
                    RunAway();
                break;
            case States.Nutz:
                break;
            case States.Loving:
                if (!movRef.HasArrived(playerRef.position, 5f)) {
                    movRef.ResumePath();
                    movRef.Move(playerRef.position);
                }
                else
                    movRef.Stop();
                break;
            case States.PostAttck:
                BackOff();
                break;
        }
    }

    /// <summary>
    /// Manages the stun behaviours and countdown
    /// </summary>
    private void StunManager() {
        if (Stunned) {
            auxStunTime -= Time.deltaTime;
            if (auxStunTime <= 0f)
                RecoverStun();
        }
    }

    private void BackOff() {
        CanAttack = false;
        //Move on the opposite direction of the player, then go back to chasing
        Vector2 dir = transform.position - playerRef.position;
        float sqrDis = dir.sqrMagnitude;
        dir = (Vector2)transform.position + dir.normalized * 10f;
        if (sqrDis <= (actionRange * 3f) * (actionRange * 3f))
            movRef.Move(dir);
        else {
            CanAttack = true;
            currState = States.Chase;
        }
    }

    private IEnumerator Patrol() {
        //Check for lover
        CurrentTarget = Detect();
        if (CurrentTarget != null) {
            nodePosition = transform.position;
            currState = States.Chase;
            yield break;
        }
        //Create a new vector everytime
        if (movRef.HasArrived(nodePosition, 0.5f)) {
            movRef.Stop();
            //Create random magnitude as well
            float magnitude = Random.Range(20f, 40f);
            Vector2 nPos = Utilities.GetRandomVector(0f, 1f) * magnitude;
            //Random position from 0 to 1
            nodePosition = nPos;
            yield return new WaitForSeconds(2f);
            movRef.ResumePath();
        }
        movRef.Move(nodePosition);
    }

    private void RunAway() {
        if (CurrentTarget.GetComponent<AI>().CurrentTarget != transform) {
            currState = States.Patrol;
            return;
        }
        if (movRef.HasArrived(nodePosition, 0.5f)) {
            nodePosition = (transform.position - CurrentTarget.position).normalized;
            nodePosition = (Vector2)transform.position + nodePosition;
            float a = Random.Range(0f, 180f);
            Utilities.OffsetVectorByAngle(ref nodePosition, a);

        }
        movRef.Move(nodePosition);
    }
    private void Chase() {
        //Maybe refactor this a bit
        //If we detect the player instead of a lover, switch to them [IN REVIEW]
        Transform tmp = Detect();
        CurrentTarget = tmp  != null && tmp.position.sqrMagnitude < CurrentTarget.position.sqrMagnitude ? tmp : CurrentTarget;
        
        Vector2 origin = CurrentTarget.position - transform.position;
        origin = (Vector2)transform.position + (origin.normalized * 2f);
        
        //Replace this with a distance operation
        RaycastHit2D hit = Physics2D.CircleCast(origin, actionRange, Vector2.zero, 0f, targetMask);

        if (hit.transform != null && CanAttack) {
            Attack();
        }
        else {
            //Keep moving and reset the lover's timer if applicable
            if (!playerRef.Equals(CurrentTarget)) {
                AI loverRef = CurrentTarget.GetComponent<AI>();
                loverRef.conversionTimer = 0f;
            }
            movRef.Move(CurrentTarget.position);
        }
    }

    private void Attack () {
        #region Attacking the player
        if (playerRef.Equals(CurrentTarget)) {
            //Get the health component and fuck him up
            if (!ranged) {
                playerRef.GetComponent<HealthManager>().Damage();
                currState = States.PostAttck;
            }
            else {
                if (rangedAttCooldown <= 0f) {
                    var instance = Instantiate(projectile, transform.position, Quaternion.identity);
                    Projectile projInstance = instance.GetComponent<Projectile>();
                    //t = v / d
                    projInstance.Initiate((CurrentTarget.position - transform.position).normalized, projSpeed, this, projMaxDistance);
                    rangedAttCooldown = 2f;
                }
                else
                    rangedAttCooldown -= Time.deltaTime;
            }
            return;
        }
        #endregion

        #region Attacking a lover
        AI loverRef = CurrentTarget.GetComponent<AI>();
        loverRef.conversionTimer += Time.deltaTime;
        
        if (loverRef.conversionTimer >= 1f) {
            //Attack
            loverRef.ConvertToHater();
            loverRef.conversionTimer = 0f;
            CurrentTarget = null;
            currState = States.Patrol;
        }
        #endregion
    }

    /// <summary>
    /// Sets the agent's unique values based on his type (lover or hater)
    /// </summary>
    private void SetNewLHValues() {
        if (lover) {
            //Change this for an animation
            GetComponent<SpriteRenderer>().color = Color.magenta;
            //Set own layer
            gameObject.layer = LayerMask.NameToLayer("Lovers");
            //Set target layer
            int t = 1 << 8;
            targetMask = t;
        }
        else {
            //Change this for an animation
            GetComponent<SpriteRenderer>().color = Color.gray;
            //Set own layer
            gameObject.layer = LayerMask.NameToLayer("Haters");
            //Set target layers
            int playerLayer = 1 << 9;
            int loverLayer = 1 << 10;
            targetMask = playerLayer | loverLayer;
        }
    }

    public void ConvertToLover() {
        lover = true;
        SetNewLHValues();
        CurrentTarget = null;
        currState = States.Loving;
    }

    public void ConvertToHater() {
        lover = false;
        SetNewLHValues();
        CurrentTarget = null;
        MatchRef.RestorePreferences();
        movRef.canMove = true;
        movRef.ResumePath();
        currState = States.Patrol;
    }

    private Transform Detect() {
        //Circle cast
        return Physics2D.CircleCast(transform.position, detectionRange, Vector2.zero, 0f, targetMask).transform;
    }

    private void OnDrawGizmos() {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.yellow;
        if (CurrentTarget != null) {
            Vector2 origin = CurrentTarget.position - transform.position;
            origin = (Vector2)transform.position + (origin.normalized * 2f);
            Gizmos.DrawWireSphere(origin, actionRange);
        }
    }

    public void BeginMatch() {
        StopAllCoroutines();
        Stun(MatchRef.matchingTime);
        OpenForMatch = true;
        StartCoroutine(CloseMatch());
    }

    public void Stun(float time) {
        movRef.Stop();
        CanAttack = false;
        movRef.canMove = false;
        auxStunTime = time;
        Stunned = true;
    }

    private void RecoverStun() {
        movRef.canMove = true;
        CanAttack = true;
        Stunned = false;
        movRef.ResumePath();
    }

    private IEnumerator CloseMatch() {
        yield return new WaitForSeconds(MatchRef.matchingTime);
        OpenForMatch = false;
        var shootingRef = playerRef.GetComponent<Shooting>();
        shootingRef.personBuffer = null;
    }

}
