using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrickScript : MonoBehaviour {
	
	[SerializeField] private int defaultCoinCount, bounceCount;
	[SerializeField] private float destroyObjectTime, speed_coinOrFireFlowerBrickAnim, speed_PickupBrickAnim, resetBumpBrickTime;

	private PlayerController playerScript;
	private EnemyScript enemyScript;
	private PickupScript pickupScript;
	private MeshRenderer meshRenderer;
	private Material[] materials;
    private Collider2D thisCollider;
	private Animator animator;
	private AudioScript audioScript;
	public AudioClip soundBrickSolid, soundBrickDestroy, soundPickup, soundCoin;
	public Material brick_solid;
	public GameObject coinBrickAnim, fireFlower, mushroom, m1up, brickParticles;
	private GameObject newCoinBrickAnim, newPickup, newBrickParticlesAnim;

	private Vector3 moveToCoinBrickAnimPosition, moveToPickupPosition;

	public bool playerBumpsBrick, randomBrick;
	private bool moveCoinOrFireFlower_BrickAnim, movePickup_BrickAnim;

	// workaround for probability of random-brick states:
	private enum RandomBrickState { BrickNormal1, BrickNormal2, BrickNormal3, BrickNormal4, BrickCoin1, BrickCoin2, BrickCoin3, BrickCoin4, BrickCoin5, BrickCoin6,
		BrickCoin7, BrickCoin8, BrickCoin9, BrickCoin10, BrickMushroom1, BrickMushroom2, Brick1up1, BrickFireFlower1, BrickFireFlower2 }
    public enum BrickState	{ BrickSolid, BrickNormal, BrickCoin, BrickMushroom, Brick1up, BrickFireFlower }
    public BrickState brickState;

	////////////////////////////////////////////////////////////////////////////////////////

	void Awake () {
        audioScript = GetComponent<AudioScript> ();
		animator = GetComponent<Animator> ();
		meshRenderer = GetComponent<MeshRenderer> ();
		materials = meshRenderer.materials;
        thisCollider = GetComponent<Collider2D> ();

		playerBumpsBrick = false;
		moveCoinOrFireFlower_BrickAnim = false;
		movePickup_BrickAnim = false;

		defaultCoinCount = 3;
		bounceCount = defaultCoinCount;
		destroyObjectTime = 1f;
		speed_coinOrFireFlowerBrickAnim = 2f;
		speed_PickupBrickAnim = 1f;
		resetBumpBrickTime = 0.5f;
	}

	void Start () {
		if ( randomBrick ) SetRandomBrickState ();
		else InitializeBrickState (brickState);
	}

	void Update () {
		if ( moveCoinOrFireFlower_BrickAnim ) MoveCoinOrFireFlower_BrickAnim (speed_coinOrFireFlowerBrickAnim);
		if ( movePickup_BrickAnim ) MovePickup_BrickAnim (speed_PickupBrickAnim);
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
				materials[0] = brick_solid;
				meshRenderer.materials = materials;
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
        audioScript.PlayAudio (a);
        meshRenderer.enabled = !b;
        thisCollider.enabled = !b;
	}

	void AnimateBrickSolid () {
		audioScript.PlayAudioWaitToFinishClip (soundBrickSolid);
	}

	void AnimateBrickNormal () {
		animator.Play ("BrickBounce");
		if ( randomBrick ) InitializeBrickState (BrickState.BrickSolid);

		if ( playerScript.PlayerNotSmall () ) {
			audioScript.PlayAudioWaitToFinishClip (soundBrickDestroy);
			SpawnBrickParticles ();
			DisableGameObject (true, soundBrickDestroy);
			Destroy (gameObject, audioScript.audioSource.clip.length - 0.1f);
		} else {
			audioScript.PlayAudio (soundBrickSolid);
		}
	}

	void AnimateBrickCoin (int i) {
		SpawnCoinOrFireFlower_BrickAnim (true, coinBrickAnim, 0f, 0.85f, -1f);
		if ( i <= 1 ) {
			playerScript.UpdatePlayerCoins (1);
			audioScript.PlayAudio (soundCoin);
			InitializeBrickState (BrickState.BrickSolid);
		} else {
			animator.Play ("BrickBounce");
			i--;
			playerScript.UpdatePlayerCoins (1);
			audioScript.PlayAudio (soundCoin);
			Debug.Log ("i: " + i);
			bounceCount = i;
		}
	}

	void AnimateBrickFireFlower () {
		SpawnCoinOrFireFlower_BrickAnim (false, fireFlower, 0f, 0.8f, 1f);
		audioScript.PlayAudio (soundPickup);
		InitializeBrickState (BrickState.BrickSolid);
	}

	void AnimateBrickMushroomOr1up (GameObject pickup) {
		SpawnMushroomOr1up (pickup, 0f, 0.75f, 0f);
		audioScript.PlayAudio (soundPickup);
		InitializeBrickState (BrickState.BrickSolid);
	}

	void SpawnBrickParticles () {
		newBrickParticlesAnim = Instantiate (brickParticles, transform.position, Quaternion.identity);
		newBrickParticlesAnim.GetComponent<ParticleSystem> ().Play ();
		//Destroy (newBrickParticlesAnim, 3f);
	}

	void SpawnMushroomOr1up (GameObject pickup, float x, float y, float z) {
		newPickup = Instantiate (pickup, transform.position, Quaternion.identity);
		moveToPickupPosition = new Vector3 (transform.position.x + x, transform.position.y + y, transform.position.z + z);
		movePickup_BrickAnim = true;
	}

	void SpawnCoinOrFireFlower_BrickAnim (bool b, GameObject pickup, float x, float y, float z) {	// TRUE for CoinBrickAnim, FALSE for FireFlower
        newCoinBrickAnim = Instantiate (pickup, transform.position, Quaternion.identity);
		moveToCoinBrickAnimPosition = new Vector3 (transform.position.x + x, transform.position.y + y, transform.position.z + z);
		moveCoinOrFireFlower_BrickAnim = true;
		if ( b ) Destroy (newCoinBrickAnim, destroyObjectTime);
	}

	void MoveCoinOrFireFlower_BrickAnim (float speed) {
		if ( newCoinBrickAnim.transform.position != moveToCoinBrickAnimPosition ) newCoinBrickAnim.transform.position = Vector3.MoveTowards ( newCoinBrickAnim.transform.position, moveToCoinBrickAnimPosition, speed * Time.deltaTime );
		else moveCoinOrFireFlower_BrickAnim = false;
	}

	void MovePickup_BrickAnim (float speed) {
		if ( newPickup.transform.position != moveToPickupPosition ) newPickup.transform.position = Vector3.MoveTowards ( newPickup.transform.position, moveToPickupPosition, speed * Time.deltaTime );
		else {
			newPickup.GetComponent<Rigidbody2D> ().simulated = true;
			newPickup.GetComponent<PickupScript> ().movePickup = true;
			movePickup_BrickAnim = false;
		}
	}

	void OnTriggerEnter2D (Collider2D other) {
		if ( other.gameObject.name == "PlayerHead" ) {
			Debug.Log ("Player hits brick with head!");
			if ( gameObject.name == "brick_solid" ) {
				audioScript.PlayAudio (soundBrickSolid);
			} else {
				playerBumpsBrick = true;	// USED FOR HITTING ENEMIES FROM BELOW
				playerScript = other.GetComponentInParent<PlayerController> ();
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
		if ( other.gameObject.tag == TagScript.EnemyTag ) {
			enemyScript = other.gameObject.GetComponent<EnemyScript> ();
			if ( playerBumpsBrick ) enemyScript.BounceKillEnemy ();
		}

		if ( other.gameObject.tag == TagScript.PickupMushroomTag || other.gameObject.tag == TagScript.Pickup1upTag ) {
			pickupScript = other.gameObject.GetComponent<PickupScript> ();
			if ( playerBumpsBrick ) pickupScript.ChangePickupMoveDirection ();
		}
	}

	IEnumerator ResetBumpBrick (float t) {
		yield return new WaitForSeconds (t);
		playerBumpsBrick = false;
	}

} // end of class
