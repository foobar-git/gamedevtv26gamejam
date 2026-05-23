using UnityEngine;

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
    // shared state — distinct from per-player lives and coins tracked inside PlayerController
    public int sharedLives = 3;
    public int sharedCoins = 0;

    [Header("Start Point")]
    public Transform startPointTransform;

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
}
