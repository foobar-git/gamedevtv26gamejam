using UnityEngine;

// Manages the two-player tube warp sequence. Both players must press down to enter
// before travel begins — neither teleports until both are inside the tube.
public class TubeScript : MonoBehaviour
{
    private bool _isPlayerRedEnteringTube, _isPlayerBlueEnteringTube;
    private bool _isPlayerRedArrivedInTube, _isPlayerBlueArrivedInTube;
    private bool _isMovingPlayersTubeToTube, _isMovingPlayersOutOfTube;

    [SerializeField] private bool _canEnterTube = true;

    [SerializeField] private float speed_movePlayerInOutTubeAnim, speed_movePlayerTubeToTubeAnim, tubeEntryExit_y;

    private GameObject _gameObjectPlayerRed, _gameObjectPlayerBlue;
    public Transform otherTubeExitTransform;
    public AudioClip soundTube;
    private Vector3 _tubeEntryPosition;
    private Vector3 _playerRedTubeToTubePosition, _playerBlueTubeToTubePosition;
    private Vector3 _playerRedOutOfTubePosition, _playerBlueOutOfTubePosition;

    ////////////////////////////////////////////////////////////////////////////////////////

    void Awake()
    {
        _isPlayerRedEnteringTube = false;
        _isPlayerBlueEnteringTube = false;
        _isPlayerRedArrivedInTube = false;
        _isPlayerBlueArrivedInTube = false;
        _isMovingPlayersTubeToTube = false;
        _isMovingPlayersOutOfTube = false;

        // entry/exit is slow and visible (2f); tube-to-tube jump is near-instant (10f)
        speed_movePlayerInOutTubeAnim = 2f;
        speed_movePlayerTubeToTubeAnim = 10f;
        tubeEntryExit_y = 0.7f;
    }

    void Update()
    {
        if (_isPlayerRedEnteringTube)
        {
            PerPlayerEnterTube(_gameObjectPlayerRed, _tubeEntryPosition, ref _isPlayerRedEnteringTube, ref _isPlayerRedArrivedInTube);
        }
        if (_isPlayerBlueEnteringTube)
        {
            PerPlayerEnterTube(_gameObjectPlayerBlue, _tubeEntryPosition, ref _isPlayerBlueEnteringTube, ref _isPlayerBlueArrivedInTube);
        }

        // both players must be inside before the tube-to-tube jump fires
        if (_isPlayerRedArrivedInTube && _isPlayerBlueArrivedInTube && !_isMovingPlayersTubeToTube && !_isMovingPlayersOutOfTube)
        {
            PreparePlayersForTubeTravel();
        }

        if (_isMovingPlayersTubeToTube)
        {
            MovePlayersTubeToTube(speed_movePlayerTubeToTubeAnim);
        }
        else if (_isMovingPlayersOutOfTube)
        {
            MovePlayersOutOfTube(speed_movePlayerInOutTubeAnim);
        }
    }

    void DisablePlayer(GameObject player)
    {
        player.GetComponent<PlayerController>().playerControlsEnabled = false;
        player.GetComponent<Rigidbody2D>().simulated = false;
    }

    void EnablePlayer(GameObject player)
    {
        player.GetComponent<Rigidbody2D>().simulated = true;
        player.GetComponent<PlayerController>().playerControlsEnabled = true;
    }

    void PreparePlayersForTubeTravel()
    {
        // both players warp to the same destination since they always travel together
        _playerRedTubeToTubePosition = new Vector3(otherTubeExitTransform.position.x, otherTubeExitTransform.position.y - tubeEntryExit_y, otherTubeExitTransform.position.z);
        _playerBlueTubeToTubePosition = _playerRedTubeToTubePosition;
        _isMovingPlayersTubeToTube = true;
    }

    void PreparePlayersForTubeExit()
    {
        AudioScript.Instance.PlayAudio(soundTube);
        _playerRedOutOfTubePosition = new Vector3(otherTubeExitTransform.position.x, otherTubeExitTransform.position.y + tubeEntryExit_y, otherTubeExitTransform.position.z);
        _playerBlueOutOfTubePosition = _playerRedOutOfTubePosition;
    }

    void PerPlayerEnterTube(GameObject player, Vector3 targetPos, ref bool entering, ref bool arrived)
    {
        float dt = speed_movePlayerInOutTubeAnim * Time.deltaTime;
        player.transform.position = Vector3.MoveTowards(player.transform.position, targetPos, dt);
        if (player.transform.position == targetPos)
        {
            // hide the player during the instant tube-to-tube teleport
            player.SetActive(false);
            entering = false;
            arrived = true;
        }
    }

