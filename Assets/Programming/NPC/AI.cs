﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI : MonoBehaviour {

	public enum States {
		Patrol,
		Chase,
		PostAttck,
		Nutz,
		Loving
	}

	#region Variables
	public Matching MatchRef { get; private set; }
	public LoverCombat LoverCombatRef { get; private set; }
	public Transform CurrentTarget { get; private set; }
	public bool OpenForMatch { get; private set; }
	public bool CanAttack { get; private set; }
	public bool Stunned { get; private set; }

	[SerializeField]
	private GameObject projectile;
	[SerializeField]
	private GameObject fuckProjectile;
	[SerializeField]
	private GameObject conversionBars;
	[SerializeField]
	private Sprite[] angSprite;
	[SerializeField]
	private Sprite[] lovSprite;
	private Transform playerRef;
	public NPMovement movRef;

	[SerializeField]
	private Vector2 nodePosition;
	[SerializeField]
	private LayerMask targetMask;

	public States currState;
	
	[SerializeField]
	private float detectionRange;
	[SerializeField]
	private float actionRange;
	[SerializeField]
	public float conversionTimer;
	[SerializeField]
	private float auxStunTime;
	[SerializeField]
	private float projMaxDistance, projSpeed;
	public float conversionTime;
	/// <summary>
	/// Acts like a poison value
	/// </summary>
	public float rangedConversion;
	private float prevConversionValue;
	private float nutzTime;
	public float rangedAttCooldown;
	public bool lover;
	public bool ranged;
	#endregion

	private void Start() {
		Initialize();
	}

	public void Initialize() {
		MatchRef = GetComponent<Matching>();
		LoverCombatRef = GetComponent<LoverCombat>();
		movRef = GetComponent<NPMovement>();
		var player = GameObject.FindGameObjectWithTag("Player");
		playerRef = player.transform;
		currState = States.Patrol;
		CurrentTarget = null;
		CanAttack = true;
		conversionBars.SetActive(false);
		nutzTime = 0f;
		if (ranged)
			actionRange = 20f;
		SetNewLHValues();
		nodePosition = movRef.TargetPosHolder.position;
	}

	private void Update() {
		StunManager();
		//FSM
		FiniteStateMachine();
		ManageConversionValues();
		CorrectConversion();
		if (!OpenForMatch && Utilities.gameManager.buffers.Contains(this)) {
			Utilities.gameManager.buffers.Remove(this);
		}
	}


	private void CorrectConversion() {
		if (!MatchRef.Paired || lover) return;
		bool result = true;
		//Check that everyone on the previous matches is a lover
		for (int i = 0; i < MatchRef.previousMatches.Count; i++) {
			if (MatchRef != MatchRef.previousMatches[i] && !MatchRef.previousMatches[i].AI_Ref.lover) {
				result = false;
				break;
			}
		}
		if (result)
			ConvertToLover();
	}

	public void OverrideTarget(Transform target) {
		CurrentTarget = target;
		currState = States.Chase;
	}

	private void ManageConversionValues() {
		if (rangedConversion > 0f) {
			rangedConversion -= Time.deltaTime;
			conversionTimer += 0.5f * Time.deltaTime;
		}
		if (conversionTimer == prevConversionValue)
			conversionTimer -= Time.deltaTime;
		rangedConversion = Mathf.Clamp(rangedConversion, 0f, 2f);
		conversionTimer = Mathf.Clamp(conversionTimer, 0f, conversionTime);
		//UI display for health bar
		if (conversionTimer > 0f && lover) {
			conversionBars.SetActive(true);
			float ratio = conversionTimer / conversionTime;
			Transform barToScale = conversionBars.transform.GetChild(0);
			barToScale.localScale = new Vector3(ratio, 1f, 1f);
		}
		else
			conversionBars.SetActive(false);

		prevConversionValue = conversionTimer;
	}

	private void FiniteStateMachine() {
		switch (currState) {
			case States.Patrol:
				StartCoroutine(Patrol());
				break;
			case States.Chase:
				if (!lover)
					Chase();
				break;
			case States.Nutz:
				if (nutzTime == 0f) {
					RecoverStun();
					//Create random magnitude as well
					float magnitude = Random.Range(10f, 20f);
					Vector2 nPos = Utilities.GetRandomVector(0f, 1f) * magnitude;
					//Random position from 0 to 1
					nodePosition = nPos;
				}
				if (nutzTime > 4f) {
					GetComponent<SpriteRenderer>().color = Color.white;
					nutzTime = 0f;
					movRef.Speed = movRef.AuxSpeed;
					currState = States.Patrol;
					break;
				}
				if (movRef.HasArrived(nodePosition, 0.5f)) {
					//Create random magnitude as well
					float magnitude = Random.Range(10f, 20f);
					Vector2 nPos = Utilities.GetRandomVector(0f, 1f) * magnitude;
					//Random position from 0 to 1
					nodePosition = nPos;
				}
				movRef.Speed = movRef.AuxSpeed * 2.5f;
				GetComponent<SpriteRenderer>().color = Color.red;
				nutzTime += Time.deltaTime;
				movRef.Move(nodePosition);
				break;
			case States.Loving:
				GetComponent<SpriteRenderer>().color = Color.white;
				nutzTime = 0f;
				movRef.Speed = movRef.AuxSpeed;
				CurrentTarget = Detect();
				if (!movRef.HasArrived(playerRef.position, 5f)) {
					movRef.ResumePath();
					movRef.Move(playerRef.position);
				}
				else
					movRef.Stop();
				
				if (CurrentTarget != null) {
					Shoot();
				}

				break;
			case States.PostAttck:
				BackOff();
				break;
		}
	}

	/// <summary>
	/// Manages the stun behaviours and countdown
	/// </summary>
	private void StunManager() {
		if (Stunned) {
			auxStunTime -= Time.deltaTime;
			if (auxStunTime <= 0f)
				RecoverStun();
		}
	}

	private void BackOff() {
		CanAttack = false;
		//Move on the opposite direction of the player, then go back to chasing
		Vector2 dir = transform.position - playerRef.position;
		float sqrDis = dir.sqrMagnitude;
		dir = (Vector2)transform.position + dir.normalized * 10f;
		if (sqrDis <= (actionRange * 3f) * (actionRange * 3f))
			movRef.Move(dir);
		else {
			CanAttack = true;
			currState = States.Chase;
		}
	}

	private IEnumerator Patrol() {
		//Check for lover
		CurrentTarget = Detect();
		if (CurrentTarget != null) {
			nodePosition = transform.position;
			currState = States.Chase;
			yield break;
		}
		//Create a new vector everytime
		if (movRef.HasArrived(nodePosition, 0.5f)) {
			movRef.Stop();
			//Create random magnitude as well
			float magnitude = Random.Range(20f, 40f);
			Vector2 nPos = Utilities.GetRandomVector(0f, 1f) * magnitude;
			//Random position from 0 to 1
			nodePosition = nPos;
			yield return new WaitForSeconds(2f);
			movRef.ResumePath();
		}
		movRef.Move(nodePosition);
	}

	private void RunAway() {
		Vector2 toTarget = CurrentTarget.position - transform.position;
		float sqrDistance = toTarget.sqrMagnitude;
		if (sqrDistance > LoverCombatRef.escapedDistance * LoverCombatRef.escapedDistance) {
			currState = States.Loving;
			return;
		}
		if (movRef.HasArrived(nodePosition, 0.5f)) {
			nodePosition = (transform.position - CurrentTarget.position).normalized;
			nodePosition = (Vector2)transform.position + nodePosition;
			float a = Random.Range(0f, 180f);
			Utilities.OffsetVectorByAngle(ref nodePosition, a);

		}
		movRef.Move(nodePosition);
	}

	public void ResetNode() {
		nodePosition = transform.position;
	}

	private void Chase() {
		GetComponent<SpriteRenderer>().color = Color.white;
		nutzTime = 0f;
		movRef.Speed = movRef.AuxSpeed;
		//Maybe refactor this a bit
		//If we detect the player instead of a lover, switch to them [IN REVIEW]
		Transform tmp = Detect();
		CurrentTarget = tmp  != null && tmp.position.sqrMagnitude < CurrentTarget.position.sqrMagnitude ? tmp : CurrentTarget;
		
		Vector2 origin = CurrentTarget.position - transform.position;
		origin = (Vector2)transform.position + (origin.normalized * 2f);
		
		//Replace this with a distance operation
		RaycastHit2D hit = Physics2D.CircleCast(origin, actionRange, Vector2.zero, 0f, targetMask);

		if (hit.transform != null && CanAttack) {
			Attack();
		}
		else {
			//Keep moving and reset the lover's timer if applicable
			movRef.Move(CurrentTarget.position);
		}
	}

	private void Attack () {
		#region Attacking the player
		if (playerRef.Equals(CurrentTarget)) {
			//Get the health component and fuck him up
			if (!ranged) {
				playerRef.GetComponent<HealthManager>().Damage();
				currState = States.PostAttck;
			}
			else
				Shoot();
			return;
		}
		#endregion

		#region Attacking a lover
		AI loverRef = CurrentTarget.GetComponent<AI>();
		if (!ranged)
			loverRef.conversionTimer += Time.deltaTime;
		else
			Shoot();

		//Ranged attacks are managed by the projectile script
		
		if (loverRef.conversionTimer >= conversionTime) {
			//Attack
			loverRef.ConvertToHater();
			loverRef.conversionTimer = 0f;
			CurrentTarget = null;
			currState = States.Patrol;
		}
		#endregion
	}

	private void Shoot() {
		if (rangedAttCooldown <= 0f) {
			movRef.Stop();
			GameObject p = lover ? projectile : fuckProjectile;
			var instance = Instantiate(p, transform.position, Quaternion.identity);
			Projectile projInstance = instance.GetComponent<Projectile>();
			//t = v / d
			projInstance.Initiate((CurrentTarget.position - transform.position).normalized, projSpeed, this, projMaxDistance);
			rangedAttCooldown = 2f;
			movRef.ResumePath();
		}
		else
			rangedAttCooldown -= Time.deltaTime;
	}

	/// <summary>
	/// Sets the agent's unique values based on his type (lover or hater)
	/// </summary>
	private void SetNewLHValues() {
		if (lover) {
			//Change this for an animation
			SpriteRenderer r = GetComponent<SpriteRenderer>();
			//Set own layer
			gameObject.layer = LayerMask.NameToLayer("Lovers");
			r.sprite = lovSprite[(int)MatchRef.GenderId];
			//Set target layer
			int t = 1 << 8;
			targetMask = t;
		}
		else {
			//Change this for an animation
			SpriteRenderer r = GetComponent<SpriteRenderer>();
			r.sprite = angSprite[(int)MatchRef.GenderId];
			//Set own layer
			gameObject.layer = LayerMask.NameToLayer("Haters");
			//Set target layers
			int playerLayer = 1 << 9;
			int loverLayer = 1 << 10;
			targetMask = playerLayer | loverLayer;
		}
	}
	#region Conversion
	public void ConvertToLover() {
		if (lover || !MatchRef.Paired) return;
		Utilities.spawner.previous.Remove(MatchRef);
		Utilities.gameManager.lovers.Add(this);
		//Redundant, but avoids bugs
		if (!lover)
			Utilities.scoreManager.score++;
		lover = true;
		SetNewLHValues();
		for (int i = 0; i < MatchRef.previousMatches.Count; i++) {
			MatchRef.previousMatches[i].previousMatches.Remove(MatchRef);
		}

		CurrentTarget = null;
		currState = States.Loving;
	}

	public void ConvertToHater() {
		lover = false;
		SetNewLHValues();
		CurrentTarget = null;
		MatchRef.RestorePreferences();
		movRef.canMove = true;
		movRef.ResumePath();
		currState = States.Patrol;
	}
	#endregion
	private Transform Detect() {
		//Circle cast
		return Physics2D.CircleCast(transform.position, detectionRange, Vector2.zero, 0f, targetMask).transform;
	}

	private void OnDrawGizmos() {
		Gizmos.color = Color.red;
		Gizmos.DrawWireSphere(transform.position, detectionRange);
		Gizmos.color = Color.yellow;
		if (CurrentTarget != null) {
			Vector2 origin = CurrentTarget.position - transform.position;
			origin = (Vector2)transform.position + (origin.normalized * 2f);
			Gizmos.DrawWireSphere(origin, actionRange);
		}
	}

	public void BeginMatch() {
		StopAllCoroutines();
		OpenForMatch = true;
		StartCoroutine(CloseMatch());
	}

	public void Stun(float time) {
		movRef.Stop();
		CanAttack = false;
		movRef.canMove = false;
		auxStunTime = time;
		Stunned = true;
	}

	private void RecoverStun() {
		movRef.canMove = true;
		CanAttack = true;
		Stunned = false;
		movRef.ResumePath();
	}

	private IEnumerator CloseMatch() {
		yield return new WaitForSeconds(MatchRef.matchingTime);
		OpenForMatch = false;
	}

}
