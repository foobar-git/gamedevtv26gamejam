using UnityEngine;

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
        if (cameraScript == null) return;

        if (cameraScript.cameraTargetList == null)
            cameraScript.cameraTargetList = new System.Collections.Generic.List<Transform>();

        if (playerControllerRed != null)
            cameraScript.AddCameraTarget(playerControllerRed.transform);
        if (playerControllerBlue != null)
            cameraScript.AddCameraTarget(playerControllerBlue.transform);
    }
}
