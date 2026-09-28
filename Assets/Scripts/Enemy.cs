using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

// Base das sombras. Subclasses definem para onde querem ir (Desired);
// a base cuida de desviar de paredes, dano, morte e drops.
public abstract class Enemy : MonoBehaviour
{
    public static readonly List<Enemy> All = new List<Enemy>();

    public float speed = 2.5f;
    public int hp = 1;
    public int scoreValue = 5;

    protected PlayerController player;
    protected Rigidbody2D rb;
    SpriteRenderer sr;
    Vector2 knockback;
    bool dead;

    static readonly RaycastHit2D[] hits = new RaycastHit2D[8];
    static readonly float[] steerAngles = { 0f, 35f, -35f, 70f, -70f, 110f, -110f };

    protected abstract Color BodyColor { get; }
    protected abstract string SpriteKey { get; }

    // Para onde a sombra quer ir (posição no mundo).
    protected abstract Vector2 Desired();

    public static T Create<T>(Vector3 pos, PlayerController target, float speed, int hp) where T : Enemy
    {
        var go = Bootstrap.NewObject(typeof(T).Name);
        go.transform.position = pos;

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sortingOrder = 1;
        go.AddComponent<SpriteAnimator>().fps = 7f;

        var rb = go.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.mass = 0.5f;

        go.AddComponent<CircleCollider2D>().radius = 0.4f;

        var e = go.AddComponent<T>();
        e.player = target;
        e.speed = speed;
        e.hp = hp;
        return e;
    }

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
    }

    protected virtual void Start()
    {
        GetComponent<SpriteAnimator>().Play(SpriteFactory.Shade(SpriteKey, BodyColor, new Color(1f, 0.9f, 0.3f)));
        transform.localScale = Vector3.one * (hp > 1 ? 1.3f : 1f);
    }

    void OnEnable() { All.Add(this); }
    void OnDisable() { All.Remove(this); }

    void FixedUpdate()
    {
        var gm = GameManager.Instance;
        if (player == null || gm == null || gm.State != GameState.Playing || !player.Alive)
        {
            rb.SetVelocity(Vector2.zero);
            return;
        }

        Vector2 pos = rb.position;
        Vector2 dir = Desired() - pos;
        if (dir.sqrMagnitude > 0.0001f) dir = Steer(pos, dir.normalized);

        rb.SetVelocity(dir * speed + knockback);
        knockback = Vector2.MoveTowards(knockback, Vector2.zero, 30f * Time.fixedDeltaTime);
    }

    // Desvio de obstáculos: testa ângulos ao redor da direção desejada até achar caminho livre.
    Vector2 Steer(Vector2 pos, Vector2 dir)
    {
        var filter = ContactFilter2D.noFilter;
        foreach (float angle in steerAngles)
        {
            Vector2 d = Quaternion.Euler(0f, 0f, angle) * dir;
            if (!WallAhead(pos, d, filter)) return d;
        }
        return dir;
    }

    bool WallAhead(Vector2 pos, Vector2 dir, ContactFilter2D filter)
    {
        int count = Physics2D.CircleCast(pos, 0.35f, dir, filter, hits, 1.2f);
        for (int i = 0; i < count; i++)
        {
            if (hits[i].collider is TilemapCollider2D) return true;
        }
        return false;
    }

    public void Knockback(Vector2 force)
    {
        knockback = force;
    }

    public void Hit(int damage)
    {
        if (dead) return;
        hp -= damage;
        sr.color = new Color(1f, 0.5f, 0.5f);
        Invoke(nameof(ResetColor), 0.06f);
        if (player != null) Knockback(((Vector2)(transform.position - player.transform.position)).normalized * 5f);
        if (hp <= 0) Die();
    }

    void ResetColor()
    {
        sr.color = Color.white;
    }

    public void Die()
    {
        if (dead) return;
        dead = true;
        All.Remove(this);

        Fx.Burst(transform.position, BodyColor + new Color(0.2f, 0.2f, 0.2f, 0f), 1.8f);
        CameraFollow.Instance?.Shake(0.08f);
        GameManager.Instance.OnEnemyKilled(scoreValue);

        // Recompensa: gema sempre, coração às vezes.
        Pickup.Spawn(Pickup.Kind.Gem, transform.position);
        if (Random.value < 0.07f) Pickup.Spawn(Pickup.Kind.Heart, transform.position + new Vector3(0.4f, 0.3f, 0f));

        Destroy(gameObject);
    }
}
