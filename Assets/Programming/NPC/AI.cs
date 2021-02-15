using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI : MonoBehaviour {

    #region Variables
    private Matching agentMatch;
    #endregion



    private void Start() {
        agentMatch = GetComponent<Matching>();
    }

    private void Update() {
        
    }

}
