using UnityEngine;

// Centralizes all Unity tag strings used across scripts — avoids hardcoded string literals.
public class TagScript : MonoBehaviour {

    // TAGS ////////////////////////////////////////////////////////////////

    public const string MAIN_CAMERA_TAG = "MainCamera";
    public const string PLAYER_TAG = "Player";
    public const string GROUND_TAG = "Ground";
    public const string WATER_TAG = "Water";
    public const string ENEMY_TAG = "Enemy";
    public const string TURN_ENEMY_TAG = "TurnEnemy";
    public const string CAN_STUN_ENEMY_TAG = "CanStunEnemy";
    public const string CANNOT_STUN_ENEMY_TAG = "CannotStunEnemy";
    public const string FIRE_BALL_TAG = "FireBall";
    public const string PICKUP_COIN_TAG = "PickupCoin";
    public const string PICKUP_FIRE_FLOWER_TAG = "PickupFireFlower";
    public const string PICKUP_MUSHROOM_TAG = "PickupMushroom";
    public const string PICKUP_1UP_TAG = "Pickup1up";
    public const string BRICK_TAG = "Brick";
    public const string START_POINT_TAG = "StartPoint";
    public const string SAVE_POINT_TAG = "SavePoint";
    public const string KILL_BOX_TAG = "KillBox";
    public const string DESTROY_GAME_OBJECT_BOX_TAG = "DestroyGameObjectBox";

} // end of class
