using System.Collections;
using UnityEngine;

// Controls a brick block — manages state (normal, coin, mushroom, etc.), bump
// animations, pickup spawning, and enemy/pickup interactions when bumped from below.
public class BrickScript : MonoBehaviour
{
    [SerializeField] private int defaultCoinCount, bounceCount;
    [SerializeField] private float destroyObjectTime, speedCoinOrFireFlowerBrickAnim, speedPickupBrickAnim, resetBumpBrickTime;

    private PlayerController _playerController;
    private EnemyScript _enemyScript;
    private PickupScript _pickupScript;
    private MeshRenderer _meshRenderer;
    private Material[] _materials;
    private Collider2D _thisCollider;
    private Animator _animator;
    public AudioClip soundBrickSolid, soundBrickDestroy, soundPickup, soundCoin;
    public Material materialBrickSolid;
    public GameObject gameObjectCoinBrickAnim, gameObjectFireFlower, gameObjectMushroom, gameObjectM1up, gameObjectBrickParticles;
    private GameObject _gameObjectNewCoinBrickAnim, _gameObjectNewPickup, _gameObjectNewBrickParticlesAnim;

    private Vector3 _moveToCoinBrickAnimPosition, _moveToPickupPosition;

    public bool playerHasBumpedBrick, isRandomBrick;
    private bool _isMovingCoinAnim, _isMovingPickupAnim;

    // workaround for probability of random-brick states: multiple entries per type give
    // weighted probability since enum selection is uniform across all values
    private enum RandomBrickState { BrickNormal1, BrickNormal2, BrickNormal3, BrickNormal4, BrickCoin1, BrickCoin2, BrickCoin3, BrickCoin4, BrickCoin5, BrickCoin6,
        BrickCoin7, BrickCoin8, BrickCoin9, BrickCoin10, BrickMushroom1, BrickMushroom2, Brick1up1, BrickFireFlower1, BrickFireFlower2 }
    public enum BrickState { BrickSolid, BrickNormal, BrickCoin, BrickMushroom, Brick1up, BrickFireFlower }
    public BrickState brickState;

    ////////////////////////////////////////////////////////////////////////////////////////

    void Awake()
    {
        _animator = GetComponent<Animator>();
        _meshRenderer = GetComponent<MeshRenderer>();
        _materials = _meshRenderer.materials;
        _thisCollider = GetComponent<Collider2D>();

        playerHasBumpedBrick = false;
        _isMovingCoinAnim = false;
        _isMovingPickupAnim = false;

        defaultCoinCount = 3;
        bounceCount = defaultCoinCount;
        destroyObjectTime = 1f;
        speedCoinOrFireFlowerBrickAnim = 2f;
        speedPickupBrickAnim = 1f;
        resetBumpBrickTime = 0.5f;
    }

    void Start()
    {
        if (isRandomBrick)
        {
            SetRandomBrickState();
        }
        else
        {
            InitializeBrickState(brickState);
        }
    }

    void Update()
    {
        MoveCoinOrFireFlower_BrickAnim(speedCoinOrFireFlowerBrickAnim);
        MovePickup_BrickAnim(speedPickupBrickAnim);
    }

    void SetRandomBrickState()
    {
        System.Array enumValues = System.Enum.GetValues(typeof(RandomBrickState));
        RandomBrickState randomState = (RandomBrickState)enumValues.GetValue(UnityEngine.Random.Range(0, enumValues.Length));
        InitializeRandomBrickState(randomState);
    }

    private void FnRandomBrickState(BrickState s, int i)
    {
        brickState = s;
        bounceCount = i;
    }

