using UnityEngine;

// Trigger zone — any player entering it enables the camera's auto-scroll.
public class CameraAutoMoveTrigger : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.gameObject.CompareTag(TagScript.PLAYER_TAG))
        {
            return;
        }
        if (GameManager.Instance != null && GameManager.Instance.cameraScript != null)
        {
            GameManager.Instance.cameraScript.EnableAutoMove();
        }
    }
}
