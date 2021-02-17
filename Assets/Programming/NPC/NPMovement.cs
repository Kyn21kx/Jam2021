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
    private Vector2 debugDir = Vector2.zero;

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
        if (canMove) {
            //Check if there are obstacles in our way
            Vector2 dir = position - (Vector2)transform.position;
            debugDir = (Vector2)transform.position + dir.normalized;
            RaycastHit2D hit = Physics2D.CircleCast(transform.position, 4f, dir.normalized, 2f, obstacleLayer);
            if (hit.transform == null) {
                //Normal direct vector movement
                path.enabled = false;
                agent.enabled = false;
                seeker.enabled = false;
                rig.position = Vector2.Lerp(rig.position, position, Time.deltaTime * speed);
            }
            else {
                //Pathfinding movement
                path.enabled = true;
                agent.enabled = true;
                seeker.enabled = true;
                TargetPosHolder.position = position;
            }
        }
    }

    private void OnDrawGizmos() {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere((Vector2)transform.position + (debugDir * 2f), 4f);
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
