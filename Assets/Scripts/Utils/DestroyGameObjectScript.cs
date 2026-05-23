using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyGameObjectScript : MonoBehaviour {
	
	void OnTriggerEnter2D (Collider2D other) {
		if ( other.gameObject.tag != TagScript.PLAYER_TAG ) {
			Destroy (other.gameObject);
		}
	}

} // end of class
