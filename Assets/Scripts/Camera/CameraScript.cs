using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent ( typeof (Camera) )]
public class CameraScript : MonoBehaviour {

    public float cameraSmoothTime, cameraZoom, maxZoom, minZoom, zoomLimiter;
    
    public Vector3 offset;
    private Vector3 cameraCenterPoint, velocity;
    private Bounds moveBounds, zoomBounds;
    private bool hasSnapped;

    public List<Transform> cameraTargetList = new List<Transform>();
    private Camera mainCamera;

    ////////////////////////////////////////////////////////////////////////////////////////

    void Awake() {
        mainCamera = GetComponent<Camera> ();
        offset = new Vector3 ( 0f, 0f, -10f);
        cameraSmoothTime = 0.5f;
        minZoom = 10f;
        maxZoom = 5f;
        zoomLimiter = 30f;
    }

	void LateUpdate () {
        if (!hasSnapped && cameraTargetList.Count > 0)
        {
            cameraCenterPoint = GetCameraCenterPoint ();
            transform.position = cameraCenterPoint + offset;
            hasSnapped = true;
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
        moveBounds = new Bounds ( cameraTargetList[0].position, Vector3.zero );
        for ( int i = 0; i < cameraTargetList.Count; i++ ) {
            moveBounds.Encapsulate ( cameraTargetList[i].position );
		}
        return moveBounds.center;
	}

    float GetMaxPlayerDistance () {
        zoomBounds = new Bounds ( cameraTargetList[0].position, Vector3.zero );
        for ( int i = 0; i < cameraTargetList.Count; i++ ) {
            zoomBounds.Encapsulate ( cameraTargetList[i].position );
		}
        return zoomBounds.size.x;
	}

    void MoveCamera () {
        if ( cameraTargetList.Count <= 0 ) return;
		cameraCenterPoint = GetCameraCenterPoint ();
        //transform.position = cameraCenterPoint + offset;
        transform.position = Vector3.SmoothDamp ( transform.position, cameraCenterPoint + offset, ref velocity, cameraSmoothTime);
    }

    void ZoomCamera () {
        if ( cameraTargetList.Count <= 0 ) return;
        cameraZoom = Mathf.Lerp ( maxZoom, minZoom, GetMaxPlayerDistance () / zoomLimiter );
        mainCamera.orthographicSize = Mathf.Lerp ( mainCamera.orthographicSize, cameraZoom, Time.deltaTime );
	}

} //end of class
