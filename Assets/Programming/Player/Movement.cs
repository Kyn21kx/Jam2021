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
    private bool idle;
    private SpriteRenderer spriteRenderer;
    #endregion

    private void Start() {
        rig = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        movVector = Vector2.zero;
        idle = true;
    }

    private void Update() {
        ReadInput();
    }

    private void FixedUpdate() {
        Move();
    }

    private void ReadInput() {
        movVector = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        if (!GetComponent<Shooting>().holding) {
            if (movVector.x > 0f)
                spriteRenderer.flipX = true;
            else if (movVector.x < 0f)
                spriteRenderer.flipX = false;
        }
    }

    private void Move() {
        if (movVector == Vector2.zero)
            idle = true;
        else if (idle && movVector != Vector2.zero) {
            rig.velocity *= 0f;
            idle = false;
        }
        Vector2 nextPos = rig.position + movVector;
        rig.position = Vector2.Lerp(rig.position, nextPos, Time.fixedDeltaTime * speed);
    }
}
