using UnityEngine;
using UnityEngine.InputSystem;

// Centralizes keyboard input for both player characters. Supports mid-game control
// swap so Red reads Blue's keys and vice versa — toggled with spacebar.
public struct PlayerCharacterInput
{
    public float horizontal;
    public bool jumpPressed;
    public bool shootPressed;
}

public class InputProvider : MonoBehaviour
{
    public static InputProvider Instance { get; private set; }

    // when true, GetRedInput returns Blue's keys and GetBlueInput returns Red's —
    // PlayerController never needs to know a swap happened
    public bool isSwapped { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        // persist across scene loads so input is never lost during level transitions
        DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        CheckSwapInput();
    }

    private void CheckSwapInput()
    {
        Keyboard kb = Keyboard.current;
        if (kb == null)
        {
            return;
        }

        if (kb.spaceKey.wasPressedThisFrame)
        {
            isSwapped = !isSwapped;
        }
    }

    public PlayerCharacterInput GetRedInput()
    {
        if (Keyboard.current == null)
        {
            return default; // safe no-input state when no keyboard is present
        }
        return isSwapped ? ReadBlueKeys() : ReadRedKeys();
    }

    public PlayerCharacterInput GetBlueInput()
    {
        if (Keyboard.current == null)
        {
            return default; // safe no-input state when no keyboard is present
        }
        return isSwapped ? ReadRedKeys() : ReadBlueKeys();
    }

    private PlayerCharacterInput ReadRedKeys()
    {
        Keyboard kb = Keyboard.current;
        PlayerCharacterInput input;
        input.horizontal = (kb.aKey.isPressed ? -1 : 0) + (kb.dKey.isPressed ? 1 : 0);
        input.jumpPressed = kb.wKey.wasPressedThisFrame;
        input.shootPressed = kb.sKey.wasPressedThisFrame;
        return input;
    }

    private PlayerCharacterInput ReadBlueKeys()
    {
        Keyboard kb = Keyboard.current;
        PlayerCharacterInput input;
        input.horizontal = (kb.leftArrowKey.isPressed ? -1 : 0) + (kb.rightArrowKey.isPressed ? 1 : 0);
        input.jumpPressed = kb.upArrowKey.wasPressedThisFrame;
        input.shootPressed = kb.downArrowKey.wasPressedThisFrame;
        return input;
    }
}
