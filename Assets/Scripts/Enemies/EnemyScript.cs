using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyScript : MonoBehaviour {

    [SerializeField] private float moveSpeed, playerPushForce, enemyBounceForce, raycastDistanceGround, raycastDistanceFront, raycastDistanceBack;
    [SerializeField] private bool isMovingLeft, enemyStunned, enemyHit, isEnemyFalling;

    private float _tempVelocity;
    private bool _isBounceKill, _stunnedByFireBall, _killedByFireBall;

    private RaycastHit2D _frontRaycastHit, _backRaycastHit, _groundRaycastHit, __groundRaycastHitFallDamage;
    public LayerMask playerLayer, groundLayer;
    private Vector2 _tempScaleVec;

    public Transform groundCheckPosition;
    private CircleCollider2D _enemyCircleCollider2D;
    private Rigidbody2D _enemyRb;
    private Animator _animator;
    public AudioClip fireBallHit, soundPlayerStunEnemy;
    private AudioScript _audioScript;
    
    public enum WithFireBallState	{ NeutralToFireBall, StunnedByFireBall, KilledByFireBall }
    public WithFireBallState withFireBallState;

    ////////////////////////////////////////////////////////////////////////////////////////

    // Awake is used for initialization
    void Awake () {
        _enemyRb = GetComponent<Rigidbody2D> ();
        _enemyCircleCollider2D = GetComponent<CircleCollider2D> ();
        _animator = GetComponent<Animator> ();
        _audioScript = GetComponent<AudioScript> ();

        enemyStunned = false;
        isEnemyFalling = false;
        _isBounceKill = false;

        enemyBounceForce = 3f;
        playerPushForce = 6f;
        raycastDistanceGround = 0.2f;
        raycastDistanceFront = 0.25f;
        raycastDistanceBack = 0.25f;
    }

    // Start is called before the first frame update
    void Start () {
        isMovingLeft = true;
        InitializeWithFireBallState (withFireBallState);
    }

    // Update is called once per frame
    void Update () {
        if ( !_isBounceKill ) {
            CheckForGroundBelow ();
            CheckForObstacle ();
            CheckForPlayer ();
        }
    }

    // FixedUpdate is called every couple of frames, used for physics
    void FixedUpdate () {
        MoveEnemy ( isMovingLeft, moveSpeed );
    }

    void CorrectColliderOffset (float x, float y) { // small correction because of sprite going off-center
        _enemyCircleCollider2D.offset = new Vector2 (x, y);
    }

    void InitializeWithFireBallState (WithFireBallState fS) {
        switch ( fS ) {
            case WithFireBallState.NeutralToFireBall:
				withFireBallState = fS;
				_killedByFireBall = false;
                _stunnedByFireBall = false;
				break;
            case WithFireBallState.StunnedByFireBall:
                withFireBallState = fS;
				_killedByFireBall = false;
                _stunnedByFireBall = !_killedByFireBall;
                break;
			case WithFireBallState.KilledByFireBall:
				withFireBallState = fS;
				_killedByFireBall = true;
                _stunnedByFireBall = !_killedByFireBall;
				break;
            default:
                withFireBallState = WithFireBallState.NeutralToFireBall;
                break;
        }
	}

    void ChangeEnemyDirection () {
        if ( !enemyStunned && !enemyHit ) {
            Debug.Log ("Enemy turns around!");
            isMovingLeft = !isMovingLeft;
            _tempScaleVec = transform.localScale;
            if (isMovingLeft) {
                _tempScaleVec.x = Mathf.Abs (_tempScaleVec.x);
            } else {
                _tempScaleVec.x = -Mathf.Abs (_tempScaleVec.x);
            }
            transform.localScale = _tempScaleVec;
        }
    }

    void MoveEnemy (bool ml, float ms) {
        if ( !enemyStunned ) {
            if ( ml ) {
                _enemyRb.linearVelocity = new Vector2 (-ms, _enemyRb.linearVelocity.y);
            } else {
                _enemyRb.linearVelocity = new Vector2 (ms, _enemyRb.linearVelocity.y);
            }
            _animator.Play ("EnemyMove");
        } else {
            _animator.Play ("EnemyStunned");
            CorrectColliderOffset ( 0f, _enemyCircleCollider2D.offset.y);
            //StartCoroutine ( DisableEnemy (disableObjectTime) );
            //Destroy (gameObject, destroyObjectTime);
        }
    }

    void PushEnemy (float pVx) {
        if ( !isEnemyFalling ) {
            Debug.Log ("Push!");
            _enemyRb.linearVelocity = new Vector2 ( pVx, _enemyRb.linearVelocity.y );
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
        
        _frontRaycastHit = Physics2D.Raycast (transform.position, Vector2.left, raycastDistanceFront, playerLayer);
        _backRaycastHit = Physics2D.Raycast (transform.position, Vector2.right, raycastDistanceBack, playerLayer);

        if ( _frontRaycastHit ) {
            if ( enemyStunned ) {
                PushEnemy (playerPushForce);
            } else {
                PlayerHurt ();
			}
		}
        
        if ( _backRaycastHit ) {
            if ( enemyStunned ) {
                PushEnemy (-playerPushForce);
            } else {
                PlayerHurt ();
			}
		}
	}

    void CheckForGroundBelow () {
        if ( !enemyStunned ) {
            _groundRaycastHit = Physics2D.Raycast (groundCheckPosition.position, Vector2.down, raycastDistanceGround, groundLayer);
            __groundRaycastHitFallDamage = Physics2D.Raycast (transform.position, Vector2.down, 2f, groundLayer);

            if ( !_groundRaycastHit ) {
                //Debug.Log ("No ground below!");
                ChangeEnemyDirection ();
		    }

            if ( !__groundRaycastHitFallDamage ) {
                Debug.Log ("Enemy fall damage!");
                isEnemyFalling = true;
                enemyStunned = true;
		    }
        }
	}
    
    void CheckForObstacle () {
        if ( !enemyStunned ) {
            if ( isMovingLeft ) _frontRaycastHit = Physics2D.Raycast (transform.position, Vector2.left, raycastDistanceFront + 0.25f, groundLayer);
            else _frontRaycastHit = Physics2D.Raycast (transform.position, Vector2.right, raycastDistanceFront + 0.25f, groundLayer);

            if ( _frontRaycastHit ) {
                Debug.Log ("Obstacle in front of enemy!");
                ChangeEnemyDirection ();
		    }
        }
	}

    public void BounceKillEnemy () {
        _isBounceKill = true;
        BounceEnemyUp (_enemyRb.linearVelocity.x, enemyBounceForce);
        _enemyRb.GetComponent<Collider2D> ().isTrigger = true;
        _enemyRb.transform.position = new Vector3 (_enemyRb.transform.position.x, _enemyRb.transform.position.y, -5f);
        //Destroy (gameObject, 0.3f);
        enemyHit = true;
	}

    void BounceEnemyUp (float pvx, float f) {
        _enemyRb.linearVelocity = new Vector2 (pvx, f);
	}

    public void EnemyPlayAudio (AudioClip sound) {
        _audioScript.PlayAudio (sound);
	}

    public void EnemPlayAudioWaitToFinishClip (AudioClip sound) {
        _audioScript.PlayAudioWaitToFinishClip (sound);
	}

    void OnTriggerEnter2D (Collider2D other) {
        if (other.gameObject.tag == TagScript.TURN_ENEMY_TAG) {
            if (!enemyStunned) ChangeEnemyDirection ();
        }

        if (other.gameObject.tag == TagScript.PLAYER_TAG) {
            _audioScript.PlayAudioWaitToFinishClip (soundPlayerStunEnemy);
        }

        if (other.gameObject.tag == TagScript.FIRE_BALL_TAG) {
            _audioScript.PlayAudio ( fireBallHit );
            if ( _stunnedByFireBall ) enemyStunned = true;
            if ( _killedByFireBall ) {
                if ( !enemyHit ) BounceKillEnemy ();
            }
            //else {
            //    Destroy (gameObject);
            //}
		}
    }

	void OnCollisionEnter2D (Collision2D other) {
		if (other.gameObject.tag == TagScript.ENEMY_TAG) {
            ChangeEnemyDirection ();
        }
    }

} // end of class