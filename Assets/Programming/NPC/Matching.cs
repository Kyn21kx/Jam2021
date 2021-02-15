using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AI))]
public class Matching : MonoBehaviour {

    public enum Gender {
        Man,
        Woman,
        NonBinary
    }

    #region Variables
    [SerializeField]
    private Gender genderId;

    public float matchingTime;
    [SerializeField]
    private uint men;
    [SerializeField]
    private uint women;
    [SerializeField]
    private uint nonBi;

    public Gender GenderId { get { return genderId; } }
    public bool Paired { get; private set; }
    #endregion

    private void Start() {
        Paired = false;
    }

    public bool Match (Matching other) {
        //The only thing that we need to check is if the gender identity is included
        switch (GenderId) {
            case Gender.Man:
                if (other.men <= 0)
                    return false;
                break;
            case Gender.Woman:
                if (other.women <= 0)
                    return false;
                break;
            case Gender.NonBinary:
                if (other.nonBi <= 0)
                    return false;
                break;
        }
        switch (other.GenderId) {
            case Gender.Man:
                if (this.men <= 0)
                    return false;
                break;
            case Gender.Woman:
                if (this.women <= 0)
                    return false;
                break;
            case Gender.NonBinary:
                if (this.nonBi <= 0)
                    return false;
                break;
        }
        return true;
    }

    /// <summary>
    /// Reduces an element from the match and pairs the two couples
    /// </summary>
    public void Pair(Matching other) {
        ReduceByMatch(other);
        //Check that every single category is 0 before we pair
        Debug.Log("Paired!");
        Paired = men <= 0 && women <= 0 && nonBi <= 0;
    }

    private void ReduceByMatch(Matching other) {
        switch (other.GenderId) {
            case Gender.Man:
                men--;
                break;
            case Gender.Woman:
                women--;
                break;
            case Gender.NonBinary:
                nonBi--;
                break;
        }
    }

}
