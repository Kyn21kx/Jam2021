using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEditor;

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
    public List<Matching> previousMatches;
    [SerializeField]
    private GameObject symbolCountPref;
    [SerializeField]
    private GameObject[] symbols;
    [SerializeField]
    private Sprite male, female, nBinary;
    public Gender GenderId { get { return genderId; } }
    public bool Paired { get { return men <= 0 && women <= 0 && nonBi <= 0; } }
    public AI AI_Ref { get { return selfAI; } }
    #endregion

    private void Start() {
        Initialize();
    }

    private void Initialize() {
        selfAI = GetComponent<AI>();
        auxMen = men;
        auxWomen = women;
        auxNonBi = nonBi;
        symbols = new GameObject[3];
        previousMatches = new List<Matching>();

        UpdateSymbols(start: true);

        GetComponentInChildren<TextMeshPro>().SetText(genderId.ToString());

    }

    private void Update() {
        UpdateSymbols();
    }

    public void SetRandValues() {
        genderId = (Gender)Random.Range(0, 3);
        Randomize:
        int p = Random.Range(0, 3);
        if (p == 2)
            men = Random.Range(0, 5);
        p = Random.Range(0, 2);
        if (p == 1)
            women = Random.Range(0, 5);
        p = Random.Range(0, 2);
        if (p == 0)
            nonBi = Random.Range(0, 5);
        if (men <= 0 && women <= 0 && nonBi <= 0)
            goto Randomize;
        Initialize();
    }

    private void UpdateSymbols(bool start = false) {
        float offsetX = -0.3f;
        int[] preferences = { men, women, nonBi };
        Sprite[] sprites = { male, female, nBinary };
        for (int i = 0; i < 3; i++) {
            if (preferences[i] > 0) {
                
                if (symbols[i] == null)
                    symbols[i] = Instantiate(symbolCountPref, transform);
                
                symbols[i].transform.localPosition = new Vector2(offsetX, symbols[i].transform.localPosition.y);
                offsetX += 0.3f;
            }
            else if (symbols[i] != null)
                Destroy(symbols[i]);

            SpawnSymbol(symbols[i], sprites[i], preferences[i], start);
        }
    }

    private void SpawnSymbol(GameObject obj, Sprite sprite, int amnt, bool start) {
        if (obj == null) return;
        if (start) {
            SpriteRenderer renderer = obj.GetComponent<SpriteRenderer>();
            renderer.sprite = sprite;
        }
        TextMeshPro txt = obj.GetComponentInChildren<TextMeshPro>();
        txt.text = ("x" + amnt);
    }

    public bool Match (Matching other) {
        if (previousMatches.Contains(other)) return false;
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
    /// Increases by one the number of matches according to the gender, and removes itself and any previous matches
    /// </summary>
    private void IncreaseMatchesAndRemove() {
        for (int i = 0; i < previousMatches.Count; i++) {
            Matching match = previousMatches[i];
            
            match.IncreaseByMatch(this);
            match.previousMatches.Remove(this);
            
            this.previousMatches.RemoveAt(i--);
        }
    }

    /// <summary>
    /// Reduces an element from the match and pairs the two couples
    /// </summary>
    public void Pair(Matching other) {
        ReduceByMatch(other);
        other.ReduceByMatch(this);

        previousMatches.Add(other);
        other.previousMatches.Add(this);
        //Check that every single category is 0 before we pair
        if (ValidPair() && Paired) {
            ChainLovers();
        }
        //Really ugly thing
        if (other.ValidPair() && other.Paired) {
            ChainLovers();
        }
    }


    public void ChainLovers() {
        List<Matching> totalLovers = new List<Matching>();
        totalLovers.AddRange(previousMatches);
        for (int i = 0; i < previousMatches.Count; i++) {
            Matching match = previousMatches[i];
            for (int j = 0; j < match.previousMatches.Count; j++) {
                if (!totalLovers.Contains(match.previousMatches[j]) && match.previousMatches[j].Paired) {
                    totalLovers.Add(match.previousMatches[j]);
                }
            }
        }
        for (int i = 0; i < totalLovers.Count; i++) {
            totalLovers[i].selfAI.ConvertToLover();
        }
        selfAI.ConvertToLover();
        
    }

    public bool ValidPair() {
        for (int i = 0; i < previousMatches.Count; i++) {
            if (!previousMatches[i].Paired)
                return false;
        }
        return true;
    }

    /// <summary>
    /// Sets the values for "wanted" people to their original values
    /// </summary>
    public void RestorePreferences() {
        men = auxMen;
        women = auxWomen;
        nonBi = auxNonBi;
        IncreaseMatchesAndRemove();
        UpdateSymbols(start: true);
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

    public void IncreaseByMatch(Matching other) {
        switch (other.GenderId) {
            case Gender.Man:
                men++;
                break;
            case Gender.Woman:
                women++;
                break;
            case Gender.NonBinary:
                nonBi++;
                break;
        }
    }

}
