using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Matching : MonoBehaviour {

    public enum Gender {
        Man,
        Woman,
        NonBinary
    }

    public enum Preference {
        Hetero,
        Homo,
        Bi,
        Pan
    }

    #region Variables
    [SerializeField]
    private Gender genderId;
    [SerializeField]
    private Preference sexualPreference;
    public Gender GenderId { get { return genderId; } }
    public Preference SexualPreference { get { return sexualPreference; } }
    #endregion



}
