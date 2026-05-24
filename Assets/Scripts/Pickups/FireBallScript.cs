using System.Collections;
using UnityEngine;

// Controls a fired fireball — movement, parabolic bouncing, and collision response.
// Bouncing alternates between a falling phase and a rising phase on each ground contact.
public class FireBallScript : MonoBehaviour
{
    [SerializeField] private float destroyfireBallTime, destroyFireBallAnimTime, fireBallSpeed, fireBallTravelHeight, bounceTimer;
    [SerializeField] private bool isShootingFireBall, isBounceActive, shouldCheckBounce;
    private RaycastHit2D _frontRaycastHit, _backRaycastHit;
    public LayerMask groundLayer;
    private Animator _animator;
    private Renderer _meshRend;
    private ParticleSystem _ps;

    void Awake()
    {
        _animator = GetComponentInChildren<Animator>();
        _ps = GetComponentInChildren<ParticleSystem>();
        _meshRend = GetComponent<Renderer>();
        isShootingFireBall = false;
        isBounceActive = true;
        shouldCheckBounce = true;
        fireBallSpeed = 5f;
        fireBallTravelHeight = 3f;
        destroyfireBallTime = 2f;
        destroyFireBallAnimTime = 0.3f;
        bounceTimer = 0.25f;
    }

    void FixedUpdate()
    {
        MoveFireBall(fireBallSpeed, fireBallTravelHeight);
        CheckForObstacles();
    }

    // checks both directions because fireball direction can be either left or right
    void CheckForObstacles()
    {
        _frontRaycastHit = Physics2D.Raycast(transform.position, Vector2.left, 0.2f, groundLayer);
        _backRaycastHit = Physics2D.Raycast(transform.position, Vector2.right, 0.2f, groundLayer);

        if (_frontRaycastHit || _backRaycastHit)
        {
            DestroyFireBall(destroyFireBallAnimTime, 0f);
        }
    }

    void MoveFireBall(float s, float h)
    {
        if (isShootingFireBall)
        {
            // isBounceActive: falling phase (negative Y); else: rising phase (positive Y)
            // shouldCheckBounce prevents BounceSwitch from being re-started every FixedUpdate during the rising phase
            if (isBounceActive)
            {
                transform.Translate(s * Time.deltaTime, -h * Time.deltaTime, 0);
            }
            else
            {
                transform.Translate(s * Time.deltaTime, h * Time.deltaTime, 0);
                if (shouldCheckBounce)
                {
                    StartCoroutine(BounceSwitch(bounceTimer));
                    shouldCheckBounce = false;
                }
            }
        }
    }

    // called once on spawn — f is the player's direction multiplier (1 or -1), applied to speed to set travel direction
    public void PlayerDirectionToFireBallSpeed(bool b, float f)
    {
        isShootingFireBall = b;
        fireBallSpeed *= f;
        Destroy(gameObject, destroyfireBallTime);
    }

    void DestroyFireBall(float t, float f)
    {
        _meshRend.enabled = false;
        _ps.Stop();
        _animator.Play("FireBallExplosion");
        // keep moving at reduced speed during the short explosion animation (renderer is off but MoveFireBall still runs)
        fireBallSpeed = f;
        Destroy(gameObject, t);
    }

    public float FireBallSpeed
    {
        get { return fireBallSpeed; }
        set { fireBallSpeed *= value; }
    }

    public bool ShootFireBall
    {
        get { return isShootingFireBall; }
        set { isShootingFireBall = value; }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // layer-based check so any ground-layered geometry triggers a bounce, not just tagged objects
        if (other.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            isBounceActive = !isBounceActive;
        }

        if (other.gameObject.CompareTag(TagScript.ENEMY_TAG))
        {
            Debug.Log("FireBall hit " + other.name);
            DestroyFireBall(destroyFireBallAnimTime, fireBallSpeed / 2f);
        }
    }

    IEnumerator BounceSwitch(float t)
    {
        yield return new WaitForSeconds(t);
        isBounceActive = !isBounceActive;
        shouldCheckBounce = true;
    }

} // end of class
