using UnityEngine;

// Script-based sprite frame animator — no Unity Animator component required.
// Drop on any GameObject. Assign animation states in the Inspector (frames + fps per state).
// External scripts set activeStateIndex to switch between animations.
public class SpriteAnimatorScript : MonoBehaviour
{
    [System.Serializable]
    public class SpriteAnimationState
    {
        public string name = "Animation";
        public Sprite[] frames;
        public float fps = 8f;
    }

    [SerializeField] private SpriteAnimationState[] spriteAnimationStates;
    [SerializeField] private SpriteRenderer spriteRenderer;

    [HideInInspector] public int activeStateIndex = 0;

    private Sprite[] _activeFrames;
    private int      _frameIndex;
    private float    _frameTimer;

    ////////////////////////////////////////////////////////////////////////////////////////

    void LateUpdate()
    {
        AdvanceAnimation();
    }

    void AdvanceAnimation()
    {
        if (spriteRenderer == null || spriteAnimationStates == null || spriteAnimationStates.Length == 0)
        {
            return;
        }

        int clampedIndex = Mathf.Clamp(activeStateIndex, 0, spriteAnimationStates.Length - 1);
        SpriteAnimationState spriteAnimationState = spriteAnimationStates[clampedIndex];

        if (spriteAnimationState.frames == null || spriteAnimationState.frames.Length == 0)
        {
            return;
        }

        // array reference comparison — if the array changed, the state changed; reset to frame 0
        if (spriteAnimationState.frames != _activeFrames)
        {
            _activeFrames = spriteAnimationState.frames;
            _frameIndex = 0;
            _frameTimer = 0f;
        }

        _frameTimer += Time.deltaTime;
        float frameDuration = 1f / Mathf.Max(spriteAnimationState.fps, 0.01f);

        if (_frameTimer >= frameDuration)
        {
            // subtract rather than reset — carries over overflow so timing stays consistent
            _frameTimer -= frameDuration;
            int next = _frameIndex + 1;
            _frameIndex = (next >= spriteAnimationState.frames.Length) ? 0 : next;
        }

        spriteRenderer.sprite = spriteAnimationState.frames[_frameIndex];
    }

} // end of class
