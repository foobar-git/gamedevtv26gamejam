using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

// Controls a single player character (Red or Blue). Handles movement, jumping,
// swimming, shooting, pickup interactions, player state (size), and death/respawn.
public class PlayerController : MonoBehaviour
{
    public enum PlayerCharacter { Red, Blue }

    [Header("Assignment")]
    public PlayerCharacter assignedPlayerCharacter;
    public CharacterDataSO characterData;

    [Header("References")]
    public Transform groundCheckTransform;
    public Transform headCheckTransform;
    public Transform fireBallSocketTransform;
    private Transform _savePointTransform;
    public GameObject fireBall;
    public GameObject stompParticles;
    public TextMeshPro hudLives;
    public TextMeshPro hudCoins;

    [Header("Audio")]
    public AudioClip soundPlayerHit;
    public AudioClip soundPlayerJump;
    public AudioClip soundPlayerSwim;
    public AudioClip soundPlayerShootFireBall;
    public AudioClip soundPlayerDied;
    public AudioClip soundSavePoint;

    [Header("State")]
    [Tooltip("Editor/testing only — player cannot take damage.")]
    public bool isGodMode = false; // TODO: [Testing] - remove before shipping
    public bool playerControlsEnabled = true;
    public bool playerCanShoot = false;
    public int playerDirection = 1;
    public int playerLives = 3;
    public int playerCoins = 0; // TODO: [Phase X] - remove, replaced by GameManager.sharedCoins

    public enum PlayerState { PlayerSmall, PlayerNormal, PlayerLarge, PlayerFire }
    public PlayerState playerState;

    public Vector2 playerScaleSmall;
    public Vector2 playerScaleNormal;
    public Vector2 playerScaleLarge;

    public LayerMask groundLayer;
    public LayerMask enemyLayer;
    public LayerMask playerLayer;

    private Collider2D _groundCheckCollider;
    private Collider2D _playerOnEnemyCollider;
    private Collider2D _playerOnPlayerCollider;
    private Collider2D _playerHeadCollider;

    private GameObject _newStompParticlesAnim;
    private EnemyScript _enemyScript;
    private Rigidbody2D _playerRb;
    private Animator _animator;
    private CameraScript _cameraScript;

    private float _inputH;
    private bool _isJumpPressed;
    private bool _isShootPressed;
    private bool _playerDied;
    private bool _playerSaved;
    private bool _isGrounded;
    // _playerJumped and _jumpButtonReleased together prevent jump re-triggering while
    // the button is held down or while the player is already airborne
    private bool _playerJumped;
    private bool _isSwimming;
    private bool _jumpButtonReleased;
    private bool _isHanging;
    private bool _bodyTypeJustChanged;

    private float _moveSpeed;
    private float _defaultMoveSpeed;
    private Vector3 _newPosition;
    private Vector2 _tempScale;

    private const float SWIM_SPEED = 2f;
    private const float SWIM_FORCE = 3f;
    private const float DEFAULT_GRAVITY_SCALE = 2f;
    private const float IN_WATER_GRAVITY_SCALE = 0.5f;
    private const float DISABLE_PLAYER_TIME = 2f;
    private const float INVINCIBILITY_TIME = 1.5f;
    private float _groundRaycastDistance;
    private float _jumpForce;
    private float _stunBounceForce;
    private bool _isInvincible;
    private float _invincibilityTimer;
    private bool _isSharedLivesMode;

    void Awake()
    {
        _playerRb = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();

        // fall back to hardcoded defaults if no CharacterDataSO is assigned in the Inspector
        if (characterData != null)
        {
            _defaultMoveSpeed = characterData.moveSpeed;
            _jumpForce = characterData.jumpForce;
            _stunBounceForce = characterData.stunBounceForce;
            playerScaleSmall = characterData.scaleSmall;
            playerScaleNormal = characterData.scaleNormal;
            playerScaleLarge = characterData.scaleLarge;
        }
        else
        {
            _defaultMoveSpeed = 4f;
            _jumpForce = 11f;
            _stunBounceForce = 3f;
            playerScaleSmall = new Vector2(0.6f, 0.6f);
            playerScaleNormal = new Vector2(1f, 1f);
            playerScaleLarge = new Vector2(2f, 2f);
        }

        _moveSpeed = _defaultMoveSpeed;
        // derive from scale so the ground check radius stays accurate when the player grows/shrinks
        _groundRaycastDistance = transform.localScale.x / 5f;
        playerDirection = 1;
        _jumpButtonReleased = true;
        playerState = PlayerState.PlayerSmall;
    }

