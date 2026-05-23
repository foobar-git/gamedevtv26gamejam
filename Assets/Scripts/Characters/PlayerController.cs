using System.Collections;
using UnityEngine;
using TMPro;

public class PlayerController : MonoBehaviour
{
    public enum Hand { Left, Right }

    [Header("Assignment")]
    public Hand assignedHand;
    public CharacterDataSO characterData;

    [Header("References")]
    public Transform groundCheckPosition;
    public Transform headCheckPosition;
    public Transform fireBallSocketPosition;
    public Transform savePoint;
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

    private Collider2D groundCheckRayHit;
    private Collider2D playerOnEnemyCheckRayHit;
    private Collider2D playerOnPlayerCheckRayHit;
    private Collider2D playerHeadCheckRayHit;

    private GameObject newStompParticlesAnim;
    private AudioScript audioScript;
    private AudioScript gameMasterAudioScript;
    private EnemyScript enemyScript;
    private Rigidbody2D playerBody;
    private Animator animator;

    private float inputH;
    private bool jump;
    private bool shoot;
    private bool playerDied;
    private bool playerSaved;
    private bool playerOnGround;
    private bool playerJumped;
    private bool playerSwimming;
    private bool jumpButtonReleased;

    private float moveSpeed;
    private float defaultMoveSpeed;
    private Vector3 newPosition;
    private Vector2 tempScale;

    private const float SWIM_SPEED = 2f;
    private const float SWIM_FORCE = 3f;
    private const float DEFAULT_GRAVITY_SCALE = 2f;
    private const float IN_WATER_GRAVITY_SCALE = 0.5f;
    private const float DISABLE_PLAYER_TIME = 2f;
    private float groundRaycastDistance;
    private float jumpForce;
    private float stunBounceForce;

    void Awake()
    {
        playerBody = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        audioScript = GetComponent<AudioScript>();
        if (GameManager.Instance != null) gameMasterAudioScript = GameManager.Instance.GetComponent<AudioScript>();

        GameObject go = GameObject.Find("Main Camera");
        if (go != null) cameraScript = go.GetComponent<CameraScript>();
        else Debug.LogWarning("PlayerController: Main Camera not found");

        go = GameObject.Find("hudLives");
        if (go != null) hudLives = go.GetComponent<TextMeshPro>();
        else Debug.LogWarning("PlayerController: hudLives not found");

        go = GameObject.Find("hudCoins");
        if (go != null) hudCoins = go.GetComponent<TextMeshPro>();
        else Debug.LogWarning("PlayerController: hudCoins not found");

        if (characterData != null)
        {
            defaultMoveSpeed = characterData.moveSpeed;
            jumpForce = characterData.jumpForce;
            stunBounceForce = characterData.stunBounceForce;
            playerScaleSmall = characterData.scaleSmall;
            playerScaleNormal = characterData.scaleNormal;
            playerScaleLarge = characterData.scaleLarge;
        }
        else
        {
            defaultMoveSpeed = 4f;
            jumpForce = 11f;
            stunBounceForce = 3f;
            playerScaleSmall = new Vector2(0.6f, 0.6f);
            playerScaleNormal = new Vector2(1f, 1f);
            playerScaleLarge = new Vector2(2f, 2f);
        }

        moveSpeed = defaultMoveSpeed;
        groundRaycastDistance = transform.localScale.x / 5f;
        playerDirection = 1;
        jumpButtonReleased = true;
        playerState = PlayerState.PlayerSmall;
    }

    void Start()
    {
        if (GameManager.Instance != null && GameManager.Instance.startPoint != null)
            transform.position = GameManager.Instance.startPoint.position;

        if (hudLives != null) hudLives.text = playerLives.ToString();
        if (hudCoins != null) hudCoins.text = playerCoins.ToString();
    }

    void Update()
    {
        if (InputProvider.Instance == null) return;

        HandInput input = assignedHand == Hand.Left
            ? InputProvider.Instance.GetLeftHand()
            : InputProvider.Instance.GetRightHand();

        inputH = input.horizontal;
        jump = input.jumpPressed;
        shoot = input.shootPressed;

        if (!playerDied)
        {
            if (playerControlsEnabled)
                PlayerMoveInput(inputH, moveSpeed, jump, shoot);

            CheckIfPlayerHeadButt();
            CheckIfPlayerOnGround();
        }
    }

