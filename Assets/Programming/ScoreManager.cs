using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour {

	public static int highScore;
	
	[SerializeField]
	private GameObject middleClone;
	[SerializeField]
	private Transform canvas;
	public TextMeshProUGUI txt;
	public RectTransform tail;

	public int score;
	private bool incremented;

	private void Start() {
		score = 0;
		incremented = false;
	}

	private void Update() {
		txt.transform.SetAsLastSibling();
		txt.SetText(score.ToString());
		//Get the number of delta digits
		if (IncrementedDigit()) {
			if (!incremented) {
				//Instantiate a new sprite, and displace the other one
				var instance = Instantiate(middleClone, canvas);
				var rectTransform = instance.GetComponent<RectTransform>();
				rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x + 51f, rectTransform.anchoredPosition.y);
				tail.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x + 35f, tail.anchoredPosition.y);
				incremented = true;
			}
		}
		else
			incremented = false;
	}

	private bool IncrementedDigit() {
		return score > 0 && score % 10 == 0;
	}

}