    void Start()
    {
        if (GameManager.Instance != null)
        {
            _cameraScript = GameManager.Instance.cameraScript;
        }
        if (GameManager.Instance != null && GameManager.Instance.startPointTransform != null)
        {
            transform.position = GameManager.Instance.startPointTransform.position;
        }

        if (hudLives != null)
        {
            hudLives.text = playerLives.ToString();
        }
        if (hudCoins != null)
        {
            hudCoins.text = (GameManager.Instance?.sharedCoins ?? 0).ToString();
        }
    }

    void Update()
    {
        ReadPlayerInput();
        UpdatePlayerMovement();
    }

    void FixedUpdate()
    {
        UpdatePlayerPhysicsChecks();
    }

    void LateUpdate()
    {
        LockPlayerZPosition();
    }

    void LockPlayerZPosition()
    {
        // Z must always be 0 — transform.position assignments can inherit non-zero Z from source transforms
        transform.position = new Vector3(transform.position.x, transform.position.y, 0f);
    }

    void ReadPlayerInput()
    {
        if (InputProvider.Instance == null)
        {
            return;
        }
        PlayerCharacterInput input = assignedPlayerCharacter == PlayerCharacter.Red
            ? InputProvider.Instance.GetRedInput()
            : InputProvider.Instance.GetBlueInput();
        _inputH = input.horizontal;
        _isJumpPressed = input.jumpPressed;
        _isShootPressed = input.shootPressed;
    }

    void UpdatePlayerMovement()
    {
        if (_playerDied)
        {
            return;
        }
        if (playerControlsEnabled)
        {
            PlayerMoveInput(_inputH, _moveSpeed, _isJumpPressed, _isShootPressed);
        }
    }

    void UpdatePlayerPhysicsChecks()
    {
        if (_playerDied)
        {
            return;
        }
        CheckIfPlayerHeadButt();
        CheckIfPlayerOnGround();
        TickInvincibility();
        UpdateWallHang();
    }

    void TickInvincibility()
    {
        if (!_isInvincible)
        {
            return;
        }
        _invincibilityTimer -= Time.fixedDeltaTime;
        if (_invincibilityTimer <= 0f)
        {
            _isInvincible = false;
        }
    }

    void UpdateWallHang()
    {
        // exit hang when player releases horizontal input
        if (!_isHanging || _inputH != 0)
        {
            return;
        }
        SetHanging(false);
    }

    void SetHanging(bool hanging)
    {
        // guard prevents repeated bodyType switches — only fires on state change
        if (_isHanging == hanging)
        {
            return;
        }
        _isHanging = hanging;
        _bodyTypeJustChanged = true;
        _playerRb.bodyType = hanging ? RigidbodyType2D.Static : RigidbodyType2D.Dynamic;
    }

    void PlayerMoveInput(float h, float ms, bool j, bool s)
    {
        if (h > 0)
        {
            playerDirection = 1;
            ChangePlayerDirection(playerDirection, ms, _playerRb.linearVelocity.y, "playerMoveAnimParam", true);
        }
        else if (h < 0)
        {
            playerDirection = -1;
            ChangePlayerDirection(playerDirection, -ms, _playerRb.linearVelocity.y, "playerMoveAnimParam", true);
        }
        else
        {
            ChangePlayerDirection(0f, _playerRb.linearVelocity.y, "playerMoveAnimParam", false);
        }

        if (!_isSwimming)
        {
            _animator.SetBool("playerSwimAnimParam", false);
            _playerRb.gravityScale = DEFAULT_GRAVITY_SCALE;
            _moveSpeed = _defaultMoveSpeed;
            if (_isGrounded && !_playerJumped && _jumpButtonReleased)
            {
                if (j)
                {
                    _playerJumped = true;
                    _jumpButtonReleased = false;
                    ChangePlayerDirection(_playerRb.linearVelocity.x, _jumpForce, "playerJumpAnimParam", true);
                    AudioScript.Instance.PlayAudio(soundPlayerJump);
                }
            }
            else
            {
                if (j)
                {
                    // cut vertical velocity when jump is re-pressed mid-air — variable jump height
                    _jumpButtonReleased = true;
                    ChangePlayerDirection(_playerRb.linearVelocity.x, _playerRb.linearVelocity.y - 4f, "playerJumpAnimParam", true);
                }
            }
        }
        else
        {
            _playerRb.gravityScale = IN_WATER_GRAVITY_SCALE;
            _moveSpeed = SWIM_SPEED;
            if (j)
            {
                ChangePlayerDirection(_playerRb.linearVelocity.x, SWIM_FORCE, "playerSwimAnimParam", true);
                AudioScript.Instance.PlayAudio(soundPlayerSwim);
            }
        }

        if (playerCanShoot && s)
        {
            GameObject newBullet = Instantiate(fireBall, fireBallSocketTransform.position, Quaternion.identity);
            newBullet.GetComponent<FireBallScript>().PlayerDirectionToFireBallSpeed(true, playerDirection);
            AudioScript.Instance.PlayAudio(soundPlayerShootFireBall);
        }
    }