    private void InitializeRandomBrickState(RandomBrickState rbS)
    {
        switch (rbS)
        {
            case RandomBrickState.BrickNormal1:
                FnRandomBrickState(BrickState.BrickNormal, 0);
                break;
            case RandomBrickState.BrickNormal2:
                FnRandomBrickState(BrickState.BrickNormal, 0);
                break;
            case RandomBrickState.BrickNormal3:
                FnRandomBrickState(BrickState.BrickNormal, 0);
                break;
            case RandomBrickState.BrickNormal4:
                FnRandomBrickState(BrickState.BrickNormal, 0);
                break;
            case RandomBrickState.BrickCoin1:
                FnRandomBrickState(BrickState.BrickCoin, defaultCoinCount);
                break;
            case RandomBrickState.BrickCoin2:
                FnRandomBrickState(BrickState.BrickCoin, defaultCoinCount);
                break;
            case RandomBrickState.BrickCoin3:
                FnRandomBrickState(BrickState.BrickCoin, defaultCoinCount);
                break;
            case RandomBrickState.BrickCoin4:
                FnRandomBrickState(BrickState.BrickCoin, defaultCoinCount);
                break;
            case RandomBrickState.BrickCoin5:
                FnRandomBrickState(BrickState.BrickCoin, defaultCoinCount);
                break;
            case RandomBrickState.BrickCoin6:
                FnRandomBrickState(BrickState.BrickCoin, defaultCoinCount);
                break;
            case RandomBrickState.BrickCoin7:
                FnRandomBrickState(BrickState.BrickCoin, defaultCoinCount);
                break;
            case RandomBrickState.BrickCoin8:
                FnRandomBrickState(BrickState.BrickCoin, defaultCoinCount);
                break;
            case RandomBrickState.BrickCoin9:
                FnRandomBrickState(BrickState.BrickCoin, defaultCoinCount);
                break;
            case RandomBrickState.BrickCoin10:
                FnRandomBrickState(BrickState.BrickCoin, defaultCoinCount);
                break;
            case RandomBrickState.BrickMushroom1:
                FnRandomBrickState(BrickState.BrickMushroom, 1);
                break;
            case RandomBrickState.BrickMushroom2:
                FnRandomBrickState(BrickState.BrickMushroom, 1);
                break;
            case RandomBrickState.Brick1up1:
                FnRandomBrickState(BrickState.Brick1up, 1);
                break;
            case RandomBrickState.BrickFireFlower1:
                FnRandomBrickState(BrickState.BrickFireFlower, 1);
                break;
            case RandomBrickState.BrickFireFlower2:
                FnRandomBrickState(BrickState.BrickFireFlower, 1);
                break;
            default:
                brickState = BrickState.BrickNormal;
                break;
        }
        Debug.Log("Random-Brick state: " + brickState);
    }

    void InitializeBrickState(BrickState bS)
    {
        brickState = bS;
        switch (bS)
        {
            case BrickState.BrickSolid:
                bounceCount = 0;
                _materials[0] = materialBrickSolid;
                _meshRenderer.materials = _materials;
                break;
            case BrickState.BrickNormal:
                bounceCount = 0;
                break;
            case BrickState.BrickCoin:
                bounceCount = defaultCoinCount;
                break;
            case BrickState.BrickMushroom:
                bounceCount = 1;
                break;
            case BrickState.Brick1up:
                bounceCount = 1;
                break;
            case BrickState.BrickFireFlower:
                bounceCount = 1;
                break;
            default:
                brickState = BrickState.BrickNormal;
                break;
        }
        Debug.Log("Brick state: " + brickState);
    }

    void DisableGameObject(bool b, AudioClip a)
    {
        AudioScript.Instance.PlayAudio(a);
        _meshRenderer.enabled = !b;
        _thisCollider.enabled = !b;
    }

    void AnimateBrickSolid()
    {
        AudioScript.Instance.PlayAudioWaitToFinishClip(soundBrickSolid);
    }

    void AnimateBrickNormal()
    {
        _animator.Play("BrickBounce");
        // random bricks become solid after breaking so they can't be bumped again
        if (isRandomBrick)
        {
            InitializeBrickState(BrickState.BrickSolid);
        }

        if (_playerController.PlayerNotSmall())
        {
            AudioScript.Instance.PlayAudioWaitToFinishClip(soundBrickDestroy);
            SpawnBrickParticles();
            DisableGameObject(true, soundBrickDestroy);
            Destroy(gameObject, soundBrickDestroy.length - 0.1f);
        }
        else
        {
            AudioScript.Instance.PlayAudio(soundBrickSolid);
        }
    }

    void AnimateBrickCoin(int i)
    {
        SpawnCoinOrFireFlower_BrickAnim(true, gameObjectCoinBrickAnim, 0f, 0.85f, -1f);
        // last coin depletes the brick — go solid; otherwise decrement and stay active
        if (i <= 1)
        {
            _playerController.UpdatePlayerCoins(1);
            AudioScript.Instance.PlayAudio(soundCoin);
            InitializeBrickState(BrickState.BrickSolid);
        }
        else
        {
            _animator.Play("BrickBounce");
            i--;
            _playerController.UpdatePlayerCoins(1);
            AudioScript.Instance.PlayAudio(soundCoin);
            bounceCount = i;
        }
    }

    void AnimateBrickFireFlower()
    {
        SpawnCoinOrFireFlower_BrickAnim(false, gameObjectFireFlower, 0f, 0.8f, 1f);
        AudioScript.Instance.PlayAudio(soundPickup);
        InitializeBrickState(BrickState.BrickSolid);
    }

    void AnimateBrickMushroomOr1up(GameObject gameObjectPickup)
    {
        SpawnMushroomOr1up(gameObjectPickup, 0f, 0.75f, 0f);
        AudioScript.Instance.PlayAudio(soundPickup);
        InitializeBrickState(BrickState.BrickSolid);
    }

