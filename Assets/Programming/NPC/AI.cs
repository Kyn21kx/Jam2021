﻿using System.Collections;
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
    public bool lover;
    private Transform playerRef;
    private Transform currentTarget;
    public NPMovement movRef;
    private Vector2 nodePosition;
    public States currState;
    [SerializeField]
    private float detectionRange;
    [SerializeField]
    private float actionRange;
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
        currentTarget = null;
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
                break;
            case States.Nutz:
                break;
            case States.Loving:
                break;
        }
    }

    private IEnumerator Patrol() {
        //Check for lover
        currentTarget = Detect();
        if (currentTarget != null) {
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

    private void RunAway() {

    }

    private void Chase() {
        Vector2 toTarget = currentTarget.position - transform.position;
        float distanceSqr = toTarget.SqrMagnitude();
        //If we detect the player instead of a lover, switch to them [IN REVIEW]
        currentTarget = Detect() == playerRef ? playerRef : currentTarget;
        if (distanceSqr <= actionRange * actionRange) {
            
            if (playerRef.Equals(currentTarget)) {
                //Get the health component and fuck him up
                playerRef.GetComponent<HealthManager>().Damage();
                return;
            }
            //Attack
            AI loverRef = currentTarget.GetComponent<AI>();
            loverRef.ConvertToHater();
            currentTarget = null;
            currState = States.Patrol;
        
        }
        else {
            //Follow
            movRef.Move(currentTarget.position);
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
        currentTarget = null;
        currState = States.Loving;
    }

    public void ConvertToHater() {
        lover = false;
        SetNewLHValues();
        currentTarget = null;
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
