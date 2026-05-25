using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Follows both players with smooth-damped position and orthographic zoom that
// expands as the players move apart. Snaps to center on the first frame.
[RequireComponent(typeof(Camera))]
public class CameraScript : MonoBehaviour
{
    public float cameraSmoothTime, cameraZoom, maxZoom, minZoom, zoomLimiter;

    [Header("Auto Move")]
    [SerializeField] private bool isAutoMove;
    [SerializeField] private float autoMoveSpeed;

    public Vector3 offset;
    private Vector3 _cameraCenterPoint, _velocityVec;
    private Bounds _moveBounds, _zoomBounds;
    // prevents SmoothDamp from sliding in from the editor position on frame one
    private bool _hasSnapped;
    // stores isAutoMove state before a temporary disable — restored when players respawn
    private bool _savedAutoMoveState;
    // guards RestoreAutoMove so it's a no-op unless DisableAutoMoveTemporarily was actually called
    private bool _isAutoMoveTemporarilyDisabled;
    // tracks how many players are mid-death/respawn — kill zone stays off until this reaches 0
    private int _pendingRespawnCount;

    public List<Transform> cameraTargetList = new List<Transform>();
    // invisible collider walls that are children of the camera — they move with it automatically.
    // their local X is updated each frame to sit exactly at the camera edges, accounting for zoom.
    // layer matrix ensures only players collide with them — enemies and projectiles pass through.
    [SerializeField] private Transform _boundaryLeftTransform;
    [SerializeField] private Transform _boundaryRightTransform;
    [SerializeField] private GameObject gameObjectLeftKillZone;
    private Camera _mainCamera;

    ////////////////////////////////////////////////////////////////////////////////////////

    void Awake()
    {
        _mainCamera = GetComponent<Camera>();
        offset = new Vector3(0f, 0f, -10f);
        cameraSmoothTime = 0.5f;
        // counter-intuitive: in orthographic cameras a smaller size is more zoomed in
        // maxZoom (5) = closest view, minZoom (10) = furthest view
        minZoom = 10f;
        maxZoom = 5f;
        zoomLimiter = 30f;
    }

    void LateUpdate()
    {
        SnapCameraOnFirstFrame();
        MoveCamera();
        ZoomCamera();
        UpdateBoundaryPositions();
    }

    void SnapCameraOnFirstFrame()
    {
        if (_hasSnapped || cameraTargetList.Count <= 0)
        {
            return;
        }
        _cameraCenterPoint = GetCameraCenterPoint();
        transform.position = _cameraCenterPoint + offset;
        _hasSnapped = true;
    }

    public void AddCameraTarget(Transform playerTransform)
    {
        if (playerTransform != null)
        {
            cameraTargetList.Add(playerTransform);
        }
    }

    public void RemoveCameraTarget(Transform playerTransform)
    {
        cameraTargetList.Remove(playerTransform);
    }

    Vector3 GetCameraCenterPoint()
    {
        if (cameraTargetList.Count == 1)
        {
            return cameraTargetList[0].position;
        }
        _moveBounds = new Bounds(cameraTargetList[0].position, Vector3.zero);
        for (int i = 0; i < cameraTargetList.Count; i++)
        {
            _moveBounds.Encapsulate(cameraTargetList[i].position);
        }
        return _moveBounds.center;
    }

    float GetMaxPlayerDistance()
    {
        _zoomBounds = new Bounds(cameraTargetList[0].position, Vector3.zero);
        for (int i = 0; i < cameraTargetList.Count; i++)
        {
            _zoomBounds.Encapsulate(cameraTargetList[i].position);
        }
        return _zoomBounds.size.x;
    }

