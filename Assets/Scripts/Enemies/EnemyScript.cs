using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyScript : MonoBehaviour {

    [SerializeField] private float moveSpeed, playerPushForce, enemyBounceForce, raycastDistanceGround, raycastDistanceFront, raycastDistanceBack;
    [SerializeField] private bool moveLeft, enemyStunned, enemyHit, enemyFalling;

    private float tempVelocity;
    private bool bounceKill, stunnedByFireBall, killedByFireBall;

    private RaycastHit2D frontRaycastHit, backRaycastHit, groundRaycastHit, groundRaycastHitFallDamage;
    public LayerMask playerLayer, groundLayer;
    private Vector2 tempScale;

    public Transform groundCheckPosition;
    private CircleCollider2D enemyCircleCollider2D;
    private Rigidbody2D enemyBody;
    private Animator animator;
    public AudioClip fireBallHit, soundPlayerStunEnemy;
    private AudioScript audioScript;
    
    public enum WithFireBallState	{ NeutralToFireBall, StunnedByFireBall, KilledByFireBall }
    public WithFireBallState withFireBallState;

    ////////////////////////////////////////////////////////////////////////////////////////

    // Awake is used for initialization
    void Awake () {
        enemyBody = GetComponent<Rigidbody2D> ();
        enemyCircleCollider2D = GetComponent<CircleCollider2D> ();
        animator = GetComponent<Animator> ();
        audioScript = GetComponent<AudioScript> ();

        enemyStunned = false;
        enemyFalling = false;
        bounceKill = false;

        enemyBounceForce = 3f;
        playerPushForce = 6f;
        raycastDistanceGround = 0.2f;
        raycastDistanceFront = 0.25f;
        raycastDistanceBack = 0.25f;
    }

    // Start is called before the first frame update
    void Start () {
        moveLeft = true;
        InitializeWithFireBallState (withFireBallState);
    }

    // Update is called once per frame
    void Update () {
        if ( !bounceKill ) {
            CheckForGroundBelow ();
            CheckForObstacle ();
            CheckForPlayer ();
        }
    }

    // FixedUpdate is called every couple of frames, used for physics
    void FixedUpdate () {
        MoveEnemy ( moveLeft, moveSpeed );
    }

    void CorrectColliderOffset (float x, float y) { // small correction because of sprite going off-center
        enemyCircleCollider2D.offset = new Vector2 (x, y);
    }

    void InitializeWithFireBallState (WithFireBallState fS) {
        switch ( fS ) {
            case WithFireBallState.NeutralToFireBall:
				withFireBallState = fS;
				killedByFireBall = false;
                stunnedByFireBall = false;
				break;
            case WithFireBallState.StunnedByFireBall:
                withFireBallState = fS;
				killedByFireBall = false;
                stunnedByFireBall = !killedByFireBall;
                break;
			case WithFireBallState.KilledByFireBall:
				withFireBallState = fS;
				killedByFireBall = true;
                stunnedByFireBall = !killedByFireBall;
				break;
            default:
                withFireBallState = WithFireBallState.NeutralToFireBall;
                break;
        }
	}

    void ChangeEnemyDirection () {
        if ( !enemyStunned && !enemyHit ) {
            Debug.Log ("Enemy turns around!");
            moveLeft = !moveLeft;
            tempScale = transform.localScale;
            if (moveLeft) {
                tempScale.x = Mathf.Abs (tempScale.x);
            } else {
                tempScale.x = -Mathf.Abs (tempScale.x);
            }
            transform.localScale = tempScale;
        }
    }

    void MoveEnemy (bool ml, float ms) {
        if ( !enemyStunned ) {
            if ( ml ) {
                enemyBody.linearVelocity = new Vector2 (-ms, enemyBody.linearVelocity.y);
            } else {
                enemyBody.linearVelocity = new Vector2 (ms, enemyBody.linearVelocity.y);
            }
            animator.Play ("EnemyMove");
        } else {
            animator.Play ("EnemyStunned");
            CorrectColliderOffset ( 0f, enemyCircleCollider2D.offset.y);
            //StartCoroutine ( DisableEnemy (disableObjectTime) );
            //Destroy (gameObject, destroyObjectTime);
        }
    }

    void PushEnemy (float pVx) {
        if ( !enemyFalling ) {
            Debug.Log ("Push!");
            enemyBody.linearVelocity = new Vector2 ( pVx, enemyBody.linearVelocity.y );
        }
    }

    void PlayerHurt () {
        Debug.Log ("Player hurt!");
	}

    public bool EnemyStunned {
        get {
            return enemyStunned;
		}
        set {
            enemyStunned = value;
		}
	}

    void CheckForPlayer () {
        
        frontRaycastHit = Physics2D.Raycast (transform.position, Vector2.left, raycastDistanceFront, playerLayer);
        backRaycastHit = Physics2D.Raycast (transform.position, Vector2.right, raycastDistanceBack, playerLayer);

        if ( frontRaycastHit ) {
            if ( enemyStunned ) {
                PushEnemy (playerPushForce);
            } else {
                PlayerHurt ();
			}
		}
        
        if ( backRaycastHit ) {
            if ( enemyStunned ) {
                PushEnemy (-playerPushForce);
            } else {
                PlayerHurt ();
			}
		}
	}

    void CheckForGroundBelow () {
        if ( !enemyStunned ) {
            groundRaycastHit = Physics2D.Raycast (groundCheckPosition.position, Vector2.down, raycastDistanceGround, groundLayer);
            groundRaycastHitFallDamage = Physics2D.Raycast (transform.position, Vector2.down, 2f, groundLayer);

            if ( !groundRaycastHit ) {
                //Debug.Log ("No ground below!");
                ChangeEnemyDirection ();
		    }

            if ( !groundRaycastHitFallDamage ) {
                Debug.Log ("Enemy fall damage!");
                enemyFalling = true;
                enemyStunned = true;
		    }
        }
	}
    
    void CheckForObstacle () {
        if ( !enemyStunned ) {
            if ( moveLeft ) frontRaycastHit = Physics2D.Raycast (transform.position, Vector2.left, raycastDistanceFront + 0.25f, groundLayer);
            else frontRaycastHit = Physics2D.Raycast (transform.position, Vector2.right, raycastDistanceFront + 0.25f, groundLayer);

            if ( frontRaycastHit ) {
                Debug.Log ("Obstacle in front of enemy!");
                ChangeEnemyDirection ();
		    }
        }
	}

    public void BounceKillEnemy () {
        bounceKill = true;
        BounceEnemyUp (enemyBody.linearVelocity.x, enemyBounceForce);
        enemyBody.GetComponent<Collider2D> ().isTrigger = true;
        enemyBody.transform.position = new Vector3 (enemyBody.transform.position.x, enemyBody.transform.position.y, -5f);
        //Destroy (gameObject, 0.3f);
        enemyHit = true;
	}

    void BounceEnemyUp (float pvx, float f) {
        enemyBody.linearVelocity = new Vector2 (pvx, f);
	}

    public void EnemyPlayAudio (AudioClip sound) {
        audioScript.PlayAudio (sound);
	}

    public void EnemPlayAudioWaitToFinishClip (AudioClip sound) {
        audioScript.PlayAudioWaitToFinishClip (sound);
	}

    void OnTriggerEnter2D (Collider2D other) {
        if (other.gameObject.tag == TagScript.TurnEnemyTag) {
            if (!enemyStunned) ChangeEnemyDirection ();
        }

        if (other.gameObject.tag == TagScript.PlayerTag) {
            audioScript.PlayAudioWaitToFinishClip (soundPlayerStunEnemy);
        }

        if (other.gameObject.tag == TagScript.FireBallTag) {
            audioScript.PlayAudio ( fireBallHit );
            if ( stunnedByFireBall ) enemyStunned = true;
            if ( killedByFireBall ) {
                if ( !enemyHit ) BounceKillEnemy ();
            }
            //else {
            //    Destroy (gameObject);
            //}
		}
    }

	void OnCollisionEnter2D (Collision2D other) {
		if (other.gameObject.tag == TagScript.EnemyTag) {
            ChangeEnemyDirection ();
        }
    }

} // end of class