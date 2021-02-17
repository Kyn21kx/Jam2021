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
    private float conversionTimer;
    public Matching MatchRef { get; private set; }
    public bool OpenForMatch { get; private set; }
    public bool CanAttack { get; private set; }
    #endregion



    private void Start() {
        MatchRef = GetComponent<Matching>();
        movRef = GetComponent<NPMovement>();
        nodePosition = movRef.TargetPosHolder.position;
        var player = GameObject.FindGameObjectWithTag("Player");
        playerRef = player.transform;
        currState = States.Patrol;
        CurrentTarget = null;
        CanAttack = true;
        SetNewLHValues();
    }

    private void Update() {
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
                break;
            case States.PostAttck:
                CanAttack = false;
                //Move on the opposite direction of the player, then go back to chasing
                Vector2 dir = transform.position - playerRef.position;
                float sqrDis = dir.sqrMagnitude;
                dir = (Vector2)transform.position + dir.normalized;
                if (sqrDis <= (actionRange + actionRange / 2f) * (actionRange + actionRange / 2f))
                    movRef.Move(dir);
                else {
                    CanAttack = true;
                    currState = States.Chase;
                }
                break;
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
            yield return new WaitForSeconds(2f);
            movRef.ResumePath();
            //Create random magnitud as well
            float magnitude = Random.Range(20f, 40f);
            //Random position from 0 to 1
            nodePosition = Utilities.GetRandomVector(0f, 1f) * magnitude;
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
        Vector2 toTarget = CurrentTarget.position - transform.position;
        float distanceSqr = toTarget.SqrMagnitude();
        if (distanceSqr <= actionRange * actionRange && CanAttack) {
            if (playerRef.Equals(CurrentTarget)) {
                //Get the health component and fuck him up
                playerRef.GetComponent<HealthManager>().Damage();
                currState = States.PostAttck;
                return;
            }
            AI loverRef = CurrentTarget.GetComponent<AI>();
            loverRef.conversionTimer += Time.deltaTime;
            if (loverRef.conversionTimer >= 1f) {
                //Attack
                loverRef.ConvertToHater();
                loverRef.conversionTimer = 0f;
                CurrentTarget = null;
                currState = States.Patrol;
            }
        }
        else {
            if (!playerRef.Equals(CurrentTarget)) {
                AI loverRef = CurrentTarget.GetComponent<AI>();
                loverRef.conversionTimer = 0f;
            }
            movRef.Move(CurrentTarget.position);
        }
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
        currState = States.Patrol;
    }

    private Transform Detect() {
        //Circle cast
        return Physics2D.CircleCast(transform.position, detectionRange, Vector2.zero, 0f, targetMask).transform;
    }

    private void OnDrawGizmos() {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, actionRange);
    }

    public void BeginMatch() {
        OpenForMatch = true;
        Stun(MatchRef.matchingTime);
        StartCoroutine(CloseMatch());
    }

    public void Stun(float time) {
        movRef.Stop();
        CanAttack = false;
        movRef.canMove = false;
        StartCoroutine(RecoverStun(time));
    }

    private IEnumerator RecoverStun(float time) {
        yield return new WaitForSeconds(time);
        movRef.canMove = true;
        CanAttack = true;
        movRef.ResumePath();
    }

    private IEnumerator CloseMatch() {
        yield return new WaitForSeconds(MatchRef.matchingTime);
        OpenForMatch = false;
        var shootingRef = playerRef.GetComponent<Shooting>();
        shootingRef.personBuffer = null;
    }

}
