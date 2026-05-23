using UnityEngine;
using UnityEngine.InputSystem;

public struct PlayerCharacterInput
{
    public float horizontal;
    public bool jumpPressed;
    public bool shootPressed;
}

public class InputProvider : MonoBehaviour
{
    public static InputProvider Instance { get; private set; }

    public bool isSwapped { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        Keyboard kb = Keyboard.current;
        if (kb == null) return;

        if (kb.spaceKey.wasPressedThisFrame)
            isSwapped = !isSwapped;
    }

    public PlayerCharacterInput GetRedInput()
    {
        if (Keyboard.current == null) return default;
        return isSwapped ? ReadBlueKeys() : ReadRedKeys();
    }

    public PlayerCharacterInput GetBlueInput()
    {
        if (Keyboard.current == null) return default;
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
