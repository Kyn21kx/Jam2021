﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Projectile : MonoBehaviour {

	#region Variables
	private Rigidbody2D rig;
	private Shooting shootingRef;
	private AI firingAI;

	public Vector2 dir;
	
	public float collisionRadius;
	public float travelSpeed;
	private float cntr = 0f;
	private float travelDistance;
	private bool canDetect;
	#endregion

	public void Initiate(Vector2 dir, float travelSpeed, Shooting shootingRef, float travelDistance) {
		transform.parent = null;
		this.canDetect = true;
		this.dir = dir;
		this.travelSpeed = travelSpeed;
		this.shootingRef = shootingRef;
		this.travelDistance = travelDistance;
		rig = GetComponent<Rigidbody2D>();
		float rotation = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
		transform.rotation = Quaternion.Euler(0f, 0f, rotation - 90f);
	}

	public void Initiate(Vector2 dir, float travelSpeed, AI firingAI, float travelDistance) {
		transform.parent = null;
		this.canDetect = true;
		this.dir = dir;
		this.travelSpeed = travelSpeed;
		this.firingAI = firingAI;
		this.travelDistance = travelDistance;
		rig = GetComponent<Rigidbody2D>();
		float rotation = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
		transform.rotation = Quaternion.Euler(0f, 0f, rotation - 90f);
	}

	//Destroy after a period of time
	private void FixedUpdate() {
		float mTime = travelDistance / travelSpeed;
		cntr += Time.fixedDeltaTime;
		//transform.Translate(dir * realSpeed);
		if (Detect())
			Destroy(gameObject);
		if (cntr < mTime) {
			rig.position = Vector2.Lerp(rig.position, rig.position + dir, Time.fixedDeltaTime * travelSpeed);
		}
		else {
			rig.velocity = Vector2.zero;
			canDetect = false;
			if (cntr >= mTime + 1f)
				Destroy(gameObject);
		}
	}

	private void OnDrawGizmos() {
		Gizmos.color = Color.green;
		Gizmos.DrawWireSphere(transform.position, collisionRadius);
	}

	private bool Detect() {
		if (!canDetect) return false;
		LayerMask targetMask;
		if (shootingRef == null && !firingAI.lover) {
			int playerMask = 1 << 9;
			int loverLayer = 1 << 10;
			targetMask = playerMask | loverLayer;

		}
		else {
			int hatersMask = 1 << 8;
			targetMask = hatersMask;
		}
		RaycastHit2D hit = Physics2D.CircleCast(rig.position, collisionRadius, Vector2.zero, 0f, targetMask);
		if (hit.transform != null) {
			AI aiRef;
			if (shootingRef == null) {
				if (firingAI.lover) {
					aiRef = hit.transform.GetComponent<AI>();
					if (aiRef != null) {
						aiRef.OverrideTarget(firingAI.transform);
						Utilities.gameManager.Buffer(aiRef, directBuffer: firingAI);
					}
				}
				else {
					//Make case for hitting a lover
					if (!hit.transform.CompareTag("Player")) {
						aiRef = hit.transform.GetComponent<AI>();
						aiRef.rangedConversion = 2f;
					}
					else
						Utilities.playerRef.GetComponent<HealthManager>().Damage();
				}
				return true;
			}
			//Send the player a reference to the hater object
			aiRef = hit.transform.GetComponent<AI>();
			//Override the target to the player
			aiRef.OverrideTarget(shootingRef.transform);
			Utilities.gameManager.Buffer(aiRef);
			return true;

		}
		return false;
	}

}
