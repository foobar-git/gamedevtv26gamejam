using UnityEngine;
using TMPro;

// Central singleton and access point for shared game state. Wires both players
// into the camera on Start.
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Characters")]
    public PlayerController playerControllerRed;
    public PlayerController playerControllerBlue;

    [Header("Camera")]
    public CameraScript cameraScript;

    [Header("State")]
    public int sharedLives = 3;
    public int sharedCoins = 0;

    [Header("HUD")]
    // single-player mode: one shared display for both characters
    // two-player mode: replace with hudLivesRed and hudLivesBlue (see InitializeTwoPlayerMode)
    public TextMeshPro hudLives;

    [Header("Audio")]
    public AudioClip backgroundMusic;

    [Header("Start Point")]
    public Transform startPointTransform;
    // tracks the last touched save point — initialized to startPointTransform so it is never null
    public Transform currentSavePointTransform;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        currentSavePointTransform = startPointTransform;
        InitializeSinglePlayerMode();
        // TODO: [Phase X] - uncomment for two-player mode (and comment out line above)
        // InitializeTwoPlayerMode();
        InitializeCamera();
        if (backgroundMusic != null && AudioScript.Instance != null)
        {
            AudioScript.Instance.PlayMusic(backgroundMusic);
        }
    }

    void InitializeSinglePlayerMode()
    {
        // both characters draw from one shared lives pool and one shared HUD display
        if (playerControllerRed != null)
        {
            playerControllerRed.SetupSharedLives();
        }
        if (playerControllerBlue != null)
        {
            playerControllerBlue.SetupSharedLives();
        }
        UpdateSharedLivesDisplay();
    }

    void InitializeTwoPlayerMode()
    {
        // each character owns its lives and HUD reference independently
        // 1. Replace hudLives above with: public TextMeshPro hudLivesRed; public TextMeshPro hudLivesBlue;
        // 2. Assign each in the Inspector
        // 3. Uncomment the calls below:
        // if (playerControllerRed != null) { playerControllerRed.SetupOwnLives(3, hudLivesRed); }
        // if (playerControllerBlue != null) { playerControllerBlue.SetupOwnLives(3, hudLivesBlue); }
    }

    void InitializeCamera()
    {
        if (cameraScript == null)
        {
            return;
        }
        // defensive — guards against future changes to CameraScript's field initialization order
        if (cameraScript.cameraTargetList == null)
        {
            cameraScript.cameraTargetList = new System.Collections.Generic.List<Transform>();
        }
        if (playerControllerRed != null)
        {
            cameraScript.AddCameraTarget(playerControllerRed.transform);
        }
        if (playerControllerBlue != null)
        {
            cameraScript.AddCameraTarget(playerControllerBlue.transform);
        }
    }

    public void UpdateSharedLives(int i)
    {
        sharedLives += i;
        UpdateSharedLivesDisplay();
        if (sharedLives < 0)
        {
            TriggerGameOver();
        }
    }

    void TriggerGameOver()
    {
        // force both characters to die — whoever triggered this is already guarded by _playerDied
        if (playerControllerRed != null)
        {
            playerControllerRed.ForcePlayerDied();
        }
        if (playerControllerBlue != null)
        {
            playerControllerBlue.ForcePlayerDied();
        }
    }

    // propagates a save point touch to whichever player didn't trigger it
    public void UpdateOtherPlayerSavePoint(PlayerController triggeringPlayer, Transform savePoint)
    {
        currentSavePointTransform = savePoint;
        if (playerControllerRed != triggeringPlayer && playerControllerRed != null)
        {
            playerControllerRed.SetSavePoint(savePoint);
        }
        if (playerControllerBlue != triggeringPlayer && playerControllerBlue != null)
        {
            playerControllerBlue.SetSavePoint(savePoint);
        }
    }

    public PlayerController GetOtherPlayer(PlayerController askingPlayer)
    {
        if (askingPlayer == playerControllerRed)
        {
            return playerControllerBlue;
        }
        if (askingPlayer == playerControllerBlue)
        {
            return playerControllerRed;
        }
        return null;
    }

    public bool BothPlayersDead()
    {
        bool redDead = playerControllerRed != null && playerControllerRed.IsPlayerDead;
        bool blueDead = playerControllerBlue != null && playerControllerBlue.IsPlayerDead;
        return redDead && blueDead;
    }

    // called when both players are dead — disables auto move and snaps camera to save point
    public void OnBothPlayersDied()
    {
        if (cameraScript == null)
        {
            return;
        }
        cameraScript.DisableAutoMoveTemporarily();
        if (currentSavePointTransform != null)
        {
            cameraScript.SnapToPosition(currentSavePointTransform);
        }
    }

    void UpdateSharedLivesDisplay()
    {
        if (hudLives != null)
        {
            hudLives.text = Mathf.Max(0, sharedLives).ToString();
        }
    }
}
