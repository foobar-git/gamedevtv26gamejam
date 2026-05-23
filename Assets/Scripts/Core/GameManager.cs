using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Characters")]
    public PlayerController playerLeft;
    public PlayerController playerRight;

    [Header("Camera")]
    public CameraScript cameraScript;

    [Header("State")]
    public int sharedLives = 3;
    public int sharedCoins = 0;

    [Header("Start Point")]
    public Transform startPoint;

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

        if (playerLeft != null)
            cameraScript.AddCameraTarget(playerLeft.transform);
        if (playerRight != null)
            cameraScript.AddCameraTarget(playerRight.transform);
    }
}
