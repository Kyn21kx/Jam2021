using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Movement : MonoBehaviour {

    #region Variables
    private Rigidbody2D rig;
    [SerializeField]
    private Vector2 movVector;
    [SerializeField]
    private float speed;
    #endregion

    private void Start() {
        rig = GetComponent<Rigidbody2D>();
        movVector = Vector2.zero;
    }

    private void Update() {
        ReadInput();
    }

    private void FixedUpdate() {
        Move();
    }

    private void ReadInput() {
        movVector = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
    }

    private void Move() {
        Vector2 nextPos = (Vector2)transform.position + movVector;
        rig.position = Vector2.Lerp(transform.position, nextPos, Time.fixedDeltaTime * speed);
    }
}
