using UnityEngine;
using System.Collections;
using UnityStandardAssets.Characters.FirstPerson;
/// <summary>
/// Class used to maintain all character details, including health, stamina, and also
/// passes functions to FPSControllers.
/// </summary>
public class CharacterDetails : MonoBehaviour {

	private FirstPersonController controller;

	// Use this for initialization
	void Start () {
		controller = GameObject.FindObjectOfType<FirstPersonController> ();
	}
	
	// Update is called once per frame
	void Update () {
	}

	public void Climbing(bool enable){
		controller.EnableClimbing (enable);
	}

}
