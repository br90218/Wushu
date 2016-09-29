using UnityEngine;
using System.Collections;

public class EnableClimbing : MonoBehaviour {
	public GameObject player;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {

	}

	void OnCollisionStay(Collision col) {
		if (col.gameObject.name == player.name) {
			Debug.Log ("Hitting FPSController");
			col.gameObject.GetComponent<CharacterDetails> ().Climbing (true);
		} 
	}

	void OnCollisionExit(Collision col) {
		if (col.gameObject.name == player.name) {
			col.gameObject.GetComponent<CharacterDetails> ().Climbing (false);
		}
	}
}
