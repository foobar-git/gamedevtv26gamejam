using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent ( typeof (Camera) )]
public class CameraScript : MonoBehaviour {

    public float cameraSmoothTime, cameraZoom, maxZoom, minZoom, zoomLimiter;
    
    public Vector3 offset;
    private Vector3 _cameraCenterPoint, _velocityVec;
    private Bounds _moveBounds, _zoomBounds;
    private bool _hasSnapped;

    public List<Transform> cameraTargetList = new List<Transform>();
    private Camera _mainCamera;

    ////////////////////////////////////////////////////////////////////////////////////////

    void Awake() {
        _mainCamera = GetComponent<Camera> ();
        offset = new Vector3 ( 0f, 0f, -10f);
        cameraSmoothTime = 0.5f;
        minZoom = 10f;
        maxZoom = 5f;
        zoomLimiter = 30f;
    }

	void LateUpdate () {
        if (!_hasSnapped && cameraTargetList.Count > 0)
        {
            _cameraCenterPoint = GetCameraCenterPoint ();
            transform.position = _cameraCenterPoint + offset;
            _hasSnapped = true;
        }
        MoveCamera ();
        ZoomCamera ();
	}

    public void AddCameraTarget (Transform player) {
        if ( player !=null ) cameraTargetList.Add ( player.transform );
	}

    public void RemoveCameraTarget (Transform player) {
        cameraTargetList.Remove ( player.transform );
	}

    Vector3 GetCameraCenterPoint () {
        if ( cameraTargetList.Count == 1 ) {
            return cameraTargetList[0].position;
		}
        _moveBounds = new Bounds ( cameraTargetList[0].position, Vector3.zero );
        for ( int i = 0; i < cameraTargetList.Count; i++ ) {
            _moveBounds.Encapsulate ( cameraTargetList[i].position );
		}
        return _moveBounds.center;
	}

    float GetMaxPlayerDistance () {
        _zoomBounds = new Bounds ( cameraTargetList[0].position, Vector3.zero );
        for ( int i = 0; i < cameraTargetList.Count; i++ ) {
            _zoomBounds.Encapsulate ( cameraTargetList[i].position );
		}
        return _zoomBounds.size.x;
	}

    void MoveCamera () {
        if ( cameraTargetList.Count <= 0 ) return;
		_cameraCenterPoint = GetCameraCenterPoint ();
        //transform.position = _cameraCenterPoint + offset;
        transform.position = Vector3.SmoothDamp ( transform.position, _cameraCenterPoint + offset, ref _velocityVec, cameraSmoothTime);
    }

    void ZoomCamera () {
        if ( cameraTargetList.Count <= 0 ) return;
        cameraZoom = Mathf.Lerp ( maxZoom, minZoom, GetMaxPlayerDistance () / zoomLimiter );
        _mainCamera.orthographicSize = Mathf.Lerp ( _mainCamera.orthographicSize, cameraZoom, Time.deltaTime );
	}

} //end of class
