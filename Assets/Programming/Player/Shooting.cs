using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shooting : MonoBehaviour {

    #region Variables
    [SerializeField]
    private GameObject projectile;
    [SerializeField]
    private float shootingSpeed;
    private Camera cam;
    public AI personBuffer;
    #endregion

    private void Start() {
        cam = Camera.main;
        personBuffer = null;
    }

    private void Update() {
        //TODO: Add cooldown
        if (Input.GetMouseButtonDown(0))
            Shoot();
    }

    public void BufferPerson(AI personRef) {
        personRef.BeginMatch();
        if (personBuffer == null) {
            //Stun the person for a couple of seconds
            personBuffer = personRef;
            //Send the information to the UI
        }
        else {
            //Pair them
            if (personBuffer.OpenForMatch && personRef.OpenForMatch) {
                var match1 = personRef.MatchRef;
                var match2 = personBuffer.MatchRef;

                if (match1.Match(match2)) {
                    match1.Pair(match2);
                    //Clean them and reset their states
                    personRef.movRef.canMove = true;
                    personRef.movRef.ResumePath();
                    
                    personBuffer.movRef.canMove = true;
                    personBuffer.movRef.ResumePath();
                    
                    personRef.StopAllCoroutines();
                    personBuffer.StopAllCoroutines();
                    personBuffer = null;
                }
            
            }
        }
    }

    private void Shoot() {
        //Instantiate the prefab to the mouse's position
        Vector2 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
        Vector2 dir = mousePos - (Vector2)transform.position;
        var instance = Instantiate(projectile, transform.position, Quaternion.identity);
        Projectile projInstance = instance.GetComponent<Projectile>();
        projInstance.Initiate(dir.normalized, shootingSpeed, this);
    }

}
