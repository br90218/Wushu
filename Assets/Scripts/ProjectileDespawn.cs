using UnityEngine;
using System.Collections;

public class ProjectileDespawn : MonoBehaviour {
	private GameObject whoosh;
	// Use this for initialization
	void Awake() {
		whoosh = transform.Find ("particle").gameObject;
	}

	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnCollisionExit(){
		whoosh.transform.SetParent (null);
		Destroy (this.gameObject);
	}
}
