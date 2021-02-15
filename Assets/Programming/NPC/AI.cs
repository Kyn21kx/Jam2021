using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI : MonoBehaviour {

    public enum States {
        Patrol,
        Chase,
        Nutz,
        Loving
    }

    #region Variables
    private Transform playerRef;
    public NPMovement movRef;
    private Vector2 nodePosition;
    public States currState;
    [SerializeField]
    private float detectionRange;
    [SerializeField]
    private LayerMask targetMask;
    public Matching MatchRef { get; private set; }
    public bool OpenForMatch { get; private set; }
    #endregion



    private void Start() {
        MatchRef = GetComponent<Matching>();
        movRef = GetComponent<NPMovement>();
        nodePosition = movRef.TargetPosHolder.position;
        var player = GameObject.FindGameObjectWithTag("Player");
        playerRef = player.transform;
        currState = States.Patrol;
    }

    private void Update() {
        //FSM
        switch (currState) {
            case States.Patrol:
                StartCoroutine(Patrol());
                break;
            case States.Chase:
                movRef.Stop();
                break;
            case States.Nutz:
                break;
            case States.Loving:
                break;
        }
    }

    private IEnumerator Patrol() {
        //Check for lover
        if (DetectLover()) {
            currState = States.Chase;
            yield break;
        }
        //Create a new vector everytime
        if (movRef.HasArrived(nodePosition, 0.5f)) {
            movRef.Stop();
            yield return new WaitForSeconds(2f);
            movRef.ResumePath();
            //Create random magnitud as well
            float magnitude = Random.Range(20f, 50f);
            //Random position from 0 to 1
            nodePosition = Utilities.GetRandomVector(0f, 1f) * magnitude;
        }
        movRef.Move(nodePosition);
    }

    private bool DetectLover() {
        //Circle cast
        return Physics2D.CircleCast(transform.position, detectionRange, Vector2.zero, 0f, targetMask);
    }

    private void OnDrawGizmos() {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }

    public void BeginMatch() {
        OpenForMatch = true;
        Stun(MatchRef.matchingTime);
        StartCoroutine(CloseMatch());
    }

    public void Stun(float time) {
        movRef.Stop();
        movRef.canMove = false;
        StartCoroutine(RecoverStun(time));
    }

    private IEnumerator RecoverStun(float time) {
        yield return new WaitForSeconds(time);
        movRef.canMove = true;
        movRef.ResumePath();
    }

    private IEnumerator CloseMatch() {
        yield return new WaitForSeconds(MatchRef.matchingTime);
        OpenForMatch = false;
        var shootingRef = playerRef.GetComponent<Shooting>();
        shootingRef.personBuffer = null;
    }

}
