using UnityEngine;
using UnityEngine.Tilemaps;

// Faísca disparada pelo jogador. Some ao bater em parede ou sombra.
public class Projectile : MonoBehaviour
{
    const float Speed = 15f;
    const float Lifetime = 1.1f;

    float life;
    bool done;

    public static void Spawn(Vector3 pos, Vector2 dir)
    {
        var go = Bootstrap.NewObject("Spark");
        go.transform.position = pos;
        go.transform.localScale = Vector3.one * 0.3f;

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = SpriteFactory.Circle(Color.white);
        sr.color = new Color(1f, 0.85f, 0.4f);
        sr.sortingOrder = 3;

        var rb = go.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.SetVelocity(dir * Speed);

        var col = go.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.5f;

        go.AddComponent<Projectile>();
    }

    void Update()
    {
        life += Time.deltaTime;
        if (life >= Lifetime) Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (done) return;
        var enemy = other.GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.Hit(1);
            Explode();
        }
        else if (other is TilemapCollider2D)
        {
            Explode();
        }
    }

    void Explode()
    {
        done = true;
        Fx.Burst(transform.position, new Color(1f, 0.8f, 0.3f, 0.7f), 0.8f);
        Destroy(gameObject);
    }
}
