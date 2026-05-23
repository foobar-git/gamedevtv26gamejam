using UnityEngine;
using UnityEngine.InputSystem;

public struct HandInput
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

    public HandInput GetLeftHand()
    {
        if (Keyboard.current == null) return default;
        return isSwapped ? ReadRightKeys() : ReadLeftKeys();
    }

    public HandInput GetRightHand()
    {
        if (Keyboard.current == null) return default;
        return isSwapped ? ReadLeftKeys() : ReadRightKeys();
    }

    private HandInput ReadLeftKeys()
    {
        Keyboard kb = Keyboard.current;
        HandInput input;
        input.horizontal = (kb.aKey.isPressed ? -1 : 0) + (kb.dKey.isPressed ? 1 : 0);
        input.jumpPressed = kb.wKey.wasPressedThisFrame;
        input.shootPressed = kb.sKey.wasPressedThisFrame;
        return input;
    }

    private HandInput ReadRightKeys()
    {
        Keyboard kb = Keyboard.current;
        HandInput input;
        input.horizontal = (kb.leftArrowKey.isPressed ? -1 : 0) + (kb.rightArrowKey.isPressed ? 1 : 0);
        input.jumpPressed = kb.upArrowKey.wasPressedThisFrame;
        input.shootPressed = kb.downArrowKey.wasPressedThisFrame;
        return input;
    }
}
