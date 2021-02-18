using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationController : MonoBehaviour {

    public enum States {
        Idle
    }

    #region Variables
    public States state;
    #endregion

    private void Start() {
        state = States.Idle;
    }

    private void Update() {

    }

}