    void ChangePlayerDirection(float pX, float pY, string a, bool b)
    {
        _playerRb.linearVelocity = new Vector2(pX, pY);
        _animator.SetBool(a, b);
    }

    void ChangePlayerDirection(int direction, float pX, float pY, string a, bool b)
    {
        _tempScale.x = Mathf.Abs(transform.localScale.x) * direction;
        _tempScale.y = transform.localScale.y;
        transform.localScale = _tempScale;
        _playerRb.linearVelocity = new Vector2(pX, pY);
        _animator.SetBool(a, b);
    }

    void CheckIfPlayerOnGround()
    {
        // three separate checks: ground/other-player (normal landing), enemy (stomp), each needs different response
        _groundCheckCollider = Physics2D.OverlapCircle(groundCheckTransform.position, _groundRaycastDistance, groundLayer);
        _playerOnEnemyCollider = Physics2D.OverlapCircle(groundCheckTransform.position, _groundRaycastDistance, enemyLayer);
        _playerOnPlayerCollider = Physics2D.OverlapCircle(groundCheckTransform.position, _groundRaycastDistance, playerLayer);

        if (_groundCheckCollider || _playerOnPlayerCollider)
        {
            PlayerOnGround(true);
            _playerJumped = false;
            _jumpButtonReleased = true;
        }
        else if (_playerOnEnemyCollider)
        {
            PlayerOnGround(true);
            _playerJumped = false;
            _jumpButtonReleased = true;
            _enemyScript = _playerOnEnemyCollider.GetComponent<EnemyScript>();
            if (!_enemyScript.EnemyStunned)
            {
                BouncePlayerUp(_playerRb.linearVelocity.x, _stunBounceForce);
                SpawnStompParticles();
                if (_enemyScript.withStompState == EnemyScript.WithStompState.KilledByStomp)
                {
                    _enemyScript.BounceKillEnemy();
                }
                else
                {
                    // default: stun the enemy — bounce player up so the stomp doesn't repeat next frame
                    _enemyScript.EnemyStunned = true;
                }
                _enemyScript.PlayStompSound();
            }
        }
        else
        {
            PlayerOnGround(false);
        }
    }

    void PlayerOnGround(bool b)
    {
        _isGrounded = b;
        _animator.SetBool("playerJumpAnimParam", !b);
    }

    void CheckIfPlayerHeadButt()
    {
        _playerHeadCollider = Physics2D.OverlapCircle(headCheckTransform.position, _groundRaycastDistance, groundLayer);
        if (_playerHeadCollider)
        {
            _moveSpeed = _defaultMoveSpeed;
        }
    }

    void BouncePlayerUp(float pvx, float f)
    {
        _playerRb.linearVelocity = new Vector2(pvx, f);
    }

    void SpawnStompParticles()
    {
        // z - 5 renders the particles in front of the player sprite
        _newPosition = new Vector3(transform.position.x, transform.position.y, transform.position.z - 5f);
        _newStompParticlesAnim = Instantiate(stompParticles, _newPosition, Quaternion.identity);
        _newStompParticlesAnim.GetComponent<ParticleSystem>().Play();
    }

