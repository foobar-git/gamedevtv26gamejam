using System.Collections;
using UnityEngine;

// Handles a pickup item — optional wandering movement, obstacle avoidance,
// and collection by the player. Movement starts inactive until BrickScript enables it.
public class PickupScript : MonoBehaviour
{
    private SpriteRenderer _spriteRenderer;
    private Collider2D _thisCollider;
    private Rigidbody2D _pickupRb;
    private Animator _animator;
    private AudioScript _audioScript;
    public AudioClip soundPickup, soundCoin, sound1up;
    public GameObject pickupParticles;
    private GameObject _newPickupParticlesAnim;

    private RaycastHit2D _frontRaycastHit;
    public LayerMask collisionLayer;
    private Vector3 _newPosition;

    public float moveSpeed;
    // set to true by BrickScript after the brick animation completes — pickup stays still until then
    public bool isMovingPickup;

    [SerializeField] private float raycastDistanceFront;
    [SerializeField] private bool isMovingLeft;

    ////////////////////////////////////////////////////////////////////////////////////////

    void Awake()
    {
        _pickupRb = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
        _audioScript = GetComponent<AudioScript>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _thisCollider = GetComponent<Collider2D>();
        // randomize initial direction so pickups don't always walk the same way
        isMovingLeft = RandomValue();
        raycastDistanceFront = 0.5f;
    }

    void Update()
    {
        CheckForObstacle();
    }

    void FixedUpdate()
    {
        MovePickup(isMovingLeft, moveSpeed);
    }

    bool RandomValue()
    {
        return (Random.value > 0.5f);
    }

    void MovePickup(bool ml, float ms)
    {
        if (!isMovingPickup)
        {
            return;
        }

        if (ml)
        {
            _pickupRb.linearVelocity = new Vector2(-ms, _pickupRb.linearVelocity.y);
        }
        else
        {
            _pickupRb.linearVelocity = new Vector2(ms, _pickupRb.linearVelocity.y);
        }
    }

    public void ChangePickupMoveDirection()
    {
        Debug.Log("Pickup turns around!");
        isMovingLeft = !isMovingLeft;
    }

    void CheckForObstacle()
    {
        if (!isMovingPickup)
        {
            return;
        }

        if (isMovingLeft)
        {
            _frontRaycastHit = Physics2D.Raycast(transform.position, Vector2.left, raycastDistanceFront, collisionLayer);
        }
        else
        {
            _frontRaycastHit = Physics2D.Raycast(transform.position, Vector2.right, raycastDistanceFront, collisionLayer);
        }

        if (_frontRaycastHit)
        {
            Debug.Log("Obstacle in front of Pickup item!");
            ChangePickupMoveDirection();
        }
    }

    void DisableGameObject(bool b, AudioClip a)
    {
        // hide and disable immediately; Destroy is delayed so the audio clip plays to completion
        _audioScript.PlayAudio(a);
        _spriteRenderer.enabled = !b;
        _thisCollider.enabled = !b;
    }

    void SpawnPickupParticles()
    {
        // z - 5 renders particles in front of the pickup sprite
        _newPosition = new Vector3(transform.position.x, transform.position.y, transform.position.z - 5f);
        _newPickupParticlesAnim = Instantiate(pickupParticles, _newPosition, Quaternion.identity);
        _newPickupParticlesAnim.GetComponent<ParticleSystem>().Play();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag(TagScript.PLAYER_TAG))
        {
            if (this.gameObject.CompareTag(TagScript.PICKUP_COIN_TAG))
            {
                SpawnPickupParticles();
                DisableGameObject(true, soundCoin);
            }
            else if (this.gameObject.CompareTag(TagScript.PICKUP_1UP_TAG))
            {
                DisableGameObject(true, sound1up);
            }
            else
            {
                DisableGameObject(true, soundPickup);
            }
            Destroy(gameObject, _audioScript.audioSource.clip.length);
        }
    }

} // end of class
