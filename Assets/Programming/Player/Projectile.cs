using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour {

    #region Variables
    public Vector2 dir;
    public float travelSpeed;
    private float cntr = 0f;
    private Shooting shootingRef;
    #endregion

    public void Initiate(Vector2 dir, float travelSpeed, Shooting shootingRef) {
        transform.parent = null;
        this.dir = dir;
        this.travelSpeed = travelSpeed;
        this.shootingRef = shootingRef;
    }

    //Destroy after a period of time
    private void Update() {
        cntr += Time.deltaTime;
        transform.Translate(dir * travelSpeed * Time.deltaTime);
        if (cntr >= 5f || Detect())
            Destroy(gameObject);
    }

    private bool Detect() {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, transform.forward, 1f, LayerMask.GetMask("Haters"));
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