    void OnCollisionStay2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Ground"))
        {
            if (!_isGrounded)
            {
                // damp horizontal speed when pressing into a wall mid-air
                _moveSpeed = 0.15f;
                SetHanging(IsHanging());
            }
            else
            {
                _moveSpeed = _defaultMoveSpeed;
            }
        }
    }

    void OnCollisionExit2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Ground"))
        {
            _moveSpeed = _defaultMoveSpeed;
            if (_bodyTypeJustChanged)
            {
                _bodyTypeJustChanged = false;
                return;
            }
            SetHanging(false);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!_playerDied)
        {
            if (other.gameObject.CompareTag("SavePoint") && other.transform != _savePointTransform)
            {
                AudioScript.Instance.PlayAudioWaitToFinishClip(soundSavePoint, AudioChannel.Player);
                _savePointTransform = other.transform;
                _playerSaved = true;
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.UpdateOtherPlayerSavePoint(this, other.transform);
                }
            }

            if (other.gameObject.CompareTag("PickupFireFlower"))
            {
                InitializePlayerState(PlayerState.PlayerFire);
            }

            if (other.gameObject.CompareTag("PickupMushroom"))
            {
                if (playerState != PlayerState.PlayerFire)
                {
                    InitializePlayerState(PlayerState.PlayerNormal);
                }
            }

            if (other.gameObject.CompareTag("PickupCoin"))
            {
                UpdatePlayerCoins(1);
            }

            if (other.gameObject.CompareTag("Pickup1up"))
            {
                UpdatePlayerLives(1);
            }

            if (other.gameObject.CompareTag("Water"))
            {
                _isSwimming = true;
            }

            if (other.gameObject.CompareTag("KillBox"))
            {
                PlayerDied();
            }

        }

        if (other.gameObject.CompareTag("DestroyGameObjectBox"))
        {
            _playerRb.linearVelocity = Vector2.zero;
            _playerRb.simulated = false;
            PlayerDied();
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!_playerDied && other.gameObject.CompareTag("Water"))
        {
            _isSwimming = false;
        }

    }

    void PlayerDied()
    {
        if (_playerDied)
        {
            return;
        }
        _playerDied = true;
        UpdatePlayerLives(-1);
        ExecuteDeathSequence();
    }

    // called by GameManager when shared lives hit 0 — forces death without deducting a life
    public void ForcePlayerDied()
    {
        if (_playerDied)
        {
            return;
        }
        _playerDied = true;
        ExecuteDeathSequence();
    }

    void ExecuteDeathSequence()
    {
        if (_cameraScript != null)
        {
            _cameraScript.RemoveCameraTarget(transform);
        }
        // disable kill zone before snap so boundary wall movement can't push players into it
        OnAnyPlayerDied?.Invoke();
        // check after removing from camera — if other player is also dead, snap camera to save point
        if (GameManager.Instance != null && GameManager.Instance.BothPlayersDead())
        {
            GameManager.Instance.OnBothPlayersDied();
        }
        AudioScript.Instance.PlayAudioWaitToFinishClip(soundPlayerDied, AudioChannel.Player);
        _animator.Play("PlayerHurt");
        StartCoroutine(EnumDisablePlayer(DISABLE_PLAYER_TIME));
        // 3x force for a dramatic death bounce
        BouncePlayerUp(_playerRb.linearVelocity.x, _stunBounceForce * 3f);
    }

    public void FreezeAtFinish()
    {
        playerControlsEnabled = false;
        _playerRb.bodyType = RigidbodyType2D.Static;
    }

    public bool IsPlayerDead => _playerDied;
    public static event System.Action OnAnyPlayerDied;
    public static event System.Action OnAnyPlayerRespawned;

    public bool PlayerNotSmall()
    {
        return playerState != PlayerState.PlayerSmall;
    }

    bool IsHanging()
    {
        // only called from OnCollisionStay2D wall-mid-air branch — ray confirms nothing is below;
        // input required so a player grazing the wall with no intent doesn't lock into Static
        if (_inputH == 0)
        {
            return false;
        }
        Debug.DrawRay(groundCheckTransform.position, Vector2.down * _groundRaycastDistance, new Color(0.5f, 1f, 0.5f)); // TODO: [Testing] - remove before shipping
        RaycastHit2D hit = Physics2D.Raycast(groundCheckTransform.position, Vector2.down, _groundRaycastDistance, groundLayer);
        return !hit;
    }

    // called by enemies and projectiles — shrinks player one state, kills if already small
    public void TakeHit()
    {
        if (isGodMode || _playerDied || _isInvincible)
        {
            return;
        }
        _isInvincible = true;
        _invincibilityTimer = INVINCIBILITY_TIME;
        if (playerState != PlayerState.PlayerSmall)
        {
            AudioScript.Instance.PlayAudio(soundPlayerHit);
        }
        switch (playerState)
        {
            case PlayerState.PlayerFire:
                InitializePlayerState(PlayerState.PlayerNormal);
                break;
            case PlayerState.PlayerNormal:
                InitializePlayerState(PlayerState.PlayerSmall);
                break;
            case PlayerState.PlayerLarge:
                InitializePlayerState(PlayerState.PlayerNormal);
                break;
            case PlayerState.PlayerSmall:
                PlayerDied();
                break;
        }
    }

    // called by GameManager when the other player touches a save point — syncs without playing the sound
    public void SetSavePoint(Transform savePoint)
    {
        _savePointTransform = savePoint;
        _playerSaved = true;
    }

    public void SetupSharedLives()
    {
        // single-player mode: lives managed by GameManager.sharedLives
        _isSharedLivesMode = true;
    }

    public void SetupOwnLives(int lives, TextMeshPro hud)
    {
        // two-player mode: this character owns its lives and HUD independently
        _isSharedLivesMode = false;
        playerLives = lives;
        hudLives = hud;
        if (hudLives != null)
        {
            hudLives.text = Mathf.Max(0, playerLives).ToString();
        }
    }

    public void UpdatePlayerCoins(int i)
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.sharedCoins += i;
        }
        if (hudCoins != null)
        {
            hudCoins.text = (GameManager.Instance?.sharedCoins ?? 0).ToString();
        }
    }

    public void UpdatePlayerLives(int i)
    {
        if (_isSharedLivesMode)
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.UpdateSharedLives(i);
            }
            return;
        }
        // two-player mode: each character tracks its own lives and HUD
        playerLives += i;
        if (hudLives != null)
        {
            hudLives.text = Mathf.Max(0, playerLives).ToString();
        }
    }

    void InitializePlayerState(PlayerState pS)
    {
        playerState = pS;
        switch (pS)
        {
            case PlayerState.PlayerSmall:
                transform.localScale = playerScaleSmall;
                playerCanShoot = false;
                break;
            case PlayerState.PlayerNormal:
                transform.localScale = playerScaleNormal;
                playerCanShoot = false;
                break;
            case PlayerState.PlayerLarge:
                transform.localScale = playerScaleLarge;
                playerCanShoot = false;
                break;
            case PlayerState.PlayerFire:
                transform.localScale = playerScaleNormal;
                playerCanShoot = true;
                break;
        }
        // recalculate after scale change — ground check radius must track the player's new size
        _groundRaycastDistance = transform.localScale.x / 5f;
    }

    IEnumerator EnumDisablePlayer(float t)
    {
        yield return new WaitForSeconds(t);
        DisablePlayer(t);
    }

    void DisablePlayer(float t)
    {
        _playerRb.linearVelocity = Vector2.zero;
        _playerRb.simulated = false;
        if (IsAllLivesGone())
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
        else
        {
            StartCoroutine(EnumPlacePlayerOnSavePoint(t));
        }
    }

    bool IsAllLivesGone()
    {
        if (_isSharedLivesMode)
        {
            return GameManager.Instance != null && GameManager.Instance.sharedLives <= 0;
        }
        // two-player mode: each character checks its own lives
        return playerLives < 0;
    }

    IEnumerator EnumPlacePlayerOnSavePoint(float t)
    {
        yield return new WaitForSeconds(t);
        PlacePlayerOnSavePoint();
    }

    void PlacePlayerOnSavePoint()
    {
        transform.position = GetRespawnPosition();
        InitializePlayerState(PlayerState.PlayerSmall);
        _playerRb.simulated = true;
        _playerDied = false;
        _animator.Play("PlayerIdle");
        if (_cameraScript != null)
        {
            _cameraScript.AddCameraTarget(transform);
        }
        OnAnyPlayerRespawned?.Invoke();
    }

    Vector3 GetRespawnPosition()
    {
        // if the other player is alive, respawn beside them — keeps co-op momentum going
        if (GameManager.Instance != null)
        {
            PlayerController otherPlayer = GameManager.Instance.GetOtherPlayer(this);
            if (otherPlayer != null && !otherPlayer.IsPlayerDead)
            {
                return otherPlayer.transform.position + new Vector3(1f, 0f, 0f);
            }
        }
        // both dead — respawn at save point, fall back to level start if none reached yet
        if (_savePointTransform != null)
        {
            return _savePointTransform.position;
        }
        if (GameManager.Instance != null && GameManager.Instance.startPointTransform != null)
        {
            return GameManager.Instance.startPointTransform.position;
        }
        return transform.position;
    }

} // end of class
