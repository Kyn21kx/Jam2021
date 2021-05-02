using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

[RequireComponent(typeof(AstarPath))]
public class GameManager : MonoBehaviour {

	#region Variables
	public GridGraph PathGrid { get; private set; }
	public bool Paused { get; private set; }

	private AstarPath pathSettings;
	public List<AI> lovers;
	public List<AI> buffers;
	#endregion

	private void Start() {
		Time.timeScale = 1f;
		Utilities.playerRef = GameObject.FindGameObjectWithTag("Player").transform;
		Utilities.scoreManager = FindObjectOfType<ScoreManager>();
		Utilities.spawner = FindObjectOfType<Spawner>();
		Utilities.gameManager = this;
		Utilities.anim = Utilities.playerRef.GetComponent<Animator>();
		pathSettings = GetComponent<AstarPath>();
		//Make sure this always works
		PathGrid = (GridGraph)pathSettings.graphs[0];
		InvokeRepeating("UpdateGraph", 0.5f, 0.5f);
	}

	private void Update() {
		if (Input.GetButtonDown("Pause")) {
			Time.timeScale = Time.timeScale == 0f ? 1f : 0f;
			Paused = Time.timeScale == 0f;
		}
	}

	private void UpdateGraph() {
		pathSettings.Scan(PathGrid);
	}

	public void Buffer(AI personRef, AI directBuffer=null) {
		personRef.Stun(personRef.MatchRef.matchingTime);
		if (personRef.MatchRef.Paired) return;
		if (directBuffer != null) {
			BufferPerson(personRef, directBuffer);
			return;
		}
		if (buffers.Contains(personRef)) return;
		buffers.Add(personRef);
		for (int i = 0; i < buffers.Count; i++) {
			BufferPerson(personRef, i);
		}
	}

	public void BufferPerson(AI personRef, AI other) {
		personRef.BeginMatch();
		//Pair them
		if (personRef != other) {
			var match1 = personRef.MatchRef;
			var lover = other.MatchRef;
			lover.LoverPair(match1);
			//Clean them and reset their states
			personRef.movRef.canMove = true;
			personRef.movRef.ResumePath();


			personRef.StopAllCoroutines();
		}
	}

	public void BufferPerson(AI personRef, int index) {
		personRef.BeginMatch();
		if (buffers[index] == null) {
			//Stun the person for a couple of seconds
			buffers[index] = personRef;
			//Send the information to the UI
		}
		else {
			//Pair them
			if (ValidTargets(personRef, buffers[index])) {
				var match1 = personRef.MatchRef;
				var match2 = buffers[index].MatchRef;

				if (match1.Match(match2)) {
					match1.Pair(match2);
					//Clean them and reset their states
					personRef.movRef.canMove = true;
					personRef.movRef.ResumePath();

					buffers[index].movRef.canMove = true;
					buffers[index].movRef.ResumePath();

					personRef.StopAllCoroutines();
					buffers[index].StopAllCoroutines();
					buffers.RemoveAt(index);
					buffers.Remove(personRef);
				}
				else {
					//Make 'em go crazy
					match1.AI_Ref.ResetNode();
					match1.AI_Ref.currState = AI.States.Nutz;

					match2.AI_Ref.ResetNode();
					match2.AI_Ref.currState = AI.States.Nutz;
					buffers.RemoveAt(index);
					buffers.Remove(personRef);
				}

			}
		}
	}

	private bool ValidTargets(AI t1, AI t2) {
		return t1 != t2 && (t1.OpenForMatch && t2.OpenForMatch && !t1.MatchRef.Paired && !t2.MatchRef.Paired);
	}

}
