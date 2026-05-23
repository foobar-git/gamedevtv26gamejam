using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireBallScript : MonoBehaviour {

    [SerializeField] private float destroyfireBallTime, destroyFireBallAnimTime, fireBallSpeed, fireBallTravelHeight, bounceTimer;
    [SerializeField] private bool isShootingFireBall, isBounceActive, shouldCheckBounce;
    private RaycastHit2D _frontRaycastHit, _backRaycastHit;
    public LayerMask groundLayer;
    private Animator _animator;
    private Renderer _meshRend;
    private ParticleSystem _ps;

    void Awake () {
        _animator = GetComponentInChildren<Animator> ();
        _ps = GetComponentInChildren<ParticleSystem> ();
        _meshRend = GetComponent<Renderer> ();
        isShootingFireBall = false;
        isBounceActive = true;
        shouldCheckBounce = true;
        fireBallSpeed = 5f;
        fireBallTravelHeight = 3f;
        destroyfireBallTime = 2f;
        destroyFireBallAnimTime = 0.3f;
        bounceTimer = 0.25f;
    }

	void Update () {
		CheckForObstacles ();
	}

	void FixedUpdate () {
        MoveFireBall ( fireBallSpeed, fireBallTravelHeight );
    }

    void CheckForObstacles () {
        _frontRaycastHit = Physics2D.Raycast (transform.position, Vector2.left, 0.2f, groundLayer);
        _backRaycastHit = Physics2D.Raycast (transform.position, Vector2.right, 0.2f, groundLayer);

        if (_frontRaycastHit || _backRaycastHit) {
            DestroyFireBall ( destroyFireBallAnimTime, 0f );
		}
	}

    void MoveFireBall (float s, float h) {
        if ( isShootingFireBall ) {
            Destroy (gameObject, destroyfireBallTime);
            if ( isBounceActive ) {
                transform.Translate ( s * Time.deltaTime, -h * Time.deltaTime, 0 );
            } else {
                transform.Translate ( s * Time.deltaTime, h * Time.deltaTime, 0 );
                if (shouldCheckBounce) {
                    StartCoroutine ( BounceSwitch ( bounceTimer ) );
                    shouldCheckBounce = false;
			    }
			}
        }
	}

    public void PlayerDirectionToFireBallSpeed (bool b, float f) {
        isShootingFireBall = b;
        fireBallSpeed *= f;
	}

    void DestroyFireBall (float t, float f) {
        _meshRend.enabled = false;
        _ps.Stop();
        _animator.Play ("FireBallExplosion");
        fireBallSpeed = f;
        Destroy (gameObject, t);
	}

    public float FireBallSpeed {
        get {
            return fireBallSpeed;
        }
        set {
            fireBallSpeed *= value;
        }
	}

    public bool ShootFireBall {
        get {
            return isShootingFireBall;
		}
        set {
            isShootingFireBall = value;
		}
	}

    void OnTriggerEnter2D (Collider2D other) {
        
        if ( other.gameObject.layer == LayerMask.NameToLayer ("Ground") ) {
            isBounceActive = !isBounceActive;
        }


        if (other.gameObject.tag == TagScript.ENEMY_TAG) {
            Debug.Log ("FireBall hit " + other.name);
            DestroyFireBall ( destroyFireBallAnimTime, fireBallSpeed / 2f );
		}
    }

    IEnumerator BounceSwitch (float t) {
        yield return new WaitForSeconds (t);
        isBounceActive = !isBounceActive;
        shouldCheckBounce = true;
	}

} // end of class
