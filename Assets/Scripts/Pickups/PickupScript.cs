using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;

public class PickupScript : MonoBehaviour {

    private SpriteRenderer spriteRenderer;
    private Collider2D thisCollider;
    private Rigidbody2D pickupBody;
    private Animator animator;
    private AudioScript audioScript;
    public AudioClip soundPickup, soundCoin, sound1up;
    public GameObject pickupParticles;
    private GameObject newPickupParticlesAnim;

    private RaycastHit2D frontRaycastHit;
    public LayerMask collisionLayer;
    private Vector3 newPosition;

    public float moveSpeed;
    public bool movePickup;

    [SerializeField] private float raycastDistanceFront;
    [SerializeField] private bool moveLeft;

    ////////////////////////////////////////////////////////////////////////////////////////
    
    void Awake () {
        pickupBody = GetComponent<Rigidbody2D> ();
        animator = GetComponent<Animator> ();
        audioScript = GetComponent<AudioScript> ();
        spriteRenderer = GetComponent<SpriteRenderer> ();
        thisCollider = GetComponent<Collider2D> ();
        //moveLeft = true;
        moveLeft = RandomValue ();
        raycastDistanceFront = 0.5f;
    }

	void Update () {
		if ( movePickup ) CheckForObstacle ();
	}

	void FixedUpdate () {
		if ( movePickup ) MovePickup (moveLeft, moveSpeed);
	}

    bool RandomValue () {
        return (Random.value > 0.5f);
	}

    void MovePickup (bool ml, float ms) {
        if ( ml ) {
            pickupBody.linearVelocity = new Vector2 (-ms, pickupBody.linearVelocity.y);
        } else {
            pickupBody.linearVelocity = new Vector2 (ms, pickupBody.linearVelocity.y);
        }
	}

    public void ChangePickupMoveDirection () {
        Debug.Log ("Pickup turns around!");
        moveLeft = !moveLeft;
	}

    void CheckForObstacle () {
        if ( moveLeft ) frontRaycastHit = Physics2D.Raycast (transform.position, Vector2.left, raycastDistanceFront, collisionLayer);
        else frontRaycastHit = Physics2D.Raycast (transform.position, Vector2.right, raycastDistanceFront, collisionLayer);

        if ( frontRaycastHit ) {
            Debug.Log ("Obstacle in front of Pickup item!");
            ChangePickupMoveDirection ();
		}
	}

	void DisableGameObject (bool b, AudioClip a) {
        audioScript.PlayAudio (a);
        spriteRenderer.enabled = !b;
        thisCollider.enabled = !b;
	}

    void SpawnPickupParticles () {
        newPosition = new Vector3 (transform.position.x, transform.position.y, transform.position.z + -5f);
		newPickupParticlesAnim = Instantiate (pickupParticles, newPosition, Quaternion.identity);
		newPickupParticlesAnim.GetComponent<ParticleSystem> ().Play ();
		//Destroy (newPickupParticlesAnim, 2f);
	}

	void OnTriggerEnter2D (Collider2D other) {
		if ( other.gameObject.tag == TagScript.PlayerTag ) {
            if ( this.gameObject.tag == TagScript.PickupCoinTag ) {
                SpawnPickupParticles ();
                DisableGameObject (true, soundCoin);
            }
            else if ( this.gameObject.tag == TagScript.Pickup1upTag ) DisableGameObject (true, sound1up);
            else DisableGameObject (true, soundPickup);
            Destroy (gameObject, audioScript.audioSource.clip.length);
		}
	}

} // end of class
