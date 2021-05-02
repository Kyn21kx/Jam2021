using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class HighScore : MonoBehaviour {

	private void Start() {
		GetComponent<TextMeshProUGUI>().SetText(ScoreManager.highScore.ToString());
	}

}
