using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrickScript : MonoBehaviour {
	
	[SerializeField] private int defaultCoinCount, bounceCount;
	[SerializeField] private float destroyObjectTime, speed_coinOrFireFlowerBrickAnim, speed_PickupBrickAnim, resetBumpBrickTime;

	private PlayerController _playerController;
	private EnemyScript _enemyScript;
	private PickupScript _pickupScript;
	private MeshRenderer _meshRenderer;
	private Material[] _materials;
    private Collider2D _thisCollider;
	private Animator _animator;
	private AudioScript _audioScript;
	public AudioClip soundBrickSolid, soundBrickDestroy, soundPickup, soundCoin;
	public Material brick_solid;
	public GameObject coinBrickAnim, fireFlower, mushroom, m1up, brickParticles;
	private GameObject _newCoinBrickAnim, _newPickup, _newBrickParticlesAnim;

	private Vector3 _moveToCoinBrickAnimPosition, _moveToPickupPosition;

	public bool playerHasBumpedBrick, isRandomBrick;
	private bool _isMovingCoinAnim, _isMovingPickupAnim;

	// workaround for probability of random-brick states:
	private enum RandomBrickState { BrickNormal1, BrickNormal2, BrickNormal3, BrickNormal4, BrickCoin1, BrickCoin2, BrickCoin3, BrickCoin4, BrickCoin5, BrickCoin6,
		BrickCoin7, BrickCoin8, BrickCoin9, BrickCoin10, BrickMushroom1, BrickMushroom2, Brick1up1, BrickFireFlower1, BrickFireFlower2 }
    public enum BrickState	{ BrickSolid, BrickNormal, BrickCoin, BrickMushroom, Brick1up, BrickFireFlower }
    public BrickState brickState;

	////////////////////////////////////////////////////////////////////////////////////////

	void Awake () {
        _audioScript = GetComponent<AudioScript> ();
		_animator = GetComponent<Animator> ();
		_meshRenderer = GetComponent<MeshRenderer> ();
		_materials = _meshRenderer.materials;
        _thisCollider = GetComponent<Collider2D> ();

		playerHasBumpedBrick = false;
		_isMovingCoinAnim = false;
		_isMovingPickupAnim = false;

		defaultCoinCount = 3;
		bounceCount = defaultCoinCount;
		destroyObjectTime = 1f;
		speed_coinOrFireFlowerBrickAnim = 2f;
		speed_PickupBrickAnim = 1f;
		resetBumpBrickTime = 0.5f;
	}

	void Start () {
		if ( isRandomBrick ) SetRandomBrickState ();
		else InitializeBrickState (brickState);
	}

	void Update () {
		if ( _isMovingCoinAnim ) MoveCoinOrFireFlower_BrickAnim (speed_coinOrFireFlowerBrickAnim);
		if ( _isMovingPickupAnim ) MovePickup_BrickAnim (speed_PickupBrickAnim);
	}

	void SetRandomBrickState () {
		System.Array enumValues = System.Enum.GetValues ( typeof (RandomBrickState) );
		RandomBrickState randomState = (RandomBrickState)enumValues.GetValue ( UnityEngine.Random.Range (0, enumValues.Length) );
		InitializeRandomBrickState (randomState);
	}

	private void FnRandomBrickState (BrickState s, int i) {
		brickState = s;
		bounceCount = i;
	}

	private void InitializeRandomBrickState (RandomBrickState rbS) {
        switch ( rbS ) {
            case RandomBrickState.BrickNormal1:
				FnRandomBrickState (BrickState.BrickNormal, 0);
                break;
			case RandomBrickState.BrickNormal2:
                FnRandomBrickState (BrickState.BrickNormal, 0);
                break;
			case RandomBrickState.BrickNormal3:
                FnRandomBrickState (BrickState.BrickNormal, 0);
                break;
			case RandomBrickState.BrickNormal4:
                FnRandomBrickState (BrickState.BrickNormal, 0);
                break;
            case RandomBrickState.BrickCoin1:
				FnRandomBrickState (BrickState.BrickCoin, defaultCoinCount);
                break;
			case RandomBrickState.BrickCoin2:
                FnRandomBrickState (BrickState.BrickCoin, defaultCoinCount);
                break;
			case RandomBrickState.BrickCoin3:
                FnRandomBrickState (BrickState.BrickCoin, defaultCoinCount);
                break;
			case RandomBrickState.BrickCoin4:
                FnRandomBrickState (BrickState.BrickCoin, defaultCoinCount);
                break;
			case RandomBrickState.BrickCoin5:
                FnRandomBrickState (BrickState.BrickCoin, defaultCoinCount);
                break;
			case RandomBrickState.BrickCoin6:
                FnRandomBrickState (BrickState.BrickCoin, defaultCoinCount);
                break;
			case RandomBrickState.BrickCoin7:
                FnRandomBrickState (BrickState.BrickCoin, defaultCoinCount);
                break;
			case RandomBrickState.BrickCoin8:
                FnRandomBrickState (BrickState.BrickCoin, defaultCoinCount);
                break;
			case RandomBrickState.BrickCoin9:
                FnRandomBrickState (BrickState.BrickCoin, defaultCoinCount);
                break;
			case RandomBrickState.BrickCoin10:
                FnRandomBrickState (BrickState.BrickCoin, defaultCoinCount);
                break;
            case RandomBrickState.BrickMushroom1:
                FnRandomBrickState (BrickState.BrickMushroom, 1);
                break;
			case RandomBrickState.BrickMushroom2:
                FnRandomBrickState (BrickState.BrickMushroom, 1);
                break;
            case RandomBrickState.Brick1up1:
                FnRandomBrickState (BrickState.Brick1up, 1);
                break;
			case RandomBrickState.BrickFireFlower1:
                FnRandomBrickState (BrickState.BrickFireFlower, 1);
                break;
			case RandomBrickState.BrickFireFlower2:
                FnRandomBrickState (BrickState.BrickFireFlower, 1);
                break;
            default:
                brickState = BrickState.BrickNormal;
                break;
        }
		Debug.Log ( "Random-Brick state: " + brickState );
	}

	void InitializeBrickState (BrickState bS) {
        switch ( bS ) {
			case BrickState.BrickSolid:
				brickState = bS;
				bounceCount = 0;
				_materials[0] = brick_solid;
				_meshRenderer.materials = _materials;
				break;
            case BrickState.BrickNormal:
                brickState = bS;
				bounceCount = 0;
                break;
            case BrickState.BrickCoin:
                brickState = bS;
				bounceCount = defaultCoinCount;
                break;
            case BrickState.BrickMushroom:
                brickState = bS;
				bounceCount = 1;
                break;
            case BrickState.Brick1up:
                brickState = bS;
				bounceCount = 1;
                break;
			case BrickState.BrickFireFlower:
                brickState = bS;
				bounceCount = 1;
                break;
            default:
                brickState = BrickState.BrickNormal;
                break;
        }
		Debug.Log ( "Brick state: " + brickState );
	}

	void DisableGameObject (bool b, AudioClip a) {
        _audioScript.PlayAudio (a);
        _meshRenderer.enabled = !b;
        _thisCollider.enabled = !b;
	}

	void AnimateBrickSolid () {
		_audioScript.PlayAudioWaitToFinishClip (soundBrickSolid);
	}

	void AnimateBrickNormal () {
		_animator.Play ("BrickBounce");
		if ( isRandomBrick ) InitializeBrickState (BrickState.BrickSolid);

		if ( _playerController.PlayerNotSmall () ) {
			_audioScript.PlayAudioWaitToFinishClip (soundBrickDestroy);
			SpawnBrickParticles ();
			DisableGameObject (true, soundBrickDestroy);
			Destroy (gameObject, _audioScript.audioSource.clip.length - 0.1f);
		} else {
			_audioScript.PlayAudio (soundBrickSolid);
		}
	}

	void AnimateBrickCoin (int i) {
		SpawnCoinOrFireFlower_BrickAnim (true, coinBrickAnim, 0f, 0.85f, -1f);
		if ( i <= 1 ) {
			_playerController.UpdatePlayerCoins (1);
			_audioScript.PlayAudio (soundCoin);
			InitializeBrickState (BrickState.BrickSolid);
		} else {
			_animator.Play ("BrickBounce");
			i--;
			_playerController.UpdatePlayerCoins (1);
			_audioScript.PlayAudio (soundCoin);
			Debug.Log ("i: " + i);
			bounceCount = i;
		}
	}

	void AnimateBrickFireFlower () {
		SpawnCoinOrFireFlower_BrickAnim (false, fireFlower, 0f, 0.8f, 1f);
		_audioScript.PlayAudio (soundPickup);
		InitializeBrickState (BrickState.BrickSolid);
	}

	void AnimateBrickMushroomOr1up (GameObject pickup) {
		SpawnMushroomOr1up (pickup, 0f, 0.75f, 0f);
		_audioScript.PlayAudio (soundPickup);
		InitializeBrickState (BrickState.BrickSolid);
	}

	void SpawnBrickParticles () {
		_newBrickParticlesAnim = Instantiate (brickParticles, transform.position, Quaternion.identity);
		_newBrickParticlesAnim.GetComponent<ParticleSystem> ().Play ();
		//Destroy (_newBrickParticlesAnim, 3f);
	}

	void SpawnMushroomOr1up (GameObject pickup, float x, float y, float z) {
		_newPickup = Instantiate (pickup, transform.position, Quaternion.identity);
		_moveToPickupPosition = new Vector3 (transform.position.x + x, transform.position.y + y, transform.position.z + z);
		_isMovingPickupAnim = true;
	}

	void SpawnCoinOrFireFlower_BrickAnim (bool b, GameObject pickup, float x, float y, float z) {	// TRUE for CoinBrickAnim, FALSE for FireFlower
        _newCoinBrickAnim = Instantiate (pickup, transform.position, Quaternion.identity);
		_moveToCoinBrickAnimPosition = new Vector3 (transform.position.x + x, transform.position.y + y, transform.position.z + z);
		_isMovingCoinAnim = true;
		if ( b ) Destroy (_newCoinBrickAnim, destroyObjectTime);
	}

	void MoveCoinOrFireFlower_BrickAnim (float speed) {
		if ( _newCoinBrickAnim.transform.position != _moveToCoinBrickAnimPosition ) _newCoinBrickAnim.transform.position = Vector3.MoveTowards ( _newCoinBrickAnim.transform.position, _moveToCoinBrickAnimPosition, speed * Time.deltaTime );
		else _isMovingCoinAnim = false;
	}

	void MovePickup_BrickAnim (float speed) {
		if ( _newPickup.transform.position != _moveToPickupPosition ) _newPickup.transform.position = Vector3.MoveTowards ( _newPickup.transform.position, _moveToPickupPosition, speed * Time.deltaTime );
		else {
			_newPickup.GetComponent<Rigidbody2D> ().simulated = true;
			_newPickup.GetComponent<PickupScript> ().isMovingPickup = true;
			_isMovingPickupAnim = false;
		}
	}

	void OnTriggerEnter2D (Collider2D other) {
		if ( other.gameObject.name == "PlayerHead" ) {
			Debug.Log ("Player hits brick with head!");
			if ( gameObject.name == "brick_solid" ) {
				_audioScript.PlayAudio (soundBrickSolid);
			} else {
				playerHasBumpedBrick = true;	// USED FOR HITTING ENEMIES FROM BELOW
				_playerController = other.GetComponentInParent<PlayerController> ();
				if ( brickState == BrickState.BrickSolid ) AnimateBrickSolid ();
				else if ( brickState == BrickState.BrickNormal ) AnimateBrickNormal ();
				else if ( brickState == BrickState.BrickCoin ) AnimateBrickCoin (bounceCount);
				else if ( brickState == BrickState.BrickMushroom ) AnimateBrickMushroomOr1up (mushroom);
				else if ( brickState == BrickState.Brick1up ) AnimateBrickMushroomOr1up (m1up);
				else if ( brickState == BrickState.BrickFireFlower ) AnimateBrickFireFlower ();
				StartCoroutine ( ResetBumpBrick (resetBumpBrickTime) );
			}
		}
	}

	void OnCollisionStay2D (Collision2D other) {
		if ( other.gameObject.tag == TagScript.ENEMY_TAG ) {
			_enemyScript = other.gameObject.GetComponent<EnemyScript> ();
			if ( playerHasBumpedBrick ) _enemyScript.BounceKillEnemy ();
		}

		if ( other.gameObject.tag == TagScript.PICKUP_MUSHROOM_TAG || other.gameObject.tag == TagScript.PICKUP_1UP_TAG ) {
			_pickupScript = other.gameObject.GetComponent<PickupScript> ();
			if ( playerHasBumpedBrick ) _pickupScript.ChangePickupMoveDirection ();
		}
	}

	IEnumerator ResetBumpBrick (float t) {
		yield return new WaitForSeconds (t);
		playerHasBumpedBrick = false;
	}

} // end of class
