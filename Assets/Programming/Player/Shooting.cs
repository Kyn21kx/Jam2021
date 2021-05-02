using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shooting : MonoBehaviour {

	#region Variables
	public AI personBuffer;
	[SerializeField]
	private GameObject projectile;
	[SerializeField]
	private GameObject arrowIndicator;
	private Camera cam;

	[SerializeField]
	private float shootingSpeed;
	[SerializeField]
	private float cooldown;
	private float holdTime;
	private float auxCooldown;
	private bool onCooldown;
	public bool holding;
	#endregion

	private void Start() {
		arrowIndicator.SetActive(false);
		cam = Camera.main;
		personBuffer = null;
		holdTime = 0f;
		holding = false;
		auxCooldown = cooldown;
		onCooldown = false;
	}

	private void Update() {
		if (Utilities.gameManager.Paused) return;
		//TODO: Add cooldown
		HoldShoot();
		Cooldown();
	}

	private void Cooldown() {
		if (onCooldown) {
			cooldown -= Time.deltaTime;
			if (cooldown <= 0f) {
				cooldown = auxCooldown;
				onCooldown = false;
			}
		}
	}

	private void HoldShoot() {
		holdTime = Mathf.Clamp(holdTime, 0f, 2f);
		float dis = (holdTime * (35f / 1.5f)) + 10f;
		if (Input.GetMouseButtonDown(0) && !holding)
			holding = true;

		if (holding && Input.GetMouseButtonUp(0)) {
			//d = x^(1.5f) - (x/2)
			//Graphics to show it
			arrowIndicator.SetActive(false);
			holding = false;
			holdTime = 0f;
			Utilities.anim.SetBool("Holding", false);
			Shoot(dis);
		}
		//Increment x 
		if (holding) {
			holdTime += Time.deltaTime;
			Vector2 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
			DisplayAimGuide(mousePos, dis);
			float xValue = mousePos.x - transform.position.x;
			if (xValue > 0) {
				GetComponent<SpriteRenderer>().flipX = true;
			}
			else {
				GetComponent<SpriteRenderer>().flipX = false;
			}
			Utilities.anim.SetBool("Holding", true);
		}
	}

	public void BufferPerson(AI personRef) {
		personRef.BeginMatch();
		if (personBuffer == null) {
			//Stun the person for a couple of seconds
			personBuffer = personRef;
			//Send the information to the UI
		}
		else {
			//Pair them
			if (ValidTargets(personRef, personBuffer)) {
				var match1 = personRef.MatchRef;
				var match2 = personBuffer.MatchRef;

				if (match1.Match(match2)) {
					match1.Pair(match2);
					//Clean them and reset their states
					personRef.movRef.canMove = true;
					personRef.movRef.ResumePath();
					
					personBuffer.movRef.canMove = true;
					personBuffer.movRef.ResumePath();
					
					personRef.StopAllCoroutines();
					personBuffer.StopAllCoroutines();
					personBuffer = null;
				}
				else {
					//Make 'em go crazy
					match1.AI_Ref.ResetNode();
					match1.AI_Ref.currState = AI.States.Nutz;

					match2.AI_Ref.ResetNode();
					match2.AI_Ref.currState = AI.States.Nutz;
					personBuffer = null;
				}
			
			}
		}
	}

	private bool ValidTargets(AI t1, AI t2) {
		return t1 != t2 && t1.OpenForMatch && t2.OpenForMatch && !t1.MatchRef.Paired && !t2.MatchRef.Paired;
	}

	private void Shoot(float distance) {
		if (!onCooldown) {
			//Instantiate the prefab to the mouse's position
			Vector2 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
			Vector2 dir = mousePos - (Vector2)transform.position;
			var instance = Instantiate(projectile, transform.position, Quaternion.identity);
			Projectile projInstance = instance.GetComponent<Projectile>();
			//t = v / d
			projInstance.Initiate(dir.normalized, shootingSpeed, this, distance);
			onCooldown = true;
		}
	}

	private void DisplayAimGuide(Vector2 mousePos, float mag) {
		Vector2 dir = (mousePos - (Vector2)transform.position).normalized * mag;
		//Get the angle for the mouse as tan^-1 (y / x)
		float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
		//Put it in the position of the thing
		arrowIndicator.SetActive(true);
		arrowIndicator.transform.position =(Vector3)dir + transform.position;
		arrowIndicator.transform.rotation = Quaternion.Euler(0, 0, angle - 90f);
	}

}
