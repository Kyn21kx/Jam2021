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
    private float holdTime;
    public bool holding;
    [SerializeField]
    private float cooldown;
    private float auxCooldown;
    private bool onCooldown;
    #endregion

    private void Start() {
        cam = Camera.main;
        personBuffer = null;
        holdTime = 0f;
        holding = false;
        auxCooldown = cooldown;
        onCooldown = false;
    }

    private void Update() {
        //TODO: Add cooldown
        HoldShoot();
        Cooldown();
    }

    private void Cooldown() {
        if (onCooldown) {
            cooldown -= Time.deltaTime;
            if (cooldown <= 0f) {
                cooldown = auxCooldown;
                onCooldown = false;
            }
        }
    }

    private void HoldShoot() {
        if (Input.GetMouseButtonDown(0))
            holding = true;
        if (holding && Input.GetMouseButtonUp(0)) {
            //d = x^(1.5f) - (x/2)
            //Graphics to show it
            holdTime = Mathf.Clamp(holdTime, 0f, 2f);
            float d = (holdTime * (35f / 1.5f)) + 10;
            //Debug.LogWarning("Distance: " + d);
            holding = false;
            holdTime = 0f;
            Utilities.anim.SetBool("Holding", false);
            Shoot(d);
        }
        //Increment x 
        if (holding) {
            holdTime += Time.deltaTime;
            Vector2 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
            float xValue = mousePos.x - transform.position.x;
            if (xValue > 0) {
                GetComponent<SpriteRenderer>().flipX = true;
            }
            else {
                GetComponent<SpriteRenderer>().flipX = false;
            }
            Utilities.anim.SetBool("Holding", true);
        }
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
            if (ValidTargets(personRef, personBuffer)) {
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
                else {
                    //Make 'em go crazy
                    match1.AI_Ref.ResetNode();
                    match1.AI_Ref.currState = AI.States.Nutz;

                    match2.AI_Ref.ResetNode();
                    match2.AI_Ref.currState = AI.States.Nutz;
                    personBuffer = null;
                }
            
            }
        }
    }

    private bool ValidTargets(AI t1, AI t2) {
        return t1 != t2 && t1.OpenForMatch && t2.OpenForMatch && !t1.MatchRef.Paired && !t2.MatchRef.Paired;
    }

    private void Shoot(float distance) {
        if (!onCooldown) {
            //Instantiate the prefab to the mouse's position
            Vector2 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
            Vector2 dir = mousePos - (Vector2)transform.position;
            var instance = Instantiate(projectile, transform.position, Quaternion.identity);
            Projectile projInstance = instance.GetComponent<Projectile>();
            //t = v / d
            projInstance.Initiate(dir.normalized, shootingSpeed, this, distance);
            onCooldown = true;
        }
    }

}
