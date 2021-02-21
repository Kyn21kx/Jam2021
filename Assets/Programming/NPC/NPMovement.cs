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
    private Rigidbody2D rig;
    private AIPath path;
    private AIDestinationSetter agent;
    [Tooltip("This needs to be the exact same as the pathfinder's layermask")]
    public LayerMask obstacleLayer;
    public bool canMove;

    public Transform TargetPosHolder { get; private set; }
    public float Speed { get { return speed; } set { speed = value; path.maxSpeed = value; } }
    public float AuxSpeed { get; private set; }
    #endregion

    private void Awake() {
        rig = GetComponent<Rigidbody2D>();
        path = GetComponent<AIPath>();
        agent = GetComponent<AIDestinationSetter>();
        TargetPosHolder = agent.target;
        path.maxSpeed = speed;
        canMove = true;
        AuxSpeed = speed;
    }

    public void Move(Vector2 position) {
        if (canMove) {
            if (position.x - transform.position.x < 0) {
                GetComponent<SpriteRenderer>().flipX = false;
            }
            else {
                GetComponent<SpriteRenderer>().flipX = true;
            }
            TargetPosHolder.position = position;
        }
    }

    public bool HasArrived(Vector2 position, float threshold) {
        return Vector2.Distance(transform.position, position) <= threshold;
    }

    public void Stop() {
        rig.velocity = Vector2.zero;
        path.maxSpeed = 0f;
        path.canSearch = false;
        path.canMove = false;
    }

    public void ResumePath() {
        path.maxSpeed = speed;
        path.canMove = true;
        path.canSearch = true;
    }

}
