using UnityEngine;
using System.Collections;

public class NPCDetails : MonoBehaviour {
	[Tooltip("The maximum health allowed for this being (Unit)")] 
	public int maximumHealth = 100;
	[Tooltip("The healing speed of this being (Unit/sec)")] 
	public int recoverSpeed = 5; 
	private float health;
	private Animator animator;
	private AnimatorStateInfo currentHealthState;
	private AnimatorStateInfo currentBattleState;
	private float distance;
	private bool isInBattle = false;
	// Use this for initialization
	void Start () {
		health = maximumHealth;
		animator = GetComponent<Animator> ();
	}

	// Update is called once per frame
	void Update () {
		currentHealthState = animator.GetCurrentAnimatorStateInfo (0);
		currentBattleState = animator.GetCurrentAnimatorStateInfo (1);
		int currHealthStateHash = currentHealthState.fullPathHash;
		int currBattleStateHash = currentBattleState.fullPathHash;
		distance = Vector3.Distance (transform.position, GameObject.FindWithTag("Player").transform.position); //This is something to be changed. Too speicific.

		animator.SetBool ("isHurt", (health >= maximumHealth ? false : true));
		animator.SetBool ("isInBattle", isInBattle);
		animator.SetFloat ("distance", distance);

		if (currHealthStateHash == Animator.StringToHash ("Health.Idle")) {
		} else if (currHealthStateHash == Animator.StringToHash ("Health.Panicking")) {
		} else if (currHealthStateHash == Animator.StringToHash ("Health.Healing")) {
			Heal ();
		} else if (currHealthStateHash == Animator.StringToHash ("Health.Death")) {
			Destroy (gameObject);
		}

		if (currBattleStateHash == Animator.StringToHash ("Battle.Idle")) {
			isInBattle = false;
		} else if (currBattleStateHash == Animator.StringToHash ("Battle.In Battle")) {
			isInBattle = true;
			if (distance > 30f) {
				StartCoroutine (Escape ());
			}
		}



		if (health <= 0f) {
			StartCoroutine (triggerSetReset ("die"));
		}
		
	}
		
	public void Hurt(float damage) {
		health -= damage;
		StartCoroutine (triggerSetReset ("hurt"));
	}

	/// <summary>
	/// Heals this being with its designated recovery speed.
	/// </summary>
	public void Heal() {
		health = Mathf.Min (maximumHealth, health + Time.deltaTime * recoverSpeed);
	}

	/// <summary>
	/// Heals this being immediately by a value.
	/// Maybe applicable when using a potion or healing spell.
	/// </summary>
	/// <param name="instant">healing value</param>
	public void Heal(int instant) { 
		health += instant;
	}

	public void Attack() {
		StartCoroutine (triggerSetReset ("attack"));
	}

	IEnumerator Escape() {
		yield return new WaitForSeconds (5f);
		animator.SetTrigger ("battleTimeOut");
		yield return new WaitForSeconds (0.5f);
		animator.ResetTrigger ("battleTimeOut");
	}

	IEnumerator triggerSetReset(string triggerName) {
		animator.SetTrigger (triggerName);
		yield return new WaitForSeconds (0.5f);
		animator.ResetTrigger (triggerName);
	}
		
}
