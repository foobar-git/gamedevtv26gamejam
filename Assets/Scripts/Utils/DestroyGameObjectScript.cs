using UnityEngine;

// Destroys any object that enters this trigger zone, except the player.
// Used for off-screen cleanup boxes (projectiles, pickups, enemies).
public class DestroyGameObjectScript : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.gameObject.CompareTag(TagScript.PLAYER_TAG))
        {
            Destroy(other.gameObject);
        }
    }

} // end of class
