using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireBallScript : MonoBehaviour {

    [SerializeField] private float destroyfireBallTime, destroyFireBallAnimTime, fireBallSpeed, fireBallTravelHeight, bounceTimer;
    [SerializeField] private bool shootFireBall, bounceSwitch, checkBounce;
    private RaycastHit2D frontRaycastHit, backRaycastHit;
    public LayerMask groundLayer;
    private Animator animator;
    private Renderer meshRend;
    private ParticleSystem partSys;

    void Awake () {
        animator = GetComponentInChildren<Animator> ();
        partSys = GetComponentInChildren<ParticleSystem> ();
        meshRend = GetComponent<Renderer> ();
        shootFireBall = false;
        bounceSwitch = true;
        checkBounce = true;
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
        frontRaycastHit = Physics2D.Raycast (transform.position, Vector2.left, 0.2f, groundLayer);
        backRaycastHit = Physics2D.Raycast (transform.position, Vector2.right, 0.2f, groundLayer);

        if (frontRaycastHit || backRaycastHit) {
            DestroyFireBall ( destroyFireBallAnimTime, 0f );
		}
	}

    void MoveFireBall (float s, float h) {
        if ( shootFireBall ) {
            Destroy (gameObject, destroyfireBallTime);
            if ( bounceSwitch ) {
                transform.Translate ( s * Time.deltaTime, -h * Time.deltaTime, 0 );
            } else {
                transform.Translate ( s * Time.deltaTime, h * Time.deltaTime, 0 );
                if (checkBounce) {
                    StartCoroutine ( BounceSwitch ( bounceTimer ) );
                    checkBounce = false;
			    }
			}
        }
	}

    public void PlayerDirectionToFireBallSpeed (bool b, float f) {
        shootFireBall = b;
        fireBallSpeed *= f;
	}

    void DestroyFireBall (float t, float f) {
        meshRend.enabled = false;
        partSys.Stop();
        animator.Play ("FireBallExplosion");
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
            return shootFireBall;
		}
        set {
            shootFireBall = value;
		}
	}

    void OnTriggerEnter2D (Collider2D other) {
        
        if ( other.gameObject.layer == LayerMask.NameToLayer ("Ground") ) {
            bounceSwitch = !bounceSwitch;
        }


        if (other.gameObject.tag == TagScript.EnemyTag) {
            Debug.Log ("FireBall hit " + other.name);
            DestroyFireBall ( destroyFireBallAnimTime, fireBallSpeed / 2f );
		}
    }

    IEnumerator BounceSwitch (float t) {
        yield return new WaitForSeconds (t);
        bounceSwitch = !bounceSwitch;
        checkBounce = true;
	}

} // end of class
