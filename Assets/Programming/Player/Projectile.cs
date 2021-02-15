using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour {

    #region Variables
    public Vector2 dir;
    public float travelSpeed;
    private float cntr = 0f;
    #endregion

    public void Initiate(Vector2 dir, float travelSpeed) {
        transform.parent = null;
        this.dir = dir;
        this.travelSpeed = travelSpeed;
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
            //hit.transform.GetComponent<Health>().Damage(1, transform.position);
            return true;
        }
        return false;
    }

}