    void UpdateBoundaryPositions()
    {
        // half the camera width in world units — walls sit exactly at the visible edges.
        // recalculated every frame because orthographicSize changes as players spread apart.
        float halfWidth = _mainCamera.orthographicSize * _mainCamera.aspect;
        // only X is driven by code — Y and Z stay as placed in the editor
        if (_boundaryLeftTransform != null)
        {
            _boundaryLeftTransform.localPosition = new Vector3(-halfWidth, _boundaryLeftTransform.localPosition.y, _boundaryLeftTransform.localPosition.z);
        }
        if (_boundaryRightTransform != null)
        {
            _boundaryRightTransform.localPosition = new Vector3(halfWidth, _boundaryRightTransform.localPosition.y, _boundaryRightTransform.localPosition.z);
        }
    }

    void MoveCamera()
    {
        if (cameraTargetList.Count <= 0)
        {
            return;
        }
        _cameraCenterPoint = GetCameraCenterPoint();
        if (isAutoMove)
        {
            // X moves right at constant speed — players are pushed along by the boundary walls
            float targetX = transform.position.x + autoMoveSpeed * Time.deltaTime;
            // Y still follows players so jumps and falls stay on screen
            float targetY = Mathf.SmoothDamp(transform.position.y, _cameraCenterPoint.y + offset.y, ref _velocityVec.y, cameraSmoothTime);
            transform.position = new Vector3(targetX, targetY, transform.position.z);
        }
        else
        {
            transform.position = Vector3.SmoothDamp(transform.position, _cameraCenterPoint + offset, ref _velocityVec, cameraSmoothTime);
        }
    }

    void ZoomCamera()
    {
        if (cameraTargetList.Count <= 0)
        {
            return;
        }
        // lerps from close (maxZoom=5) to far (minZoom=10) as player distance grows;
        // zoomLimiter normalizes the raw distance into a 0-1 range for the lerp
        cameraZoom = Mathf.Lerp(maxZoom, minZoom, GetMaxPlayerDistance() / zoomLimiter);
        // Time.deltaTime smoothing is intentional — gives a soft, frame-rate-dependent ease
        _mainCamera.orthographicSize = Mathf.Lerp(_mainCamera.orthographicSize, cameraZoom, Time.deltaTime);
    }

    void OnEnable()
    {
        PlayerController.OnAnyPlayerDied += HandlePlayerDied;
        PlayerController.OnAnyPlayerRespawned += HandlePlayerRespawned;
    }

    void OnDisable()
    {
        PlayerController.OnAnyPlayerDied -= HandlePlayerDied;
        PlayerController.OnAnyPlayerRespawned -= HandlePlayerRespawned;
    }

    void HandlePlayerDied()
    {
        _pendingRespawnCount++;
        if (gameObjectLeftKillZone != null)
        {
            gameObjectLeftKillZone.SetActive(false);
        }
    }

    void HandlePlayerRespawned()
    {
        _pendingRespawnCount--;
        if (_pendingRespawnCount <= 0)
        {
            _pendingRespawnCount = 0;
            RestoreAutoMove();
            StartCoroutine(EnumReEnableKillZone());
        }
    }

    IEnumerator EnumReEnableKillZone()
    {
        // wait 5 physics steps so overlap resolution completes before kill zone is active again
        for (int i = 0; i < 5; i++)
        {
            yield return new WaitForFixedUpdate();
        }
        if (gameObjectLeftKillZone != null)
        {
            gameObjectLeftKillZone.SetActive(true);
        }
    }

    // saves isAutoMove state and disables it — call when both players die
    public void DisableAutoMoveTemporarily()
    {
        _savedAutoMoveState = isAutoMove;
        isAutoMove = false;
        _isAutoMoveTemporarilyDisabled = true;
    }

    // restores isAutoMove to whatever it was before DisableAutoMoveTemporarily — no-op if never disabled
    public void RestoreAutoMove()
    {
        if (!_isAutoMoveTemporarilyDisabled)
        {
            return;
        }
        isAutoMove = _savedAutoMoveState;
        _isAutoMoveTemporarilyDisabled = false;
    }

    // instantly snaps camera to save point position and clears SmoothDamp velocity
    public void SnapToPosition(Transform targetTransform)
    {
        transform.position = new Vector3(targetTransform.position.x, targetTransform.position.y, transform.position.z);
        _velocityVec = Vector3.zero;
    }

} // end of class
