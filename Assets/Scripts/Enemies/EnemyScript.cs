using System.Collections;
using UnityEngine;

// Controls an enemy — patrol movement, ground-edge and obstacle detection,
// player interaction (stomp stun, fireball reaction), bounce-kill on brick bump,
// optional flying mode (gravity off, no ledge detection), and directional projectile shooting.
public class EnemyScript : MonoBehaviour
{
    // configured per-enemy type in the Inspector — determines fireball reaction (none, stun, or kill)
    public enum WithFireBallState { NeutralToFireBall, StunnedByFireBall, KilledByFireBall }
    public enum ShootDirection { Left, Right, Up, Down }

    [SerializeField] private float moveSpeed, playerPushForce, enemyBounceForce, raycastDistanceGround, raycastDistanceFront, raycastDistanceBack;
    [SerializeField] private bool isMovingLeft, enemyStunned, enemyHit, isEnemyFalling;

    [Header("Flying")]
    [SerializeField] private bool isFlying;

    [Header("Shooting")]
    [SerializeField] private ShootDirection shootDirection;
    [SerializeField] private float shootInterval;
    [Tooltip("0 = pure gravity drop — no initial velocity, gravity does the work")]
    [SerializeField] private float shootSpeed;
    [SerializeField] private GameObject gameObjectProjectilePrefab;

    private bool _isBounceKill, _stunnedByFireBall, _killedByFireBall;
    private float _shootTimer;

    private RaycastHit2D _frontRaycastHit, _backRaycastHit, _groundRaycastHit, _groundRaycastHitFallDamage;
    public LayerMask playerLayer, groundLayer;
    private Vector2 _tempScaleVec;

    public Transform groundCheckTransform;
    private CircleCollider2D _enemyCircleCollider2D;
    private Rigidbody2D _enemyRb;
    private Animator _animator;
    public AudioClip soundFireBallHit, soundPlayerStunEnemy;
    private AudioScript _audioScript;

    public WithFireBallState withFireBallState;

    ////////////////////////////////////////////////////////////////////////////////////////

    void Awake()
    {
        _enemyRb = GetComponent<Rigidbody2D>();
        _enemyCircleCollider2D = GetComponent<CircleCollider2D>();
        _animator = GetComponent<Animator>();
        _audioScript = GetComponent<AudioScript>();

        if (isFlying)
        {
            _enemyRb.gravityScale = 0f;
        }

        enemyStunned = false;
        isEnemyFalling = false;
        _isBounceKill = false;

        enemyBounceForce = 3f;
        playerPushForce = 6f;
        raycastDistanceGround = 0.2f;
        raycastDistanceFront = 0.25f;
        raycastDistanceBack = 0.25f;

        _shootTimer = shootInterval;
    }

    void Start()
    {
        isMovingLeft = true;
        InitializeWithFireBallState(withFireBallState);
    }

    void Update()
    {
        TickEnemyTimers();
    }

    void FixedUpdate()
    {
        UpdateEnemyPhysics();
    }

    // timers only — no physics here
    void TickEnemyTimers()
    {
        if (_isBounceKill)
        {
            return;
        }
        TickShootTimer();
    }

    // all raycasts and movement in one place
    void UpdateEnemyPhysics()
    {
        if (_isBounceKill)
        {
            return;
        }
        MoveEnemy(isMovingLeft, moveSpeed);
        // flying enemies have no ledge to fall off — skip ground detection entirely
        if (!isFlying)
        {
            CheckForGroundBelow();
        }
        CheckForObstacle();
        CheckForPlayer();
    }

    void TickShootTimer()
    {
        // shootInterval = 0 means shooting is disabled for this enemy
        if (enemyStunned || shootInterval <= 0f || gameObjectProjectilePrefab == null)
        {
            return;
        }
        _shootTimer -= Time.deltaTime;
        if (_shootTimer <= 0f)
        {
            Shoot();
            _shootTimer = shootInterval;
        }
    }

    void Shoot()
    {
        GameObject gameObjectProjectile = Instantiate(gameObjectProjectilePrefab, transform.position, Quaternion.identity);
        Rigidbody2D projectileRb = gameObjectProjectile.GetComponent<Rigidbody2D>();
        if (projectileRb != null)
        {
            projectileRb.linearVelocity = GetShootVelocity();
        }
    }

    Vector2 GetShootVelocity()
    {
        switch (shootDirection)
        {
            case ShootDirection.Left:  return new Vector2(-shootSpeed, 0f);
            case ShootDirection.Right: return new Vector2(shootSpeed, 0f);
            case ShootDirection.Up:    return new Vector2(0f, shootSpeed);
            case ShootDirection.Down:  return new Vector2(0f, -shootSpeed);
            default:                   return new Vector2(-shootSpeed, 0f);
        }
    }

    void CorrectColliderOffset(float x, float y) // small correction because of sprite going off-center
    {
        _enemyCircleCollider2D.offset = new Vector2(x, y);
    }

    void InitializeWithFireBallState(WithFireBallState fS)
    {
        withFireBallState = fS;
        switch (fS)
        {
            case WithFireBallState.NeutralToFireBall:
                _killedByFireBall = false;
                _stunnedByFireBall = false;
                break;
            case WithFireBallState.StunnedByFireBall:
                _killedByFireBall = false;
                _stunnedByFireBall = true;
                break;
            case WithFireBallState.KilledByFireBall:
                _killedByFireBall = true;
                _stunnedByFireBall = false;
                break;
            default:
                withFireBallState = WithFireBallState.NeutralToFireBall;
                break;
        }
    }

