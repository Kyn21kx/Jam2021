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
    private Seeker seeker;
    [Tooltip("This needs to be the exact same as the pathfinder's layermask")]
    public LayerMask obstacleLayer;
    public bool canMove;
    private Vector2 debugDir;

    public Transform TargetPosHolder { get { return agent.target; } }
    #endregion

    private void Start() {
        rig = GetComponent<Rigidbody2D>();
        path = GetComponent<AIPath>();
        agent = GetComponent<AIDestinationSetter>();
        seeker = GetComponent<Seeker>();
        path.maxSpeed = speed;
        canMove = true;
    }

    public void Move(Vector2 position) {
        if (canMove)
            TargetPosHolder.position = position;
    }

    private void OnDrawGizmos() {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(debugDir, 4f);
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
