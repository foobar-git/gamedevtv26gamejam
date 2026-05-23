using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SavePointScript : MonoBehaviour {

	private Animator _animator;

	// Awake is used for initialization
    void Awake () {
		_animator = GetComponentInChildren<Animator> ();
	}

	void OnTriggerEnter2D (Collider2D other) {
		if ( other.gameObject.tag == TagScript.PLAYER_TAG ) {
			Debug.Log ("SavePoint Flag");
			_animator.SetBool ("flagWavingAnimParam", true);
		}
	}

	void OnTriggerExit2D (Collider2D other) {
		if ( other.gameObject.tag == TagScript.PLAYER_TAG ) {
			_animator.SetBool ("flagWavingAnimParam", false);
		}
	}

} // end of class