    void PlayerMoveInput(float h, float ms, bool j, bool s)
    {
        if (h > 0)
        {
            playerDirection = 1;
            ChangePlayerDirection(playerDirection, ms, playerBody.linearVelocity.y, "playerMoveAnimParam", true);
        }
        else if (h < 0)
        {
            playerDirection = -1;
            ChangePlayerDirection(playerDirection, -ms, playerBody.linearVelocity.y, "playerMoveAnimParam", true);
        }
        else
        {
            ChangePlayerDirection(0f, playerBody.linearVelocity.y, "playerMoveAnimParam", false);
        }

        if (!playerSwimming)
        {
            animator.SetBool("playerSwimAnimParam", false);
            playerBody.gravityScale = DEFAULT_GRAVITY_SCALE;
            moveSpeed = defaultMoveSpeed;
            if (playerOnGround && !playerJumped && jumpButtonReleased)
            {
                if (j)
                {
                    playerJumped = true;
                    jumpButtonReleased = false;
                    ChangePlayerDirection(playerBody.linearVelocity.x, jumpForce, "playerJumpAnimParam", true);
                    audioScript.PlayAudio(soundPlayerJump);
                }
            }
            else if (!playerOnGround || playerOnGround)
            {
                if (j)
                {
                    jumpButtonReleased = true;
                    ChangePlayerDirection(playerBody.linearVelocity.x, playerBody.linearVelocity.y - 4f, "playerJumpAnimParam", true);
                }
            }
        }
        else
        {
            playerBody.gravityScale = IN_WATER_GRAVITY_SCALE;
            moveSpeed = SWIM_SPEED;
            if (j)
            {
                ChangePlayerDirection(playerBody.linearVelocity.x, SWIM_FORCE, "playerSwimAnimParam", true);
                audioScript.PlayAudio(soundPlayerSwim);
            }
        }

        if (playerCanShoot && s)
        {
            GameObject newBullet = Instantiate(fireBall, fireBallSocketPosition.position, Quaternion.identity);
            newBullet.GetComponent<FireBallScript>().PlayerDirectionToFireBallSpeed(true, playerDirection);
            audioScript.PlayAudio(soundPlayerShootFireBall);
        }
    }

    void ChangePlayerDirection(float pX, float pY, string a, bool b)
    {
        playerBody.linearVelocity = new Vector2(pX, pY);
        animator.SetBool(a, b);
    }

    void ChangePlayerDirection(int direction, float pX, float pY, string a, bool b)
    {
        tempScale.x = Mathf.Abs(transform.localScale.x) * direction;
        tempScale.y = transform.localScale.y;
        transform.localScale = tempScale;
        playerBody.linearVelocity = new Vector2(pX, pY);
        animator.SetBool(a, b);
    }

    void CheckIfPlayerOnGround()
    {
        groundCheckRayHit = Physics2D.OverlapCircle(groundCheckPosition.position, groundRaycastDistance, groundLayer);
        playerOnEnemyCheckRayHit = Physics2D.OverlapCircle(groundCheckPosition.position, groundRaycastDistance, enemyLayer);
        playerOnPlayerCheckRayHit = Physics2D.OverlapCircle(groundCheckPosition.position, groundRaycastDistance, playerLayer);

        if (groundCheckRayHit || playerOnPlayerCheckRayHit)
        {
            PlayerOnGround(true);
            playerJumped = false;
            jumpButtonReleased = true;
        }
        else if (playerOnEnemyCheckRayHit)
        {
            PlayerOnGround(true);
            playerJumped = false;
            jumpButtonReleased = true;
            enemyScript = playerOnEnemyCheckRayHit.GetComponent<EnemyScript>();
            if (!enemyScript.EnemyStunned)
            {
                BouncePlayerUp(playerBody.linearVelocity.x, stunBounceForce);
                enemyScript.EnemyStunned = true;
                SpawnStompParticles();
                if (playerOnEnemyCheckRayHit.name == "Snail" && gameMasterAudioScript != null)
                    gameMasterAudioScript.PlayAudioWaitToFinishClip(soundPlayerStunEnemyBells);
                else if (playerOnEnemyCheckRayHit.name == "Beetle" && gameMasterAudioScript != null)
                    gameMasterAudioScript.PlayAudioWaitToFinishClip(soundPlayerStunEnemyBoing);
            }
        }
        else
        {
            PlayerOnGround(false);
        }
    }

    void PlayerOnGround(bool b)
    {
        playerOnGround = b;
        animator.SetBool("playerJumpAnimParam", !b);
    }

