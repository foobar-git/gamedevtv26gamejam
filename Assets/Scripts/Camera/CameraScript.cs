using System.Collections.Generic;
using UnityEngine;

// Follows both players with smooth-damped position and orthographic zoom that
// expands as the players move apart. Snaps to center on the first frame.
[RequireComponent(typeof(Camera))]
public class CameraScript : MonoBehaviour
{
    public float cameraSmoothTime, cameraZoom, maxZoom, minZoom, zoomLimiter;

    public Vector3 offset;
    private Vector3 _cameraCenterPoint, _velocityVec;
    private Bounds _moveBounds, _zoomBounds;
    // prevents SmoothDamp from sliding in from the editor position on frame one
    private bool _hasSnapped;

    public List<Transform> cameraTargetList = new List<Transform>();
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

    void MoveCamera()
    {
        if (cameraTargetList.Count <= 0)
        {
            return;
        }
        _cameraCenterPoint = GetCameraCenterPoint();
        transform.position = Vector3.SmoothDamp(transform.position, _cameraCenterPoint + offset, ref _velocityVec, cameraSmoothTime);
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

} // end of class