    void SpawnBrickParticles()
    {
        _gameObjectNewBrickParticlesAnim = Instantiate(gameObjectBrickParticles, transform.position, Quaternion.identity);
        _gameObjectNewBrickParticlesAnim.GetComponent<ParticleSystem>().Play();
    }

    void SpawnMushroomOr1up(GameObject gameObjectPickup, float x, float y, float z)
    {
        _gameObjectNewPickup = Instantiate(gameObjectPickup, transform.position, Quaternion.identity);
        _moveToPickupPosition = new Vector3(transform.position.x + x, transform.position.y + y, transform.position.z + z);
        _isMovingPickupAnim = true;
    }

    // b = true: auto-destroy after anim (coin pop); b = false: keep alive (fire flower is the real pickup)
    void SpawnCoinOrFireFlower_BrickAnim(bool b, GameObject gameObjectPickup, float x, float y, float z)
    {
        _gameObjectNewCoinBrickAnim = Instantiate(gameObjectPickup, transform.position, Quaternion.identity);
        _moveToCoinBrickAnimPosition = new Vector3(transform.position.x + x, transform.position.y + y, transform.position.z + z);
        _isMovingCoinAnim = true;
        if (b)
        {
            Destroy(_gameObjectNewCoinBrickAnim, destroyObjectTime);
        }
    }

    void MoveCoinOrFireFlower_BrickAnim(float speed)
    {
        if (!_isMovingCoinAnim)
        {
            return;
        }
        if (_gameObjectNewCoinBrickAnim.transform.position != _moveToCoinBrickAnimPosition)
        {
            _gameObjectNewCoinBrickAnim.transform.position = Vector3.MoveTowards(_gameObjectNewCoinBrickAnim.transform.position, _moveToCoinBrickAnimPosition, speed * Time.deltaTime);
        }
        else
        {
            _isMovingCoinAnim = false;
        }
    }

    void MovePickup_BrickAnim(float speed)
    {
        if (!_isMovingPickupAnim)
        {
            return;
        }
        if (_gameObjectNewPickup.transform.position != _moveToPickupPosition)
        {
            _gameObjectNewPickup.transform.position = Vector3.MoveTowards(_gameObjectNewPickup.transform.position, _moveToPickupPosition, speed * Time.deltaTime);
        }
        else
        {
            // pickup reached its target — re-enable physics and hand off to autonomous pickup movement
            _gameObjectNewPickup.GetComponent<Rigidbody2D>().simulated = true;
            _gameObjectNewPickup.GetComponent<PickupScript>().isMovingPickup = true;
            _isMovingPickupAnim = false;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // name check instead of tag — the head zone is identified by its child object name
        if (other.gameObject.name == "PlayerHead")
        {
            Debug.Log("Player hits brick with head!");
            if (gameObject.name == "brick_solid")
            {
                AudioScript.Instance.PlayAudio(soundBrickSolid);
            }
            else
            {
                playerHasBumpedBrick = true;    // USED FOR HITTING ENEMIES FROM BELOW
                // fetch here, not in Awake — must be the specific player who hit this brick
                _playerController = other.GetComponentInParent<PlayerController>();
                if (brickState == BrickState.BrickSolid)
                {
                    AnimateBrickSolid();
                }
                else if (brickState == BrickState.BrickNormal)
                {
                    AnimateBrickNormal();
                }
                else if (brickState == BrickState.BrickCoin)
                {
                    AnimateBrickCoin(bounceCount);
                }
                else if (brickState == BrickState.BrickMushroom)
                {
                    AnimateBrickMushroomOr1up(gameObjectMushroom);
                }
                else if (brickState == BrickState.Brick1up)
                {
                    AnimateBrickMushroomOr1up(gameObjectM1up);
                }
                else if (brickState == BrickState.BrickFireFlower)
                {
                    AnimateBrickFireFlower();
                }
                StartCoroutine(ResetBumpBrick(resetBumpBrickTime));
            }
        }
    }

    void OnCollisionStay2D(Collision2D other)
    {
        if (other.gameObject.CompareTag(TagScript.ENEMY_TAG))
        {
            _enemyScript = other.gameObject.GetComponent<EnemyScript>();
            if (playerHasBumpedBrick)
            {
                _enemyScript.BounceKillEnemy();
            }
        }

        if (other.gameObject.CompareTag(TagScript.PICKUP_MUSHROOM_TAG) || other.gameObject.CompareTag(TagScript.PICKUP_1UP_TAG))
        {
            _pickupScript = other.gameObject.GetComponent<PickupScript>();
            if (playerHasBumpedBrick)
            {
                _pickupScript.ChangePickupMoveDirection();
            }
        }
    }

    // short delay before resetting so OnCollisionStay2D enemy/pickup events have time to fire
    IEnumerator ResetBumpBrick(float t)
    {
        yield return new WaitForSeconds(t);
        playerHasBumpedBrick = false;
    }

} // end of class