    void CheckIfPlayerHeadButt()
    {
        playerHeadCheckRayHit = Physics2D.OverlapCircle(headCheckPosition.position, groundRaycastDistance, groundLayer);
        if (playerHeadCheckRayHit)
            moveSpeed = defaultMoveSpeed;
    }

    void BouncePlayerUp(float pvx, float f)
    {
        playerBody.linearVelocity = new Vector2(pvx, f);
    }

    void SpawnStompParticles()
    {
        newPosition = new Vector3(transform.position.x, transform.position.y, transform.position.z + -5f);
        newStompParticlesAnim = Instantiate(stompParticles, newPosition, Quaternion.identity);
        newStompParticlesAnim.GetComponent<ParticleSystem>().Play();
    }

    void OnCollisionStay2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Ground"))
        {
            if (!playerOnGround)
                moveSpeed = 0.15f;
            else
                moveSpeed = defaultMoveSpeed;
        }
    }

    void OnCollisionExit2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Ground"))
            moveSpeed = defaultMoveSpeed;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!playerDied)
        {
            if (!playerSaved && other.gameObject.CompareTag("SavePoint"))
            {
                audioScript.PlayAudioWaitToFinishClip(soundSavePoint);
                savePoint = other.transform;
                playerSaved = true;
            }

            if (other.gameObject.CompareTag("PickupFireFlower"))
                InitializePlayerState(PlayerState.PlayerFire);

            if (other.gameObject.CompareTag("PickupMushroom"))
            {
                if (playerState != PlayerState.PlayerFire)
                    InitializePlayerState(PlayerState.PlayerNormal);
            }

            if (other.gameObject.CompareTag("PickupCoin"))
                UpdatePlayerCoins(1);

            if (other.gameObject.CompareTag("Pickup1up"))
                UpdatePlayerLives(1);

            if (other.gameObject.CompareTag("Water"))
                playerSwimming = true;

            if (other.gameObject.CompareTag("KillBox"))
            {
                PlayerDied();
                playerDied = true;
            }
        }

        if (other.gameObject.CompareTag("DestroyGameObjectBox"))
        {
            playerBody.linearVelocity = Vector2.zero;
            playerBody.simulated = false;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!playerDied && other.gameObject.CompareTag("Water"))
            playerSwimming = false;
    }

    void PlayerDied()
    {
        if (cameraScript != null) cameraScript.RemoveCameraTarget(transform);
        audioScript.PlayAudioWaitToFinishClip(soundPlayerDied);
        UpdatePlayerLives(-1);
        animator.Play("PlayerHurt");
        StartCoroutine(EnumDisablePlayer(DISABLE_PLAYER_TIME));
        BouncePlayerUp(playerBody.linearVelocity.x, stunBounceForce * 3f);
    }

    public bool PlayerNotSmall()
    {
        return playerState != PlayerState.PlayerSmall;
    }

    public void UpdatePlayerCoins(int i)
    {
        playerCoins += i;
        if (hudCoins != null) hudCoins.text = playerCoins.ToString();
    }

    public void UpdatePlayerLives(int i)
    {
        playerLives += i;
        if (hudLives != null) hudLives.text = playerLives.ToString();
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
        groundRaycastDistance = transform.localScale.x / 5f;
    }

    IEnumerator EnumDisablePlayer(float t)
    {
        yield return new WaitForSeconds(t);
        DisablePlayer(t);
    }

    void DisablePlayer(float t)
    {
        playerBody.linearVelocity = Vector2.zero;
        playerBody.simulated = false;
        if (playerLives <= 0)
            gameObject.SetActive(false);
        else
            StartCoroutine(EnumPlacePlayerOnSavePoint(t));
    }

    IEnumerator EnumPlacePlayerOnSavePoint(float t)
    {
        yield return new WaitForSeconds(t);
        PlacePlayerOnSavePoint(t);
    }

    void PlacePlayerOnSavePoint(float t)
    {
        if (savePoint != null)
            transform.position = savePoint.position;
        else if (GameManager.Instance != null && GameManager.Instance.startPoint != null)
            transform.position = GameManager.Instance.startPoint.position;

        InitializePlayerState(PlayerState.PlayerSmall);
        playerBody.simulated = true;
        playerDied = false;
        animator.Play("PlayerIdle");
        if (cameraScript != null) cameraScript.AddCameraTarget(transform);
    }

    private CameraScript cameraScript;
}
