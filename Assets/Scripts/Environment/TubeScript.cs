using UnityEngine;
using UnityEngine.InputSystem;

public class TubeScript : MonoBehaviour
{
    private bool _isPlayerRedEnteringTube, _isPlayerBlueEnteringTube;
    private bool _isPlayerRedArrivedInTube, _isPlayerBlueArrivedInTube;
    private bool _isMovingPlayersTubeToTube, _isMovingPlayersOutOfTube;

    [SerializeField] private float speed_movePlayerInOutTubeAnim, speed_movePlayerTubeToTubeAnim, tubeEntryExit_y;

    private GameObject _gameObjectPlayerRed, _gameObjectPlayerBlue;
    public Transform otherTubeExitTransform;
    public AudioClip soundTube;
    private AudioScript _audioScript;
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

        speed_movePlayerInOutTubeAnim = 2f;
        speed_movePlayerTubeToTubeAnim = 10f;
        tubeEntryExit_y = 0.7f;
    }

    void Update()
    {
        if (_isPlayerRedEnteringTube) PerPlayerEnterTube(_gameObjectPlayerRed, _tubeEntryPosition, ref _isPlayerRedEnteringTube, ref _isPlayerRedArrivedInTube);
        if (_isPlayerBlueEnteringTube) PerPlayerEnterTube(_gameObjectPlayerBlue, _tubeEntryPosition, ref _isPlayerBlueEnteringTube, ref _isPlayerBlueArrivedInTube);

        if (_isPlayerRedArrivedInTube && _isPlayerBlueArrivedInTube && !_isMovingPlayersTubeToTube && !_isMovingPlayersOutOfTube)
            PreparePlayersForTubeTravel();

        if (_isMovingPlayersTubeToTube) MovePlayersTubeToTube(speed_movePlayerTubeToTubeAnim);
        else if (_isMovingPlayersOutOfTube) MovePlayersOutOfTube(speed_movePlayerInOutTubeAnim);
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

    AudioScript GetAudioScript()
    {
        if (_audioScript == null && GameManager.Instance != null)
            _audioScript = GameManager.Instance.GetComponent<AudioScript>();
        return _audioScript;
    }

    void PreparePlayersForTubeTravel()
    {
        _playerRedTubeToTubePosition = new Vector3(otherTubeExitTransform.transform.position.x, otherTubeExitTransform.transform.position.y - tubeEntryExit_y, otherTubeExitTransform.transform.position.z);
        _playerBlueTubeToTubePosition = _playerRedTubeToTubePosition;
        _isMovingPlayersTubeToTube = true;
    }

    void PreparePlayersForTubeExit()
    {
        if (GetAudioScript() != null) _audioScript.PlayAudio(soundTube);
        _playerRedOutOfTubePosition = new Vector3(otherTubeExitTransform.transform.position.x, otherTubeExitTransform.transform.position.y + tubeEntryExit_y, otherTubeExitTransform.transform.position.z);
        _playerBlueOutOfTubePosition = _playerRedOutOfTubePosition;
    }

    void PerPlayerEnterTube(GameObject player, Vector3 targetPos, ref bool entering, ref bool arrived)
    {
        float dt = speed_movePlayerInOutTubeAnim * Time.deltaTime;
        player.transform.position = Vector3.MoveTowards(player.transform.position, targetPos, dt);
        if (player.transform.position == targetPos)
        {
            player.SetActive(false);
            entering = false;
            arrived = true;
        }
    }

    void MovePlayersTubeToTube(float speed)
    {
        float dt = speed * Time.deltaTime;

        bool _gameObjectPlayerRedArrived = _gameObjectPlayerRed.transform.position == _playerRedTubeToTubePosition;
        bool _gameObjectPlayerBlueArrived = _gameObjectPlayerBlue.transform.position == _playerBlueTubeToTubePosition;

        if (!_gameObjectPlayerRedArrived)
            _gameObjectPlayerRed.transform.position = Vector3.MoveTowards(_gameObjectPlayerRed.transform.position, _playerRedTubeToTubePosition, dt);
        if (!_gameObjectPlayerBlueArrived)
            _gameObjectPlayerBlue.transform.position = Vector3.MoveTowards(_gameObjectPlayerBlue.transform.position, _playerBlueTubeToTubePosition, dt);

        if (_gameObjectPlayerRedArrived && _gameObjectPlayerBlueArrived)
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

        bool _gameObjectPlayerRedArrived = _gameObjectPlayerRed.transform.position == _playerRedOutOfTubePosition;
        bool _gameObjectPlayerBlueArrived = _gameObjectPlayerBlue.transform.position == _playerBlueOutOfTubePosition;

        if (!_gameObjectPlayerRedArrived)
            _gameObjectPlayerRed.transform.position = Vector3.MoveTowards(_gameObjectPlayerRed.transform.position, _playerRedOutOfTubePosition, dt);
        if (!_gameObjectPlayerBlueArrived)
            _gameObjectPlayerBlue.transform.position = Vector3.MoveTowards(_gameObjectPlayerBlue.transform.position, _playerBlueOutOfTubePosition, dt);

        if (_gameObjectPlayerRedArrived && _gameObjectPlayerBlueArrived)
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

    void OnTriggerStay2D(Collider2D other)
    {
        if (other.gameObject.layer != LayerMask.NameToLayer("Player")) return;

        PlayerController pc = other.GetComponentInParent<PlayerController>();
        if (pc == null) return;

        Keyboard kb = Keyboard.current;
        if (kb == null) return;

        if (pc.assignedPlayerCharacter == PlayerController.PlayerCharacter.Red && !_isPlayerRedEnteringTube && !_isPlayerRedArrivedInTube)
        {
            if (kb.sKey.wasPressedThisFrame)
            {
                _gameObjectPlayerRed = pc.gameObject;
                DisablePlayer(_gameObjectPlayerRed);
                _tubeEntryPosition = new Vector3(transform.position.x, transform.position.y - tubeEntryExit_y, transform.position.z + 1f);
                _isPlayerRedEnteringTube = true;
                if (GetAudioScript() != null) _audioScript.PlayAudio(soundTube);
            }
        }
        else if (pc.assignedPlayerCharacter == PlayerController.PlayerCharacter.Blue && !_isPlayerBlueEnteringTube && !_isPlayerBlueArrivedInTube)
        {
            if (kb.downArrowKey.wasPressedThisFrame)
            {
                _gameObjectPlayerBlue = pc.gameObject;
                DisablePlayer(_gameObjectPlayerBlue);
                _tubeEntryPosition = new Vector3(transform.position.x, transform.position.y - tubeEntryExit_y, transform.position.z + 1f);
                _isPlayerBlueEnteringTube = true;
                if (GetAudioScript() != null) _audioScript.PlayAudio(soundTube);
            }
        }
    }

} // end of class
