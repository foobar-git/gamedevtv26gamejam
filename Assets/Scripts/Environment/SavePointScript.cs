using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SavePointScript : MonoBehaviour {

	private Animator animator;

	// Awake is used for initialization
    void Awake () {
		animator = GetComponentInChildren<Animator> ();
	}

	void OnTriggerEnter2D (Collider2D other) {		
		if ( other.gameObject.tag == TagScript.PlayerTag ) {
			Debug.Log ("SavePoint Flag");
			animator.SetBool ("flagWavingAnimParam", true);
		}
	}
	
	void OnTriggerExit2D (Collider2D other) {
		if ( other.gameObject.tag == TagScript.PlayerTag ) {
			animator.SetBool ("flagWavingAnimParam", false);
		}
	}

} // end of class
