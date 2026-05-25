using UnityEngine;

// Generic projectile spawned by EnemyScript. Velocity and direction are applied by
// the spawning enemy at instantiation — this script handles gravity, sizing, lifetime,
// and hit detection (player damage, ground destruction).
public class ProjectileScript : MonoBehaviour
{
    // how strongly gravity pulls this projectile — 0 = straight line, 1 = natural arc/drop
    [SerializeField] private float gravityScale = 1f;
    // uniform scale multiplier applied at spawn — lets one prefab serve big and small variants
    [SerializeField] private float sizeScale = 1f;
    // auto-destroy after this many seconds — safety net for projectiles that miss everything
    [SerializeField] private float lifetime = 5f;
    // optional — spawned at the projectile's position when it is destroyed on impact
    [SerializeField] private GameObject gameObjectHitEffectPrefab;

    private Rigidbody2D _projectileRb;

    void Awake()
    {
        _projectileRb = GetComponent<Rigidbody2D>();
        if (_projectileRb != null)
        {
            // enemy sets linear velocity at spawn; gravity scale controls whether the
            // path curves (stone drops) or stays flat (horizontal bolt)
            _projectileRb.gravityScale = gravityScale;
        }
        // multiply the prefab's existing scale — preserves artist-set proportions at any size
        transform.localScale *= sizeScale;
        // if lifetime is 0, shooting is intentionally unlimited; skip the auto-destroy
        if (lifetime > 0f)
        {
            Destroy(gameObject, lifetime);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag(TagScript.PLAYER_TAG))
        {
            // GetComponentInParent because the player tag may be on a child collider object
            PlayerController pc = other.GetComponentInParent<PlayerController>();
            if (pc != null)
            {
                pc.TakeHit();
            }
            DestroyProjectile();
        }
        else if (other.gameObject.CompareTag("Ground"))
        {
            // hit solid ground — no damage, just shatter and disappear
            DestroyProjectile();
        }
    }

    void DestroyProjectile()
    {
        // spawn hit effect before Destroy so it exists in the scene after this object is gone
        if (gameObjectHitEffectPrefab != null)
        {
            Instantiate(gameObjectHitEffectPrefab, transform.position, Quaternion.identity);
        }
        Destroy(gameObject);
    }

} // end of class
