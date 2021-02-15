using System.Collections;
using System.Collections.Generic;
using Pathfinding;
using UnityEngine;

[RequireComponent(typeof(AI))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(AIPath))]
[RequireComponent(typeof(AIDestinationSetter))]
[RequireComponent(typeof(Seeker))]
public class NPMovement : MonoBehaviour {

    #region Variables
    [SerializeField]
    private float speed;
    [SerializeField]
    private Transform targetPosHolder;
    private Rigidbody2D rig;
    [SerializeField]
    private float detectionRange;
    private AIPath path;
    private AIDestinationSetter agent;
    public bool canMove;
    #endregion

    private void Start() {
        rig = GetComponent<Rigidbody2D>();
        path = GetComponent<AIPath>();
        agent = GetComponent<AIDestinationSetter>();
        path.maxSpeed = speed;
        canMove = true;
    }

    public void Move(Vector2 position) {
        if (!canMove) return;
        targetPosHolder.position = position;
        agent.target = targetPosHolder;
    }

    public void Stop() {
        rig.velocity = Vector2.zero;
        path.maxSpeed = 0f;
        path.canMove = false;
    }

    public void ResumePath() {
        path.maxSpeed = speed;
        path.canMove = true;
    }

}
