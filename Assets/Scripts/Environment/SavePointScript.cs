using UnityEngine;

// Triggers a waving animation on the save point flag when a player is standing in its zone.
public class SavePointScript : MonoBehaviour
{
    private Animator _animator;

    void Awake()
    {
        _animator = GetComponentInChildren<Animator>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag(TagScript.PLAYER_TAG))
        {
            Debug.Log("SavePoint Flag");
            _animator.SetBool("flagWavingAnimParam", true);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.CompareTag(TagScript.PLAYER_TAG))
        {
            _animator.SetBool("flagWavingAnimParam", false);
        }
    }

} // end of class
