using UnityEngine;

public class CameraAutoMoveTriggerScript : MonoBehaviour
{
    [Header("Camera automove actions")]
    [SerializeField] private bool enableOnEnter = true;
    [SerializeField] private bool disableOnEnter;
    [Tooltip("Overrides the camera's auto-scroll speed. 0 = no change.")]
    [SerializeField] private float newAutoMoveSpeed;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.gameObject.CompareTag(TagScript.PLAYER_TAG))
        {
            return;
        }
        if (GameManager.Instance == null || GameManager.Instance.cameraScript == null)
        {
            return;
        }

        CameraScript cam = GameManager.Instance.cameraScript;

        if (newAutoMoveSpeed > 0f)
        {
            cam.SetAutoMoveSpeed(newAutoMoveSpeed);
        }
        if (enableOnEnter)
        {
            cam.EnableAutoMove();
        }
        if (disableOnEnter)
        {
            cam.DisableAutoMove();
        }
    }
}