    void ChangeEnemyDirection()
    {
        if (!enemyStunned && !enemyHit)
        {
            Debug.Log("Enemy turns around!");
            isMovingLeft = !isMovingLeft;
            _tempScaleVec = transform.localScale;
            // flip sprite by negating X scale — preserves Y scale without rotating the object
            if (isMovingLeft)
            {
                _tempScaleVec.x = Mathf.Abs(_tempScaleVec.x);
            }
            else
            {
                _tempScaleVec.x = -Mathf.Abs(_tempScaleVec.x);
            }
            transform.localScale = _tempScaleVec;
        }
    }

    void MoveEnemy(bool ml, float ms)
    {
        if (!enemyStunned)
        {
            // flying enemies lock Y to 0 — prevents physics nudges from drifting their height
            float vy = isFlying ? 0f : _enemyRb.linearVelocity.y;
            if (ml)
            {
                _enemyRb.linearVelocity = new Vector2(-ms, vy);
            }
            else
            {
                _enemyRb.linearVelocity = new Vector2(ms, vy);
            }
            _animator.Play("EnemyMove");
        }
        else
        {
            _animator.Play("EnemyStunned");
            CorrectColliderOffset(0f, _enemyCircleCollider2D.offset.y);
        }
    }

    void PushEnemy(float pVx)
    {
        // don't redirect velocity if already falling off a ledge
        if (!isEnemyFalling)
        {
            Debug.Log("Push!");
            _enemyRb.linearVelocity = new Vector2(pVx, _enemyRb.linearVelocity.y);
        }
    }

    void PlayerHurt(RaycastHit2D hit)
    {
        PlayerController pc = hit.collider.GetComponentInParent<PlayerController>();
        if (pc != null)
        {
            pc.TakeHit();
        }
    }

    public bool EnemyStunned
    {
        get
        {
            return enemyStunned;
        }
        set
        {
            enemyStunned = value;
        }
    }

    void CheckForPlayer()
    {
        _frontRaycastHit = Physics2D.Raycast(transform.position, Vector2.left, raycastDistanceFront, playerLayer);
        _backRaycastHit = Physics2D.Raycast(transform.position, Vector2.right, raycastDistanceBack, playerLayer);

        if (_frontRaycastHit)
        {
            if (enemyStunned)
            {
                PushEnemy(playerPushForce);
            }
            else
            {
                PlayerHurt(_frontRaycastHit);
            }
        }

        if (_backRaycastHit)
        {
            if (enemyStunned)
            {
                PushEnemy(-playerPushForce);
            }
            else
            {
                PlayerHurt(_backRaycastHit);
            }
        }
    }

    void CheckForGroundBelow()
    {
        if (enemyStunned)
        {
            return;
        }
        // _groundRaycastHit: short range from edge point — detects platform drop-off to turn around
        // _groundRaycastHitFallDamage: long range from center — detects a fall and stuns the enemy
        _groundRaycastHit = Physics2D.Raycast(groundCheckTransform.position, Vector2.down, raycastDistanceGround, groundLayer);
        _groundRaycastHitFallDamage = Physics2D.Raycast(transform.position, Vector2.down, 2f, groundLayer);

        if (!_groundRaycastHit)
        {
            ChangeEnemyDirection();
        }

        if (!_groundRaycastHitFallDamage)
        {
            Debug.Log("Enemy fall damage!");
            isEnemyFalling = true;
            enemyStunned = true;
        }
    }

    void CheckForObstacle()
    {
        if (enemyStunned)
        {
            return;
        }
        if (isMovingLeft)
        {
            _frontRaycastHit = Physics2D.Raycast(transform.position, Vector2.left, raycastDistanceFront + 0.25f, groundLayer);
        }
        else
        {
            _frontRaycastHit = Physics2D.Raycast(transform.position, Vector2.right, raycastDistanceFront + 0.25f, groundLayer);
        }

        if (_frontRaycastHit)
        {
            Debug.Log("Obstacle in front of enemy!");
            ChangeEnemyDirection();
        }
    }

    public void BounceKillEnemy()
    {
        _isBounceKill = true;
        BounceEnemyUp(_enemyRb.linearVelocity.x, enemyBounceForce);
        // switch to trigger so the corpse doesn't physically interact during its upward arc
        _enemyCircleCollider2D.isTrigger = true;
        // z = -5 renders the corpse in front of other sprites while it flies up
        transform.position = new Vector3(transform.position.x, transform.position.y, -5f);
        enemyHit = true;
    }

    void BounceEnemyUp(float pvx, float f)
    {
        _enemyRb.linearVelocity = new Vector2(pvx, f);
    }

    public void EnemyPlayAudio(AudioClip sound)
    {
        _audioScript.PlayAudio(sound);
    }

    public void EnemyPlayAudioWaitToFinishClip(AudioClip sound)
    {
        _audioScript.PlayAudioWaitToFinishClip(sound);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag(TagScript.TURN_ENEMY_TAG))
        {
            if (!enemyStunned)
            {
                ChangeEnemyDirection();
            }
        }

        if (other.gameObject.CompareTag(TagScript.PLAYER_TAG))
        {
            _audioScript.PlayAudioWaitToFinishClip(soundPlayerStunEnemy);
        }

        if (other.gameObject.CompareTag(TagScript.FIRE_BALL_TAG))
        {
            _audioScript.PlayAudio(soundFireBallHit);
            if (_stunnedByFireBall)
            {
                enemyStunned = true;
            }
            if (_killedByFireBall)
            {
                if (!enemyHit)
                {
                    BounceKillEnemy();
                }
            }
        }
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag(TagScript.ENEMY_TAG))
        {
            ChangeEnemyDirection();
        }
    }

} // end of class
