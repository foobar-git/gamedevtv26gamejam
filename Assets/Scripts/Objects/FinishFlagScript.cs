using UnityEngine;
using UnityEngine.SceneManagement;

// Finish flag — first player to touch it freezes in place, second player restarts the scene.
public class FinishFlagScript : MonoBehaviour
{
    private int _playersFinished = 0;

    ////////////////////////////////////////////////////////////////////////////////////////

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.gameObject.CompareTag(TagScript.PLAYER_TAG))
        {
            return;
        }
        PlayerController playerController = other.GetComponentInParent<PlayerController>();
        if (playerController == null || playerController.IsPlayerDead)
        {
            return;
        }
        _playersFinished++;
        playerController.FreezeAtFinish();
        if (_playersFinished >= 2)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

} // end of class
