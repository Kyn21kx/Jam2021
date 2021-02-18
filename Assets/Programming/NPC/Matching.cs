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
    private int men;
    [SerializeField]
    private int women;
    [SerializeField]
    private int nonBi;

    private int auxMen;
    private int auxWomen;
    private int auxNonBi;

    private AI selfAI;

    public Gender GenderId { get { return genderId; } }
    public bool Paired { get; private set; }
    #endregion

    private void Start() {
        auxMen = men;
        auxWomen = women;
        auxNonBi = nonBi;
        Paired = false;
        selfAI = GetComponent<AI>();
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
        other.ReduceByMatch(this);
        //Check that every single category is 0 before we pair
        Paired = men <= 0 && women <= 0 && nonBi <= 0;
        other.Paired = other.men <= 0 && other.women <= 0 && other.nonBi <= 0;
        if (Paired)
            selfAI.ConvertToLover();
        if (other.Paired)
            other.selfAI.ConvertToLover();
    }

    /// <summary>
    /// Sets the values for "wanted" people to their original values
    /// </summary>
    public void RestorePreferences() {
        men = auxMen;
        women = auxWomen;
        nonBi = auxNonBi;
        Paired = false;
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
