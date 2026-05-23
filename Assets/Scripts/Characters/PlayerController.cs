using System.Collections;
using UnityEngine;
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
    public Transform savePointTransform;
    public GameObject fireBall;
    public GameObject stompParticles;
    public TextMeshPro hudLives;
    public TextMeshPro hudCoins;

    [Header("Audio")]
    public AudioClip soundPlayerJump;
    public AudioClip soundPlayerSwim;
    public AudioClip soundPlayerShootFireBall;
    public AudioClip soundPlayerDied;
    public AudioClip soundSavePoint;
    public AudioClip soundPlayerStunEnemyBells;
    public AudioClip soundPlayerStunEnemyBoing;

    [Header("State")]
    public bool playerControlsEnabled = true;
    public bool playerCanShoot = false;
    public int playerDirection = 1;
    public int playerLives = 3;
    public int playerCoins = 0;

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
    // _audioScriptSelf: sounds on this player only; _audioScriptGameMaster: global world sounds
    private AudioScript _audioScriptSelf;
    private AudioScript _audioScriptGameMaster;
    private EnemyScript _enemyScript;
    private Rigidbody2D _playerRb;
    private Animator _animator;
    private CameraScript _cameraScript;

    private float _inputH;
    private bool _isJumpPressed;
    private bool _isShootPressed;
    private bool _playerDied;
    private bool _playerSaved;
    private bool _isOnGround;
    // _playerJumped and _jumpButtonReleased together prevent jump re-triggering while
    // the button is held down or while the player is already airborne
    private bool _playerJumped;
    private bool _isSwimming;
    private bool _jumpButtonReleased;

    private float _moveSpeed;
    private float _defaultMoveSpeed;
    private Vector3 _newPosition;
    private Vector2 _tempScale;

    private const float SWIM_SPEED = 2f;
    private const float SWIM_FORCE = 3f;
    private const float DEFAULT_GRAVITY_SCALE = 2f;
    private const float IN_WATER_GRAVITY_SCALE = 0.5f;
    private const float DISABLE_PLAYER_TIME = 2f;
    private float _groundRaycastDistance;
    private float _jumpForce;
    private float _stunBounceForce;

    void Awake()
    {
        _playerRb = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
        _audioScriptSelf = GetComponent<AudioScript>();
        if (GameManager.Instance != null)
        {
            _audioScriptGameMaster = GameManager.Instance.GetComponent<AudioScript>();
            _cameraScript = GameManager.Instance.cameraScript;
        }

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
            hudCoins.text = playerCoins.ToString();
        }
    }

    void Update()
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

        if (!_playerDied)
        {
            if (playerControlsEnabled)
            {
                PlayerMoveInput(_inputH, _moveSpeed, _isJumpPressed, _isShootPressed);
            }

            CheckIfPlayerHeadButt();
            CheckIfPlayerOnGround();
        }
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
            if (_isOnGround && !_playerJumped && _jumpButtonReleased)
            {
                if (j)
                {
                    _playerJumped = true;
                    _jumpButtonReleased = false;
                    ChangePlayerDirection(_playerRb.linearVelocity.x, _jumpForce, "playerJumpAnimParam", true);
                    _audioScriptSelf.PlayAudio(soundPlayerJump);
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
                _audioScriptSelf.PlayAudio(soundPlayerSwim);
            }
        }

        if (playerCanShoot && s)
        {
            GameObject newBullet = Instantiate(fireBall, fireBallSocketTransform.position, Quaternion.identity);
            newBullet.GetComponent<FireBallScript>().PlayerDirectionToFireBallSpeed(true, playerDirection);
            _audioScriptSelf.PlayAudio(soundPlayerShootFireBall);
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
                // stun the enemy on stomp — bounce the player up so the stomp doesn't repeat next frame
                BouncePlayerUp(_playerRb.linearVelocity.x, _stunBounceForce);
                _enemyScript.EnemyStunned = true;
                SpawnStompParticles();
                if (_playerOnEnemyCollider.name == "Snail" && _audioScriptGameMaster != null)
                {
                    _audioScriptGameMaster.PlayAudioWaitToFinishClip(soundPlayerStunEnemyBells);
                }
                else if (_playerOnEnemyCollider.name == "Beetle" && _audioScriptGameMaster != null)
                {
                    _audioScriptGameMaster.PlayAudioWaitToFinishClip(soundPlayerStunEnemyBoing);
                }
            }
        }
        else
        {
            PlayerOnGround(false);
        }
    }

    void PlayerOnGround(bool b)
    {
        _isOnGround = b;
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
            if (!_isOnGround)
            {
                // damp horizontal speed when pressing into a wall mid-air
                _moveSpeed = 0.15f;
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
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!_playerDied)
        {
            if (!_playerSaved && other.gameObject.CompareTag("SavePoint"))
            {
                _audioScriptSelf.PlayAudioWaitToFinishClip(soundSavePoint);
                savePointTransform = other.transform;
                _playerSaved = true;
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
                _playerDied = true;
            }
        }

        if (other.gameObject.CompareTag("DestroyGameObjectBox"))
        {
            _playerRb.linearVelocity = Vector2.zero;
            _playerRb.simulated = false;
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
        if (_cameraScript != null)
        {
            _cameraScript.RemoveCameraTarget(transform);
        }
        _audioScriptSelf.PlayAudioWaitToFinishClip(soundPlayerDied);
        UpdatePlayerLives(-1);
        _animator.Play("PlayerHurt");
        StartCoroutine(EnumDisablePlayer(DISABLE_PLAYER_TIME));
        // 3x force for a dramatic death bounce
        BouncePlayerUp(_playerRb.linearVelocity.x, _stunBounceForce * 3f);
    }

    public bool PlayerNotSmall()
    {
        return playerState != PlayerState.PlayerSmall;
    }

    public void UpdatePlayerCoins(int i)
    {
        playerCoins += i;
        if (hudCoins != null)
        {
            hudCoins.text = playerCoins.ToString();
        }
    }

    public void UpdatePlayerLives(int i)
    {
        playerLives += i;
        if (hudLives != null)
        {
            hudLives.text = playerLives.ToString();
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
        if (playerLives <= 0)
        {
            gameObject.SetActive(false);
        }
        else
        {
            StartCoroutine(EnumPlacePlayerOnSavePoint(t));
        }
    }

    IEnumerator EnumPlacePlayerOnSavePoint(float t)
    {
        yield return new WaitForSeconds(t);
        PlacePlayerOnSavePoint(t);
    }

    void PlacePlayerOnSavePoint(float t)
    {
        // respawn at last save point; fall back to level start if none reached yet
        if (savePointTransform != null)
        {
            transform.position = savePointTransform.position;
        }
        else if (GameManager.Instance != null && GameManager.Instance.startPointTransform != null)
        {
            transform.position = GameManager.Instance.startPointTransform.position;
        }

        InitializePlayerState(PlayerState.PlayerSmall);
        _playerRb.simulated = true;
        _playerDied = false;
        _animator.Play("PlayerIdle");
        if (_cameraScript != null)
        {
            _cameraScript.AddCameraTarget(transform);
        }
    }

} // end of class
