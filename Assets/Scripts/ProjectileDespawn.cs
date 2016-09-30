using UnityEngine;
using System.Collections;

public class ProjectileDespawn : MonoBehaviour {
	private GameObject whoosh;
	// Use this for initialization
	public float damage = 0;
	void Awake() {
		whoosh = transform.Find ("particle").gameObject;
	}

	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnCollisionEnter(Collision col) {
		if (col.gameObject.tag == "Character") {
			Debug.Log ("Hit a character");
			col.gameObject.GetComponentInParent<NPCDetails> ().Hurt (damage);
		}
			
		whoosh.transform.SetParent (null);
		Destroy (this.gameObject);
	}
}
