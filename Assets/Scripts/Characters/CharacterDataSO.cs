using UnityEngine;

[CreateAssetMenu(menuName = "Game/Character Data")]
public class CharacterDataSO : ScriptableObject
{
    [Header("Movement")]
    public float moveSpeed = 4f;
    public float jumpForce = 11f;
    public float swimSpeed = 2f;
    public float swimForce = 3f;
    public float gravityScale = 2f;
    public float inWaterGravityScale = 0.5f;

    [Header("Combat")]
    public float stunBounceForce = 3f;

    [Header("Scales")]
    public Vector2 scaleSmall = new Vector2(0.6f, 0.6f);
    public Vector2 scaleNormal = new Vector2(1f, 1f);
    public Vector2 scaleLarge = new Vector2(2f, 2f);
}
