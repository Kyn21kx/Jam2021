using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreManager : MonoBehaviour {

    public static int highScore;
    public int score;
    public Transform tail;
    private Vector2 auxTailPosition;
    private int prevScore;

    private void Start() {
        score = 0;
        prevScore = score;
        auxTailPosition = tail.position;
    }

    private void Update() {
        //Get the number of delta digits
        if (IncrementedDigit()) {
            //Instantiate a new sprite, and displace the other one
            prevScore = score;
        }
    }

    private bool IncrementedDigit() {
        return score - prevScore > 0;
    }

}
