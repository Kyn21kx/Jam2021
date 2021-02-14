using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Matching : MonoBehaviour {

    public enum Gender {
        Man,
        Woman,
        NonBinary
    }

    #region Variables
    [SerializeField]
    private Gender genderId;

    [SerializeField]
    private uint men;
    [SerializeField]
    private uint women;
    [SerializeField]
    private uint nonBi;

    public Gender GenderId { get { return genderId; } }
    #endregion

    public bool Match (Matching other) {
        //The only thing that we need to check is if the gender identity is included
        bool homoCheck, anyCheck;
        bool generalHomo = other.GenderId == GenderId;
        switch (GenderId) {
            case Gender.Man:
                if (other.men < 0)
                    return false;
                break;
            case Gender.Woman:
                if (other.women < 0)
                    return false;
                break;
            case Gender.NonBinary:
                if (other.nonBi < 0)
                    return false;
                break;
        }
        homoCheck = generalHomo && this.men > 0;
        anyCheck = this.women > 0 || this.nonBi > 0;
        return homoCheck || anyCheck;
    }

    public void Pair() {
        
    }

}
