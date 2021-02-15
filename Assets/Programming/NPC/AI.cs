using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI : MonoBehaviour {

    #region Variables
    private Shooting playerShootingRef;
    public NPMovement movRef;
    public Matching MatchRef { get; private set; }
    public bool OpenForMatch { get; private set; }
    #endregion



    private void Start() {
        MatchRef = GetComponent<Matching>();
        movRef = GetComponent<NPMovement>();
        var player = GameObject.FindGameObjectWithTag("Player");
        playerShootingRef = player.GetComponent<Shooting>();
    }

    private void Update() {
        
    }

    public void BeginMatch() {
        OpenForMatch = true;
        Stun(MatchRef.matchingTime);
        StartCoroutine(CloseMatch());
    }

    public void Stun(float time) {
        movRef.Stop();
        movRef.canMove = false;
        StartCoroutine(RecoverStun(time));
    }

    private IEnumerator RecoverStun(float time) {
        yield return new WaitForSeconds(time);
        movRef.canMove = true;
        movRef.ResumePath();
    }

    private IEnumerator CloseMatch() {
        yield return new WaitForSeconds(MatchRef.matchingTime);
        OpenForMatch = false;
        playerShootingRef.personBuffer = null;
    }

}
