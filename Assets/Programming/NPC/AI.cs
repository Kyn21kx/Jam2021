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
    public float conversionTimer;
    public float conversionTime;
    /// <summary>
    /// Acts like a poison value
    /// </summary>
    public float rangedConversion;
    private float prevConversionValue;
    [SerializeField]
    private float auxStunTime;
    private float nutzTime;
    public float rangedAttCooldown;
    [SerializeField]
    private GameObject conversionBars;
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

    public void Initialize() {
        MatchRef = GetComponent<Matching>();
        LoverCombatRef = GetComponent<LoverCombat>();
        movRef = GetComponent<NPMovement>();
        var player = GameObject.FindGameObjectWithTag("Player");
        playerRef = player.transform;
        currState = States.Patrol;
        CurrentTarget = null;
        CanAttack = true;
        conversionBars.SetActive(false);
        nutzTime = 0f;
        if (ranged)
            actionRange = 20f;
        SetNewLHValues();
        nodePosition = movRef.TargetPosHolder.position;
    }

    private void Update() {
        StunManager();
        //FSM
        FiniteStateMachine();
        ManageConversionValues();
    }

    public void OverrideTarget(Transform target) {
        CurrentTarget = target;
        currState = States.Chase;
    }

    private void ManageConversionValues() {
        if (rangedConversion > 0f) {
            rangedConversion -= Time.deltaTime;
            conversionTimer += 0.5f * Time.deltaTime;
        }
        if (conversionTimer == prevConversionValue)
            conversionTimer -= Time.deltaTime;
        rangedConversion = Mathf.Clamp(rangedConversion, 0f, 2f);
        conversionTimer = Mathf.Clamp(conversionTimer, 0f, conversionTime);
        //UI display for health bar
        if (conversionTimer > 0f && lover) {
            conversionBars.SetActive(true);
            float ratio = conversionTimer / conversionTime;
            Transform barToScale = conversionBars.transform.GetChild(0);
            barToScale.localScale = new Vector3(ratio, 1f, 1f);
        }
        else
            conversionBars.SetActive(false);

        prevConversionValue = conversionTimer;
    }

    private void FiniteStateMachine() {
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
                if (nutzTime == 0f)
                    RecoverStun();
                if (nutzTime > 4f) {
                    GetComponent<SpriteRenderer>().color = Color.gray;
                    nutzTime = 0f;
                    movRef.Speed = movRef.AuxSpeed;
                    currState = States.Patrol;
                    break;
                }
                movRef.Speed = movRef.AuxSpeed * 2.5f;
                GetComponent<SpriteRenderer>().color = Color.red;
                nutzTime += Time.deltaTime;
                StartCoroutine(GoNutz(0.5f));
                break;
            case States.Loving:
                CurrentTarget = Detect();
                if (!movRef.HasArrived(playerRef.position, 5f)) {
                    movRef.ResumePath();
                    movRef.Move(playerRef.position);
                }
                else
                    movRef.Stop();
                
                if (CurrentTarget != null) {
                    Vector2 toTarget = CurrentTarget.position - transform.position;
                    float sqrDistance = toTarget.sqrMagnitude;

                    Shoot();
                    
                    //Check for distance to the current target, and if they're too close run away
                    if (sqrDistance <= LoverCombatRef.fleeDistance * LoverCombatRef.fleeDistance)
                        currState = States.Chase;

                }

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

    private IEnumerator GoNutz(float changeTime) {
        //Create random magnitude as well
        float magnitude = Random.Range(10f, 20f);
        Vector2 nPos = Utilities.GetRandomVector(0f, 1f) * magnitude;
        //Random position from 0 to 1
        nodePosition = nPos;
        yield return new WaitForSeconds(changeTime);

        movRef.Move(nodePosition);
    }

    private void RunAway() {
        Vector2 toTarget = CurrentTarget.position - transform.position;
        float sqrDistance = toTarget.sqrMagnitude;
        if (sqrDistance > LoverCombatRef.escapedDistance * LoverCombatRef.escapedDistance) {
            currState = States.Loving;
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

    public void ResetNode() {
        nodePosition = transform.position;
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
            else
                Shoot();
            return;
        }
        #endregion

            #region Attacking a lover
        AI loverRef = CurrentTarget.GetComponent<AI>();
        if (!ranged)
            loverRef.conversionTimer += Time.deltaTime;
        else
            Shoot();

        //Ranged attacks are managed by the projectile script
        
        if (loverRef.conversionTimer >= conversionTime) {
            //Attack
            loverRef.ConvertToHater();
            loverRef.conversionTimer = 0f;
            CurrentTarget = null;
            currState = States.Patrol;
        }
        #endregion
    }

    private void Shoot() {
        if (rangedAttCooldown <= 0f) {
            movRef.Stop();
            var instance = Instantiate(projectile, transform.position, Quaternion.identity);
            Projectile projInstance = instance.GetComponent<Projectile>();
            //t = v / d
            projInstance.Initiate((CurrentTarget.position - transform.position).normalized, projSpeed, this, projMaxDistance);
            rangedAttCooldown = 2f;
            movRef.ResumePath();
        }
        else
            rangedAttCooldown -= Time.deltaTime;
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
    #region Conversion
    public void ConvertToLover() {
        if (lover) return;
        Utilities.scoreManager.score++;
        Utilities.spawner.previous.Remove(MatchRef);
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
        MatchRef.previousMatches.Clear();
        currState = States.Patrol;
    }
    #endregion
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