    void MovePlayersTubeToTube(float speed)
    {
        float dt = speed * Time.deltaTime;

        bool playerRedArrived = _gameObjectPlayerRed.transform.position == _playerRedTubeToTubePosition;
        bool playerBlueArrived = _gameObjectPlayerBlue.transform.position == _playerBlueTubeToTubePosition;

        if (!playerRedArrived)
        {
            _gameObjectPlayerRed.transform.position = Vector3.MoveTowards(_gameObjectPlayerRed.transform.position, _playerRedTubeToTubePosition, dt);
        }
        if (!playerBlueArrived)
        {
            _gameObjectPlayerBlue.transform.position = Vector3.MoveTowards(_gameObjectPlayerBlue.transform.position, _playerBlueTubeToTubePosition, dt);
        }

        if (playerRedArrived && playerBlueArrived)
        {
            _isMovingPlayersTubeToTube = false;
            PreparePlayersForTubeExit();
            _isMovingPlayersOutOfTube = true;
        }
    }

    void MovePlayersOutOfTube(float speed)
    {
        float dt = speed * Time.deltaTime;

        _gameObjectPlayerRed.SetActive(true);
        _gameObjectPlayerBlue.SetActive(true);

        bool playerRedArrived = _gameObjectPlayerRed.transform.position == _playerRedOutOfTubePosition;
        bool playerBlueArrived = _gameObjectPlayerBlue.transform.position == _playerBlueOutOfTubePosition;

        if (!playerRedArrived)
        {
            _gameObjectPlayerRed.transform.position = Vector3.MoveTowards(_gameObjectPlayerRed.transform.position, _playerRedOutOfTubePosition, dt);
        }
        if (!playerBlueArrived)
        {
            _gameObjectPlayerBlue.transform.position = Vector3.MoveTowards(_gameObjectPlayerBlue.transform.position, _playerBlueOutOfTubePosition, dt);
        }

        if (playerRedArrived && playerBlueArrived)
        {
            _isMovingPlayersOutOfTube = false;
            EnablePlayer(_gameObjectPlayerRed);
            EnablePlayer(_gameObjectPlayerBlue);
            _isPlayerRedEnteringTube = false;
            _isPlayerBlueEnteringTube = false;
            _isPlayerRedArrivedInTube = false;
            _isPlayerBlueArrivedInTube = false;
        }
    }

    // OnTriggerStay2D so the player can press down at any moment while standing in the
    // zone — not only on the first frame of contact
    void OnTriggerStay2D(Collider2D other)
    {
        if (other.gameObject.layer != LayerMask.NameToLayer("Player"))
        {
            return;
        }

        PlayerController pc = other.GetComponentInParent<PlayerController>();
        if (pc == null)
        {
            return;
        }

        if (!_canEnterTube)
        {
            return;
        }

        if (pc.assignedPlayerCharacter == PlayerController.PlayerCharacter.Red && !_isPlayerRedEnteringTube && !_isPlayerRedArrivedInTube)
        {
            if (InputProvider.Instance != null && InputProvider.Instance.GetRedInput().shootPressed)
            {
                _gameObjectPlayerRed = pc.gameObject;
                DisablePlayer(_gameObjectPlayerRed);
                // z + 1 places the entry target slightly in front so the player slides visually into the tube
                _tubeEntryPosition = new Vector3(transform.position.x, transform.position.y - tubeEntryExit_y, transform.position.z + 1f);
                _isPlayerRedEnteringTube = true;
                AudioScript.Instance.PlayAudio(soundTube);
            }
        }
        else if (pc.assignedPlayerCharacter == PlayerController.PlayerCharacter.Blue && !_isPlayerBlueEnteringTube && !_isPlayerBlueArrivedInTube)
        {
            if (InputProvider.Instance != null && InputProvider.Instance.GetBlueInput().shootPressed)
            {
                _gameObjectPlayerBlue = pc.gameObject;
                DisablePlayer(_gameObjectPlayerBlue);
                _tubeEntryPosition = new Vector3(transform.position.x, transform.position.y - tubeEntryExit_y, transform.position.z + 1f);
                _isPlayerBlueEnteringTube = true;
                AudioScript.Instance.PlayAudio(soundTube);
            }
        }
    }

} // end of class
