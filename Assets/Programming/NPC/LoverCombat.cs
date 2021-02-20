using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoverCombat : MonoBehaviour {

    //TODO: REFACTOR THIS PLEASEEEEEEEEEEEEEEEEEEEEEE
    #region Variables
    [SerializeField]
    AI personBuffer;
    #endregion

    private void Start() {
        personBuffer = null;
    }

    public void Hunt(AI agentRef) {
        if (agentRef.CurrentTarget != null) {

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
                }

            }
        }
    }

    private bool ValidTargets(AI t1, AI t2) {
        return t1 != t2 && t1.OpenForMatch && t2.OpenForMatch && !t1.MatchRef.Paired && !t2.MatchRef.Paired;
    }

}
