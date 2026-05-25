using System.Collections.Generic;
using UnityEngine;

// Interactable world object — switches its SpriteAnimatorScript between state 0 (default)
// and state 1 (activated) when a player in range presses the shoot button.
public class InteractableScript : MonoBehaviour
{
    [SerializeField] private SpriteAnimatorScript spriteAnimatorScript;
    // false = one-way activation (stays on state 1); true = shoot toggles back and forth
    [SerializeField] private bool isToggle = false;

    private List<PlayerController> _playerControllerList = new List<PlayerController>();
    private bool _isActivated = false;
    private bool _wasShootPressed = false;

    ////////////////////////////////////////////////////////////////////////////////////////

    void Update()
    {
        CheckInteraction();
    }

    void CheckInteraction()
    {
        if (_playerControllerList.Count == 0)
        {
            // reset so a fresh entry doesn't carry over a stale press
            _wasShootPressed = false;
            return;
        }

        bool isShootPressed = IsAnyPlayerPressingShoot();

        // rising edge only — one press triggers, holding does not repeat
        if (isShootPressed && !_wasShootPressed)
        {
            if (isToggle)
            {
                _isActivated = !_isActivated;
            }
            else
            {
                _isActivated = true;
            }
            if (spriteAnimatorScript != null)
            {
                spriteAnimatorScript.activeStateIndex = _isActivated ? 1 : 0;
            }
        }

        _wasShootPressed = isShootPressed;
    }

    bool IsAnyPlayerPressingShoot()
    {
        if (InputProvider.Instance == null)
        {
            return false;
        }
        foreach (PlayerController playerController in _playerControllerList)
        {
            PlayerCharacterInput playerCharacterInput = playerController.assignedPlayerCharacter == PlayerController.PlayerCharacter.Red
                ? InputProvider.Instance.GetRedInput()
                : InputProvider.Instance.GetBlueInput();
            if (playerCharacterInput.shootPressed)
            {
                return true;
            }
        }
        return false;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        PlayerController playerController = other.GetComponentInParent<PlayerController>();
        if (playerController != null && !_playerControllerList.Contains(playerController))
        {
            _playerControllerList.Add(playerController);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        PlayerController playerController = other.GetComponentInParent<PlayerController>();
        if (playerController != null)
        {
            _playerControllerList.Remove(playerController);
        }
    }

} // end of class
