using UnityEngine;
using System.Collections;

public class ProjectileParticleDespawn : MonoBehaviour {
	private ParticleSystem self;
	// Use this for initialization
	void Awake () {
		self = GetComponent<ParticleSystem> ();
	}
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
		if (transform.parent == null) {
			var em = self.emission;
			em.enabled = false;	
			if (self.isStopped)
				Destroy (gameObject);
		}
	}
}
