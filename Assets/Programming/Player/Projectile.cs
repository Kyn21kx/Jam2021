using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Projectile : MonoBehaviour {

    #region Variables
    public Vector2 dir;
    public float travelSpeed;
    private float cntr = 0f;
    private Shooting shootingRef;
    private float travelDistance;
    private Rigidbody2D rig;
    #endregion

    public void Initiate(Vector2 dir, float travelSpeed, Shooting shootingRef, float travelDistance) {
        transform.parent = null;
        this.dir = dir;
        this.travelSpeed = travelSpeed;
        this.shootingRef = shootingRef;
        this.travelDistance = travelDistance;
        rig = GetComponent<Rigidbody2D>();
        float rotation = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, rotation - 90f);
    }

    //Destroy after a period of time
    private void FixedUpdate() {
        cntr += Time.fixedDeltaTime;
        rig.position = Vector2.Lerp(rig.position, rig.position + dir, Time.fixedDeltaTime * travelSpeed);
        //transform.Translate(dir * realSpeed);
        if (cntr >= travelDistance / travelSpeed || Detect())
            Destroy(gameObject);
    }

    private bool Detect() {
        RaycastHit2D hit = Physics2D.Raycast(rig.position, transform.forward, 1f, LayerMask.GetMask("Haters"));
        if (hit.transform != null) {
            //Send the player a reference to the hater object
            AI aiRef = hit.transform.GetComponent<AI>();
            shootingRef.BufferPerson(aiRef);
            //Stun the hater (soon to be lover) for a bit
            return true;
        }
        return false;
    }

}
