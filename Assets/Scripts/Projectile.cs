using UnityEngine;
using UnityEngine.Tilemaps;

// Faísca disparada por Pavio. Some ao bater em parede ou Breu.
public class Projectile : MonoBehaviour
{
    const float Speed = 15f;
    const float Lifetime = 1.1f;

    float life;
    bool done;

    public static void Spawn(Vector3 pos, Vector2 dir)
    {
        var go = Bootstrap.NewObject("Faisca");
        go.transform.position = pos;

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sortingOrder = Fx.Order - 1;
        SpriteFactory.UseUnlit(sr);
        go.AddComponent<SpriteAnimator>().Play("itens/faisca");

        var rb = go.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.SetVelocity(dir * Speed);

        var col = go.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.18f;

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
        Fx.Play("itens/impacto", transform.position);
        Destroy(gameObject);
    }
}
